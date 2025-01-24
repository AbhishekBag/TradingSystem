using TradingSystem.Interfaces;
using TradingSystem.Models;

namespace TradingSystem.Processors
{
    public class TradeProcessor : ITradeProcessor
    {
        private readonly InMemoryDataStore _dataStore;
        private int _orderIdCounter = 0;
        private int _tradeIdCounter = 0;
        private readonly object _lock = new object();

        public TradeProcessor(InMemoryDataStore dataStore)
        {
            _dataStore = dataStore;
        }

        public async Task<int> PlaceOrder(int userId, OrderType orderType, string stockSymbol, int quantity, decimal price)
        {
            var order = new Order
            {
                OrderId = Interlocked.Increment(ref _orderIdCounter),
                UserId = userId,
                OrderType = orderType,
                StockSymbol = stockSymbol,
                Quantity = quantity,
                Price = price,
                OrderAcceptedTimestamp = DateTime.UtcNow,
                Status = quantity == 0 || price == 0 ? OrderStatus.Canceled : OrderStatus.Accepted
            };

            await _dataStore.OrderStore.AddOrder(order.OrderId, order);

            if (!_dataStore.OrderBook.ContainsKey(stockSymbol))
            {
                _dataStore.OrderBook[stockSymbol] = new List<Order>();
            }

            _dataStore.OrderBook[stockSymbol].Add(order);

            if (order.Status == OrderStatus.Accepted)
            {
                await ExecuteTrades(order);
            }

            return order.OrderId;
        }

        public async Task<bool> ModifyOrder(int orderId, int quantity, decimal price)
        {
            Order? order = await _dataStore.OrderStore.GetOrder(orderId);
            if (order != null)
            {
                order.Quantity = quantity;
                order.Price = price;
                await ExecuteTrades(order);

                return true;
            }

            return false;
        }

        public async Task<bool> CancelOrder(int orderId)
        {
            Order? order = await _dataStore.OrderStore.GetOrder(orderId);
            if (order != null)
            {
                var orderTypeDict = await _dataStore.OrderStore.GetOrderCollectionBySymbol(order.StockSymbol);
                lock (_lock)
                {
                    order.Status = OrderStatus.Canceled;
                    _dataStore.OrderBook[order.StockSymbol].Remove(order);
                    if (orderTypeDict != null)
                    {
                        if (orderTypeDict.TryGetValue(order.OrderType, out var orderIds))
                        {
                            orderIds.Remove(orderId);
                        }
                    }
                }
                return true;
            }

            return false;
        }

        public async Task<Order> QueryOrder(int orderId)
        {
            Order? order = await _dataStore.OrderStore.GetOrder(orderId);
            if (order == null)
            {
                throw new InvalidOperationException($"Order with ID {orderId} not found.");
            }

            return order;
        }

        private async Task ExecuteTrades(Order order)
        {
            string stockSymbol = order.StockSymbol;
            var orderCollection = await _dataStore.OrderStore.GetOrderCollectionBySymbol(stockSymbol);

            if (orderCollection == null)
            {
                return;
            }

            var buyOrders = GetOrders(orderCollection, OrderType.Buy);
            var sellOrders = GetOrders(orderCollection, OrderType.Sell);

            for (int i = 0; i < buyOrders.Count; i++)
            {
                for (int j = 0; j < sellOrders.Count; j++)
                {
                    var buyOrder = buyOrders[i];
                    var sellOrder = sellOrders[j];

                    if (buyOrder != null && sellOrder != null && buyOrder.Price >= sellOrder.Price)
                    {
                        await ProcessTrade(buyOrder, sellOrder, stockSymbol, i, j);
                    }
                }
            }

            _dataStore.OrderBook[stockSymbol] = buyOrders.Concat(sellOrders).Where(o => o != null && o.Status == OrderStatus.Accepted).Cast<Order>().ToList();
        }

        private List<Order> GetOrders(Dictionary<OrderType, List<int>> orderCollection, OrderType orderType)
        {
            var orders = new List<Order>();
            if (orderCollection.ContainsKey(orderType))
            {
                var buyOrdersTmp = orderCollection[orderType]
                    .Select(id => _dataStore.OrderStore.GetOrder(id).Result)
                    .Where(o => o != null && o.Status == OrderStatus.Accepted)
                    .OrderBy(o => o.OrderAcceptedTimestamp)
                    .ToList();

                orders = buyOrdersTmp == null ? orders : buyOrdersTmp;
            }

            return orders;
        }

        private async Task ProcessTrade(Order buyOrder, Order sellOrder, string stockSymbol, int buyIndex, int sellIndex)
        {
            var tradeQuantity = Math.Min(buyOrder.Quantity, sellOrder.Quantity);
            var tradePrice = sellOrder.Price;

            var trade = new Trade
            {
                TradeId = Interlocked.Increment(ref _tradeIdCounter),
                TradeType = OrderType.Buy,
                BuyerOrderId = buyOrder.OrderId,
                SellerOrderId = sellOrder.OrderId,
                StockSymbol = stockSymbol,
                Quantity = tradeQuantity,
                Price = tradePrice,
                TradeTimestamp = DateTime.UtcNow
            };

            var buyOrderTypeDict = await _dataStore.OrderStore.GetOrderCollectionBySymbol(buyOrder.StockSymbol);
            var sellOrderTypeDict = await _dataStore.OrderStore.GetOrderCollectionBySymbol(sellOrder.StockSymbol);
            lock (_lock)
            {
                _dataStore.TradeStore.Trades[trade.TradeId] = trade;

                buyOrder.Quantity -= tradeQuantity;
                sellOrder.Quantity -= tradeQuantity;

                if (buyOrder.Quantity == 0)
                {
                    buyOrder.Status = OrderStatus.Completed;
                    buyIndex++;

                    if (buyOrderTypeDict != null && buyOrderTypeDict.TryGetValue(OrderType.Buy, out var orderIds))
                    {
                        orderIds.Remove(buyOrder.OrderId);
                    }
                }

                if (sellOrder.Quantity == 0)
                {
                    sellOrder.Status = OrderStatus.Completed;
                    sellIndex++;
                    if (sellOrderTypeDict != null && sellOrderTypeDict.TryGetValue(OrderType.Sell, out var orderIds))
                    {
                        orderIds.Remove(sellOrder.OrderId);
                    }
                }
            }
        }
    }
}
