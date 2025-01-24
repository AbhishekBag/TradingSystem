using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TradingSystem.Models;

namespace Svc.TradingService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TradingController : ControllerBase
    {

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
    }
}
