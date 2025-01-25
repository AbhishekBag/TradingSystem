using TradingSystem.Interfaces;
using TradingSystem.Models;
using TradingSystem.TradingSystemConfig;

namespace TradingSystem.Processors
{
    public class TradeProcessor : ITradeProcessor
    {
        private readonly InMemoryDataStore dataStore;
        private int orderIdCounter = 0;
        private int tradeIdCounter = 0;
        private readonly object _lock = new object();
        private readonly SemaphoreSlim threadPool;
        private readonly Config config;

        public TradeProcessor(InMemoryDataStore dataStore, Config config)
        {
            this.dataStore = dataStore;
            this.config = config;

            int threadCount = config.NumberOfThreads;
            threadPool = new SemaphoreSlim(threadCount, threadCount);

            // Start the background task for monitoring expired orders
            var cancellationTokenSource = new CancellationTokenSource();
            _ = MonitorExpiredOrders(cancellationTokenSource.Token);
        }

        public async Task<int> PlaceOrder(int userId, OrderType orderType, string stockSymbol, int quantity, int price)
        {
            // Determine order expiry time
            var expiryDuration = TimeSpan.FromMinutes(config.OrderExpiryMinutes); // Configurable
            var expiryTimestamp = DateTime.UtcNow.Add(expiryDuration);

            // Create a new order
            var order = new Order
            {
                OrderId = Interlocked.Increment(ref orderIdCounter),
                UserId = userId,
                OrderType = orderType,
                StockSymbol = stockSymbol,
                Quantity = quantity,
                Price = price,
                OrderAcceptedTimestamp = DateTime.UtcNow,
                ExpiryTimestamp = expiryTimestamp, // Set expiry timestamp
                Status = (quantity == 0 || price == 0) ? OrderStatus.Canceled : OrderStatus.Accepted
            };

            // Add the order to the OrderStore
            await dataStore.OrderStore.AddOrder(order.OrderId, order);
            await UpdateOrderCollection(stockSymbol);

            var orderQueues = await dataStore.OrderStore.GetOrderCollectionBySymbol(stockSymbol);
            lock (orderQueues)
            {
                orderQueues[order.OrderType].Enqueue(order, order.Price);
            }

            // Process the order if it is accepted
            if (order.Status == OrderStatus.Accepted)
            {
                await RunInThreadPool(() => ExecuteTrades(order));
            }

            return order.OrderId;
        }

        private async Task UpdateOrderCollection(string stockSymbol)
        {
            // Add the order to the appropriate priority queue
            var orderCollection = await dataStore.OrderStore.GetOrderCollectionBySymbol(stockSymbol);
            if (orderCollection == null)
            {
                orderCollection = new Dictionary<OrderType, PriorityQueue<Order, int>>
                {
                    [OrderType.Buy] = new PriorityQueue<Order, int>(Comparer<int>.Create((o1, o2) =>
                        o2.CompareTo(o1) != 0 ? o2.CompareTo(o1) : o1)),
                    [OrderType.Sell] = new PriorityQueue<Order, int>(Comparer<int>.Create((o1, o2) =>
                        o1.CompareTo(o2) != 0 ? o1.CompareTo(o2) : o1))
                };

                await dataStore.OrderStore.AddOrderCollection(stockSymbol, orderCollection);
            }
        }

        public async Task<bool> ModifyOrder(int orderId, int quantity, int price)
        {
            Order? order = await dataStore.OrderStore.GetOrder(orderId);
            if (order != null && order.Status == OrderStatus.Accepted)
            {
                var orderQueues = await dataStore.OrderStore.GetOrderCollectionBySymbol(order.StockSymbol);
                if (orderQueues != null)
                {
                    lock (orderQueues)
                    {
                        // Remove the old order from the queue
                        if (orderQueues.TryGetValue(order.OrderType, out var queue))
                        {
                            var updatedQueue = new PriorityQueue<Order, int>(Comparer<int>.Default);
                            while (queue.TryDequeue(out var item, out var priority))
                            {
                                if (item.OrderId != order.OrderId)
                                {
                                    updatedQueue.Enqueue(item, priority);
                                }
                            }
                            orderQueues[order.OrderType] = updatedQueue;
                        }

                        // Update the order
                        order.Quantity = quantity;
                        order.Price = price;

                        // Re-add the updated order to the queue
                        if (order.Status != OrderStatus.Canceled)
                        {
                            orderQueues[order.OrderType].Enqueue(order, order.Price);
                        }
                    }

                    // Process the updated order
                    await RunInThreadPool(() => ExecuteTrades(order));

                    return true;
                }
            }

            return false;
        }

        public async Task<bool> CancelOrder(int orderId)
        {
            Order? order = await dataStore.OrderStore.GetOrder(orderId);
            if (order != null && order.Status == OrderStatus.Accepted)
            {
                var orderQueues = await dataStore.OrderStore.GetOrderCollectionBySymbol(order.StockSymbol);
                if (orderQueues != null)
                {
                    lock (orderQueues)
                    {
                        // Remove the order from the appropriate queue
                        if (orderQueues.TryGetValue(order.OrderType, out var queue))
                        {
                            var updatedQueue = new PriorityQueue<Order, int>(Comparer<int>.Default);
                            while (queue.TryDequeue(out var item, out var priority))
                            {
                                if (item.OrderId != order.OrderId)
                                {
                                    updatedQueue.Enqueue(item, priority);
                                }
                            }
                            orderQueues[order.OrderType] = updatedQueue;
                        }
                    }

                    // Update order status and clean up the OrderBook
                    lock (_lock)
                    {
                        order.Status = OrderStatus.Canceled;
                        dataStore.OrderBook[order.StockSymbol].Remove(order);
                    }

                    return true;
                }
            }

            return false;
        }

        public async Task<Order> QueryOrder(int orderId)
        {
            Order? order = await dataStore.OrderStore.GetOrder(orderId);
            if (order == null)
            {
                throw new InvalidOperationException($"Order with ID {orderId} not found.");
            }

            return order;
        }

        private async Task ExecuteTrades(Order order)
        {
            string stockSymbol = order.StockSymbol;

            var orderCollection = await dataStore.OrderStore.GetOrderCollectionBySymbol(stockSymbol);
            if (orderCollection == null)
            {
                return; // No orders to process
            }

            var buyQueue = orderCollection[OrderType.Buy];
            var sellQueue = orderCollection[OrderType.Sell];

            // Continue matching until there are no eligible orders
            while (buyQueue.TryPeek(out var buyOrder, out int buyPriority) &&
                   sellQueue.TryPeek(out var sellOrder, out int sellPriority))
            {
                // Skip expired orders
                if (buyOrder.ExpiryTimestamp <= DateTime.UtcNow || sellOrder.ExpiryTimestamp <= DateTime.UtcNow)
                {
                    if (buyOrder.ExpiryTimestamp <= DateTime.UtcNow)
                    {
                        buyQueue.Dequeue();
                        buyOrder.Status = OrderStatus.Expired;
                        dataStore.OrderBook[buyOrder.StockSymbol].Remove(buyOrder);
                    }

                    if (sellOrder.ExpiryTimestamp <= DateTime.UtcNow)
                    {
                        sellQueue.Dequeue();
                        sellOrder.Status = OrderStatus.Expired;
                        dataStore.OrderBook[sellOrder.StockSymbol].Remove(sellOrder);
                    }

                    continue;
                }

                // Match orders if Buy Price >= Sell Price
                if (buyOrder.Price >= sellOrder.Price)
                {
                    ProcessTrade(buyOrder, sellOrder, stockSymbol);

                    // Remove matched orders from the queues if their quantities are zero
                    if (buyOrder.Quantity == 0)
                    {
                        buyQueue.Dequeue();
                    }

                    if (sellOrder.Quantity == 0)
                    {
                        sellQueue.Dequeue();
                    }
                }
                else
                {
                    break; // No more matches possible
                }
            }
        }


        public async Task MonitorExpiredOrders(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var currentTime = DateTime.UtcNow;

                var orderCollectionKeys = await dataStore.OrderStore.GetOrderCollectionKeys();
                foreach (var stockSymbol in orderCollectionKeys)
                {
                    var orderCollection = await dataStore.OrderStore.GetOrderCollectionBySymbol(stockSymbol);
                    if (orderCollection == null) continue;

                    lock (orderCollection)
                    {
                        foreach (var orderType in orderCollection.Keys)
                        {
                            var queue = orderCollection[orderType];
                            var updatedQueue = new PriorityQueue<Order, int>(Comparer<int>.Default);

                            while (queue.TryDequeue(out var order, out var priority))
                            {
                                if (order.ExpiryTimestamp > currentTime && order.Status == OrderStatus.Accepted)
                                {
                                    updatedQueue.Enqueue(order, priority); // Re-add valid orders
                                }
                                else
                                {
                                    // Expire the order
                                    order.Status = OrderStatus.Expired;
                                    dataStore.OrderBook[order.StockSymbol].Remove(order);
                                }
                            }

                            orderCollection[orderType] = updatedQueue; // Replace with updated queue
                        }
                    }
                }

                // Sleep for a configurable duration before the next expiry check
                await Task.Delay(TimeSpan.FromSeconds(config.ExpiryCheckIntervalSeconds), cancellationToken);
            }
        }


        private void ProcessTrade(Order buyOrder, Order sellOrder, string stockSymbol)
        {
            // Determine trade quantity and price
            var tradeQuantity = Math.Min(buyOrder.Quantity, sellOrder.Quantity);
            var tradePrice = sellOrder.Price;

            // Create a new trade record
            var trade = new Trade
            {
                TradeId = Interlocked.Increment(ref tradeIdCounter),
                TradeType = OrderType.Buy,
                BuyerOrderId = buyOrder.OrderId,
                SellerOrderId = sellOrder.OrderId,
                StockSymbol = stockSymbol,
                Quantity = tradeQuantity,
                Price = tradePrice,
                TradeTimestamp = DateTime.UtcNow
            };

            // Update order quantities
            lock (_lock)
            {
                dataStore.TradeStore.Trades[trade.TradeId] = trade;

                buyOrder.Quantity -= tradeQuantity;
                sellOrder.Quantity -= tradeQuantity;

                // Update order status if fully matched
                if (buyOrder.Quantity == 0)
                {
                    buyOrder.Status = OrderStatus.Completed;
                }

                if (sellOrder.Quantity == 0)
                {
                    sellOrder.Status = OrderStatus.Completed;
                }
            }
        }

        private async Task RunInThreadPool(Func<Task> action)
        {
            await threadPool.WaitAsync();
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                threadPool.Release();
            }
        }
    }
}
