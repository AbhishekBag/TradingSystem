using System.Collections.Concurrent;
using TradingSystem.Models;

namespace TradingSystem.DataAccess
{
    public class OrderStore
    {
        private static readonly Lazy<OrderStore> lazyInstance = new Lazy<OrderStore>(() => new OrderStore());

        public static OrderStore Instance => lazyInstance.Value;

        private readonly ConcurrentDictionary<int, Order> Orders = new ConcurrentDictionary<int, Order>();

        // Key: Stock Symbol, Value: Dictionary of OrderType and PriorityQueue of Orders
        private readonly ConcurrentDictionary<string, Dictionary<OrderType, PriorityQueue<Order, long>>> OrderCollections = new ConcurrentDictionary<string, Dictionary<OrderType, PriorityQueue<Order, long>>>();

        public Task<Order?> GetOrder(int orderId)
        {
            Orders.TryGetValue(orderId, out var order);
            return Task.FromResult(order);
        }

        public Task<List<Order>> GetOrders()
        {
            return Task.FromResult(Orders.Values.ToList());
        }

        public Task AddOrder(int orderId, Order order)
        {
            Orders[orderId] = order;
            return Task.CompletedTask;
        }

        public Task<bool> RemoveOrder(int orderId)
        {
            return Task.FromResult(Orders.TryRemove(orderId, out _));
        }

        public Task<Dictionary<OrderType, PriorityQueue<Order, long>>?> GetOrderCollectionBySymbol(string stockSymbol)
        {
            OrderCollections.TryGetValue(stockSymbol, out var orderCollection);
            return Task.FromResult(orderCollection);
        }

        public Task<List<string>> GetOrderCollectionKeys()
        {
            return Task.FromResult(OrderCollections.Keys.ToList());
        }

        public Task AddOrderCollection(string stockSymbol, Dictionary<OrderType, PriorityQueue<Order, long>> orderCollection)
        {
            OrderCollections[stockSymbol] = orderCollection;
            return Task.CompletedTask;
        }

        public Task RemoveOrderCollection(string stockSymbol)
        {
            return Task.FromResult(OrderCollections.TryRemove(stockSymbol, out _));
        }
    }
}
