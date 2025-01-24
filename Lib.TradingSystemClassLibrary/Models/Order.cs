namespace TradingSystem.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        
        public int UserId { get; set; }
        
        public OrderType OrderType { get; set; }
        
        public string StockSymbol { get; set; }
        
        public int Quantity { get; set; }
        
        public decimal Price { get; set; }
        
        public DateTime OrderAcceptedTimestamp { get; set; }

        public OrderStatus Status { get; set; }
    }
}
