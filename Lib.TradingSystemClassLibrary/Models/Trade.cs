namespace TradingSystem.Models
{
    public class Trade
    {
        public int TradeId { get; set; }
        
        public OrderType TradeType { get; set; }
        
        public int BuyerOrderId { get; set; }
        
        public int SellerOrderId { get; set; }
        
        public string StockSymbol { get; set; }
        
        public int Quantity { get; set; }
        
        public decimal Price { get; set; }
        
        public DateTime TradeTimestamp { get; set; }
    }
}
