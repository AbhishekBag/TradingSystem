namespace TradingSystem.Models
{
    public class Order
    {
        public int OrderId { get; set; }

        public int UserId { get; set; }

        public OrderType OrderType { get; set; }

        public string StockSymbol { get; set; }

        public int Quantity { get; set; }

        public int Price { get; set; }

        public DateTime OrderAcceptedTimestamp { get; set; }

        public DateTime ExpiryTimestamp { get; set; }

        public OrderStatus Status { get; set; }

        public Order(int orderId, int userId, OrderType orderType, string stockSymbol, int quantity, int price, DateTime orderAcceptedTimestamp, DateTime expiryTimestamp, OrderStatus status)
        {
            OrderId = orderId;
            UserId = userId;
            OrderType = orderType;
            StockSymbol = stockSymbol;
            Quantity = quantity;
            Price = price;
            OrderAcceptedTimestamp = orderAcceptedTimestamp;
            ExpiryTimestamp = expiryTimestamp;
            Status = status;
        }

        public Order(int userId, OrderType orderType, string stockSymbol, int quantity, int price)
        {
            UserId = userId;
            OrderType = orderType;
            StockSymbol = stockSymbol;
            Quantity = quantity;
            Price = price;
        }
    }
}
