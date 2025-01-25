using System.Collections.Concurrent;
using TradingSystem.Models;

namespace TradingSystem.DataAccess
{
    public class OrderStore
    {
        private static readonly Lazy<OrderStore> lazyInstance = new Lazy<OrderStore>(() => new OrderStore());

        public static OrderStore Instance => lazyInstance.Value;

        private readonly ConcurrentDictionary<int, Order> Orders = new ConcurrentDictionary<int, Order>();

        // StockSymbol -> OrderType -> PriorityQueue<Order>
        private readonly ConcurrentDictionary<string, Dictionary<OrderType, PriorityQueue<Order, int>>> OrderCollection = new ConcurrentDictionary<string, Dictionary<OrderType, PriorityQueue<Order, int>>>();

        private OrderStore()
        {
        }

        public async Task<bool> AddOrderCollection(string stockSymbol, Dictionary<OrderType, PriorityQueue<Order, int>> orderCollection)
        {
            return await Task.Run(() =>
            {
                return OrderCollection.TryAdd(stockSymbol, orderCollection);
            });
        }

        public async Task<bool> AddOrder(int orderId, Order order)
        {
            bool addedToOrders = Orders.TryAdd(orderId, order);

            var orderQueues = OrderCollection.GetOrAdd(order.StockSymbol, CreateOrderQueues());

            lock (orderQueues)
            {
                orderQueues[order.OrderType].Enqueue(order, order.Price);
            }

            return await Task.FromResult(addedToOrders);
        }

        public async Task<IEnumerable<string>> GetOrderCollectionKeys()
        {
            return await Task.FromResult(OrderCollection.Keys);
        }

        public async Task<Order?> GetOrder(int orderId)
        {
            Orders.TryGetValue(orderId, out var order);
            return await Task.FromResult(order);
        }

        public async Task<Dictionary<OrderType, PriorityQueue<Order, int>>?> GetOrderCollectionBySymbol(string symbol)
        {
            OrderCollection.TryGetValue(symbol, out var orderDict);
            return await Task.FromResult(orderDict);
        }

        private Dictionary<OrderType, PriorityQueue<Order, int>> CreateOrderQueues()
        {
            return new Dictionary<OrderType, PriorityQueue<Order, int>>
            {
                [OrderType.Buy] = new PriorityQueue<Order, int>(Comparer<int>.Create((o1, o2) => o2.CompareTo(o1))),
                [OrderType.Sell] = new PriorityQueue<Order, int>(Comparer<int>.Create((o1, o2) => o1.CompareTo(o2)))
            };
        }
    }
}
