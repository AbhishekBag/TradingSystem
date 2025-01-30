using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TradingSystem.Models;
using TradingSystem.Processors;

namespace Svc.TradingService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TradingController : ControllerBase
    {
        private readonly TradeActions _tradeActions;

        public TradingController(TradeActions tradeActions)
        {
            _tradeActions = tradeActions;
        }

        [HttpGet(Name = "GetTradeDetails")]
        public IEnumerable<Trade> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new Trade
            {
                BuyerOrderId = Random.Shared.Next(1, 100),
                SellerOrderId = Random.Shared.Next(1, 100),
                StockSymbol = "MSFT",
                Price = Random.Shared.Next(100, 200),
                Quantity = Random.Shared.Next(1, 100),
                TradeTimestamp = DateTime.Now,
                TradeType = OrderType.Buy
            })
            .ToArray();
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [HttpPost("PlaceOrder")]
        public async Task<ActionResult<int>> Post(Order order)
        {
            if (order == null)
            {
                return NotFound();
            }

            int orderId = await _tradeActions.PlaceOrderAsync(order);
            return Ok(orderId);
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [HttpPatch("ModifyOrder")]
        public async Task<ActionResult<bool>> Patch(int orderId, int quantity, int price)
        {
            if (orderId < 0 || quantity < 0 || price < 0)
            {
                return NotFound();
            }

            var isSuccess = await _tradeActions.ModifyOrderAsync(orderId, quantity, price);
            return Ok(isSuccess);
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [HttpDelete("CancelOrder")]
        public async Task<ActionResult<bool>> Cancel(int orderId)
        {
            if (orderId < 0)
            {
                return NotFound();
            }

            var isSuccess = await _tradeActions.CancelOrderAsync(orderId);
            return Ok(isSuccess);
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [HttpGet("QueryOrder")]
        public async Task<ActionResult<Order>> Get(int orderId)
        {
            if (orderId < 0)
            {
                return NotFound();
            }

            try
            {
                var order = await _tradeActions.QueryOrderAsync(orderId);
                return Ok(order);
            }
            catch (Exception ex)
            {
                return NotFound();

            }
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [HttpGet("GetActiveOrders")]
        public async Task<ActionResult<List<Order>>> GetActieOrders()
        {
            var orders = await _tradeActions.GetActiveOrders();
            return Ok(orders);
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [HttpGet("GetAllOrders")]
        public async Task<ActionResult<List<Order>>> GetAllOrders()
        {
            var orders = await _tradeActions.GetAllOrders();
            return Ok(orders);
        }
    }
}
