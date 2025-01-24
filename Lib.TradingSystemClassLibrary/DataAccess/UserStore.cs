using System.Collections.Concurrent;
using TradingSystem.Models;

namespace TradingSystem.DataAccess
{
    public class UserStore
    {
        private static readonly Lazy<UserStore> lazyInstance = new Lazy<UserStore>(() => new UserStore());

        public static UserStore Instance => lazyInstance.Value;

        public ConcurrentDictionary<int, User> Users { get; } = new ConcurrentDictionary<int, User>();

        private UserStore()
        {
        }
    }
}
