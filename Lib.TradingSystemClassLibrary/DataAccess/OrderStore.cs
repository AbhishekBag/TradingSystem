using System.Collections.Concurrent;
using TradingSystem.Models;

namespace TradingSystem.DataAccess
{
    public class OrderStore
    {
        private static readonly Lazy<OrderStore> lazyInstance = new Lazy<OrderStore>(() => new OrderStore());

        public static OrderStore Instance => lazyInstance.Value;

        private ConcurrentDictionary<int, Order> Orders { get; } = new ConcurrentDictionary<int, Order>();

        private OrderStore()
        {
        }

        public Task<bool> AddOrder(int orderId, Order order)
        {
            return Task.FromResult(Orders.TryAdd(orderId, order));
        }

        public Task<Order?> GetOrder(int orderId)
        {
            if (Orders != null)
            {
                return Task.FromResult(Orders.TryGetValue(orderId, out var order) ? order : null);
            }

            return Task.FromResult<Order?>(null);
        }
    }
}
