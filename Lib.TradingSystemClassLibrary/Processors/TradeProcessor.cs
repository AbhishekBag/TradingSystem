﻿using TradingSystem.Models;

namespace TradingSystem.Processors
{
    public class TradeProcessor
    {
        private readonly InMemoryDataStore _dataStore;
        private int _orderIdCounter = 1;
        private int _tradeIdCounter = 1;
        private readonly object _lock = new object();

        public TradeProcessor(InMemoryDataStore dataStore)
        {
            _dataStore = dataStore;
        }

        public int PlaceOrder(int userId, OrderType orderType, string stockSymbol, int quantity, decimal price)
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
                Status = OrderStatus.Accepted
            };

            _dataStore.Orders[order.OrderId] = order;
            if (!_dataStore.OrderBook.ContainsKey(stockSymbol))
            {
                _dataStore.OrderBook[stockSymbol] = new List<Order>();
            }
            _dataStore.OrderBook[stockSymbol].Add(order);

            ExecuteTrades(stockSymbol);

            return order.OrderId;
        }

        public bool ModifyOrder(int orderId, int quantity, decimal price)
        {
            if (_dataStore.Orders.TryGetValue(orderId, out var order))
            {
                lock (_lock)
                {
                    order.Quantity = quantity;
                    order.Price = price;
                    ExecuteTrades(order.StockSymbol);
                }
                return true;
            }
            return false;
        }

        public bool CancelOrder(int orderId)
        {
            if (_dataStore.Orders.TryGetValue(orderId, out var order))
            {
                lock (_lock)
                {
                    order.Status = OrderStatus.Canceled;
                    _dataStore.OrderBook[order.StockSymbol].Remove(order);
                }
                return true;
            }
            return false;
        }

        public Order QueryOrder(int orderId)
        {
            _dataStore.Orders.TryGetValue(orderId, out var order);
            return order;
        }

        private void ExecuteTrades(string stockSymbol)
        {
            var orders = _dataStore.OrderBook[stockSymbol];
            var buyOrders = orders.Where(o => o.OrderType == OrderType.Buy && o.Status == OrderStatus.Accepted).OrderBy(o => o.OrderAcceptedTimestamp).ToList();
            var sellOrders = orders.Where(o => o.OrderType == OrderType.Sell && o.Status == OrderStatus.Accepted).OrderBy(o => o.OrderAcceptedTimestamp).ToList();

            foreach (var buyOrder in buyOrders)
            {
                foreach (var sellOrder in sellOrders)
                {
                    if (buyOrder.Price >= sellOrder.Price)
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

                        _dataStore.Trades[trade.TradeId] = trade;

                        buyOrder.Quantity -= tradeQuantity;
                        sellOrder.Quantity -= tradeQuantity;

                        if (buyOrder.Quantity == 0)
                        {
                            buyOrder.Status = OrderStatus.Canceled;
                        }

                        if (sellOrder.Quantity == 0)
                        {
                            sellOrder.Status = OrderStatus.Canceled;
                        }

                        if (buyOrder.Quantity == 0 || sellOrder.Quantity == 0)
                        {
                            break;
                        }
                    }
                }
            }

            _dataStore.OrderBook[stockSymbol] = orders.Where(o => o.Status == OrderStatus.Accepted).ToList();
        }
    }
}
