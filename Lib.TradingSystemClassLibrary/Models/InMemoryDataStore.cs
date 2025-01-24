using System.Collections.Concurrent;
using TradingSystem.DataAccess;

namespace TradingSystem.Models
{
    public class InMemoryDataStore
    {
        public UserStore UserStore { get; set; } = UserStore.Instance;

        public OrderStore OrderStore { get; set; } = OrderStore.Instance;

        public TradeStore TradeStore { get; set; } = TradeStore.Instance;

        public ConcurrentDictionary<string, List<Order>> OrderBook { get; } = new ConcurrentDictionary<string, List<Order>>();
    }
}
