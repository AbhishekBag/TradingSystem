using System.Collections.Concurrent;

namespace TradingSystem.Models
{
    public class InMemoryDataStore
    {
        public ConcurrentDictionary<int, User> Users { get; } = new ConcurrentDictionary<int, User>();
        
        public ConcurrentDictionary<int, Order> Orders { get; } = new ConcurrentDictionary<int, Order>();
        
        public ConcurrentDictionary<int, Trade> Trades { get; } = new ConcurrentDictionary<int, Trade>();
        
        public ConcurrentDictionary<string, List<Order>> OrderBook { get; } = new ConcurrentDictionary<string, List<Order>>();
    }
}
