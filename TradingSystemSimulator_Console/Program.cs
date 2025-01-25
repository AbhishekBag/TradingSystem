using TradingSystem.Models;
using TradingSystem.Processors;
using TradingSystem.TradingSystemConfig;

Config config = new Config();

var dataStore = new InMemoryDataStore();
var tradeActions = new TradeActions(config);

// Add some dummy users
dataStore.UserStore.Users[1] = new User { UserId = 1, UserName = "Alice", PhoneNumber = "1234567890", EmailId = "alice@example.com" };
dataStore.UserStore.Users[2] = new User { UserId = 2, UserName = "Bob", PhoneNumber = "0987654321", EmailId = "bob@example.com" };

// Place some orders
var orderId1 = await tradeActions.PlaceOrderAsync(new Order(dataStore.UserStore.Users[1].UserId, OrderType.Buy, "RIL", 100, 2000));
var orderId2 = await tradeActions.PlaceOrderAsync(new Order(dataStore.UserStore.Users[2].UserId, OrderType.Buy, "RIL", 150, 2100));
var orderId3 = await tradeActions.PlaceOrderAsync(new Order(dataStore.UserStore.Users[2].UserId, OrderType.Sell, "HAL", 100, 2100));
var orderId4 = await tradeActions.PlaceOrderAsync(new Order(dataStore.UserStore.Users[2].UserId, OrderType.Buy, "HAL", 150, 2100));
var orderId5 = await tradeActions.PlaceOrderAsync(new Order(dataStore.UserStore.Users[2].UserId, OrderType.Sell, "RIL", 150, 1500));
var orderId6 = await tradeActions.PlaceOrderAsync(new Order(dataStore.UserStore.Users[2].UserId, OrderType.Sell, "RIL", 100, 2500));
var orderId7 = await tradeActions.PlaceOrderAsync(new Order(dataStore.UserStore.Users[2].UserId, OrderType.Sell, "RIL", 100, 3000));

// Query order status
var order1 = await tradeActions.QueryOrderAsync(orderId1);
var order2 = await tradeActions.QueryOrderAsync(orderId2);

Console.WriteLine($"Order 1: {order1.Status}, Quantity: {order1.Quantity}");
Console.WriteLine($"Order 2: {order2.Status}, Quantity: {order2.Quantity}");

// Modify an order
await tradeActions.ModifyOrderAsync(orderId1, 200, 1500);

// Cancel an order
await tradeActions.CancelOrderAsync(orderId2);

// Query order status again
order1 = await tradeActions.QueryOrderAsync(orderId1);
order2 = await tradeActions.QueryOrderAsync(orderId2);

Console.WriteLine($"Order 1: {order1.Status}, Quantity: {order1.Quantity}");
Console.WriteLine($"Order 2: {order2.Status}, Quantity: {order2.Quantity}");

// Additional test cases

// Place more orders
var orderId8 = await tradeActions.PlaceOrderAsync(new Order(dataStore.UserStore.Users[1].UserId, OrderType.Buy, "TCS", 50, 3000));
var orderId9 = await tradeActions.PlaceOrderAsync(new Order(dataStore.UserStore.Users[2].UserId, OrderType.Sell, "TCS", 50, 3000));
var orderId10 = await tradeActions.PlaceOrderAsync(new Order(dataStore.UserStore.Users[1].UserId, OrderType.Buy, "INFY", 200, 1500));
var orderId11 = await tradeActions.PlaceOrderAsync(new Order(dataStore.UserStore.Users[2].UserId, OrderType.Sell, "INFY", 200, 1500));

// Modify more orders
await tradeActions.ModifyOrderAsync(orderId3, 50, 2200);
await tradeActions.ModifyOrderAsync(orderId4, 100, 2000);

// Cancel more orders
await tradeActions.CancelOrderAsync(orderId5);
await tradeActions.CancelOrderAsync(orderId6);

// Query order status again
var order3 = await tradeActions.QueryOrderAsync(orderId3);
var order4 = await tradeActions.QueryOrderAsync(orderId4);
var order5 = await tradeActions.QueryOrderAsync(orderId5);
var order6 = await tradeActions.QueryOrderAsync(orderId6);

Console.WriteLine($"Order 3: {order3.Status}, Quantity: {order3.Quantity}");
Console.WriteLine($"Order 4: {order4.Status}, Quantity: {order4.Quantity}");
Console.WriteLine($"Order 5: {order5.Status}, Quantity: {order5.Quantity}");
Console.WriteLine($"Order 6: {order6.Status}, Quantity: {order6.Quantity}");

// Print all orders
Console.WriteLine($"Printing All Orders:");
var allOrders = await tradeActions.GetAllOrders();
PrintOrders(allOrders);

// Print active orders
Console.WriteLine($"Printing Active Orders:");
var activeOrders = await tradeActions.GetActiveOrders();
PrintOrders(activeOrders);

void PrintOrders(List<Order> orders)
{
    foreach (var order in orders)
    {
        Console.WriteLine($"Order ID: {order.OrderId}, User ID: {order.UserId}, Stock: {order.StockSymbol}, Quantity: {order.Quantity}, Price: {order.Price}, Status: {order.Status}");
    }
}
