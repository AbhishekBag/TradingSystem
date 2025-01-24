using System.Collections.Concurrent;
using TradingSystem.Models;

namespace TradingSystem.DataAccess
{
    public class TradeStore
    {
        private static readonly Lazy<TradeStore> lazyInstance = new Lazy<TradeStore>(() => new TradeStore());

        public static TradeStore Instance => lazyInstance.Value;

        public ConcurrentDictionary<int, Trade> Trades { get; } = new ConcurrentDictionary<int, Trade>();

        private TradeStore()
        {
        }
    }
}
