using System.Collections.Concurrent;
using TradingSystem.Models;

namespace TradingSystem.DataAccess
{
    public class OrderStore
    {
        private static readonly Lazy<OrderStore> lazyInstance = new Lazy<OrderStore>(() => new OrderStore());

        public static OrderStore Instance => lazyInstance.Value;

        private ConcurrentDictionary<int, Order> Orders { get; } = new ConcurrentDictionary<int, Order>();

        // StockSymbol -> OrderType -> List of OrderIds
        private ConcurrentDictionary<string, Dictionary<OrderType, List<int>>> OrderCollection { get; } = new ConcurrentDictionary<string, Dictionary<OrderType, List<int>>>();

        private OrderStore()
        {
        }

        public async Task<bool> AddOrder(int orderId, Order order)
        {
            // Add to Orders dictionary
            var addedToOrders = Orders.TryAdd(orderId, order);

            // Add to OrderCollection dictionary
            var orderDict = OrderCollection.GetOrAdd(order.StockSymbol, new Dictionary<OrderType, List<int>>());
            lock (orderDict)
            {
                if (!orderDict.TryGetValue(order.OrderType, out var orderList))
                {
                    orderList = new List<int>();
                    orderDict[order.OrderType] = orderList;
                }

                orderList.Add(orderId);
            }

            return await Task.FromResult(addedToOrders);
        }

        public async Task<Order?> GetOrder(int orderId)
        {
            if (Orders != null)
            {
                return await Task.FromResult(Orders.TryGetValue(orderId, out var order) ? order : null);
            }

            return await Task.FromResult<Order?>(null);
        }

        public async Task<Dictionary<OrderType, List<int>>?> GetOrderCollectionBySymbol(string symbol)
        {
            if (OrderCollection.TryGetValue(symbol, out var orderDict))
            {
                return await Task.FromResult(orderDict);
            }

            return await Task.FromResult<Dictionary<OrderType, List<int>>?>(null);
        }
    }
}
