using TradingSystem.Interfaces;
using TradingSystem.Models;

namespace TradingSystem.Processors
{
    public class TradeActions
    {
        private readonly InMemoryDataStore inMemoryDataStore;
        private readonly ITradeProcessor tradeProcessor;

        public TradeActions()
        {
            inMemoryDataStore = new InMemoryDataStore();
            tradeProcessor = new TradeProcessor(inMemoryDataStore);
        }

        public async Task<int> PlaceOrderAsync(Order order)
        {
            return await tradeProcessor.PlaceOrder(order.UserId, order.OrderType, order.StockSymbol, order.Quantity, order.Price);
        }

        public async Task<bool> ModifyOrderAsync(int orderId, int quantity, decimal price)
        {
            return await tradeProcessor.ModifyOrder(orderId, quantity, price);
        }

        public async Task<bool> CancelOrderAsync(int orderId)
        {
            return await tradeProcessor.CancelOrder(orderId);
        }

        public async Task<Order> QueryOrderAsync(int orderId)
        {
            return await tradeProcessor.QueryOrder(orderId);
        }
    }
}
