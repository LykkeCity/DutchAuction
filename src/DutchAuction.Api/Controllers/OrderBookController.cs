using System.Net;
using DutchAuction.Api.Models;
using DutchAuction.Core.Domain.Auction;
using DutchAuction.Core.Services.Auction;
using Microsoft.AspNetCore.Mvc;

namespace DutchAuction.Api.Controllers
{
    /// <summary>
    /// Controller for order book
    /// </summary>
    [Route("api/[controller]")]
    public class OrderbookController : Controller
    {
        private readonly IOrderbookService _orderbookService;

        public OrderbookController(IOrderbookService orderbookService)
        {
            _orderbookService = orderbookService;
        }

        /// <summary>
        /// Get order book
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        [ProducesResponseType(typeof(OrderbookResponse), (int)HttpStatusCode.OK)]
        public Orderbook Get()
        {
            return _orderbookService.Render();
        }
    }
}