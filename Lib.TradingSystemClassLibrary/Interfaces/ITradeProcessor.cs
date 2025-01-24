using TradingSystem.Models;

namespace TradingSystem.Interfaces
{
    public interface ITradeProcessor
    {
        public Task<int> PlaceOrder(int userId, OrderType orderType, string stockSymbol, int quantity, decimal price);

        public Task<bool> ModifyOrder(int orderId, int quantity, decimal price);

        public Task<bool> CancelOrder(int orderId);

        public Task<Order> QueryOrder(int orderId);
    }
}
