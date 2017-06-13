using DutchAuction.Core.Domain.Auction;
using DutchAuction.Core.Services.Auction;
using Microsoft.AspNetCore.Mvc;

namespace DutchAuction.Api.Controllers
{
    /// <summary>
    /// Controller for order book
    /// </summary>
    [Route("api/[controller]")]
    public class OrderBookController : Controller
    {
        private readonly IOrderbookService _orderbookService;

        public OrderBookController(IOrderbookService orderbookService)
        {
            _orderbookService = orderbookService;
        }

        /// <summary>
        /// Get order book
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public Order[] Get()
        {
            return _orderbookService.Render();
        }
    }
}