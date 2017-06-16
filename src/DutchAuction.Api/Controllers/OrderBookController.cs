using System.Linq;
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
        private readonly IAuctionManager _auctionManager;

        public OrderbookController(IAuctionManager auctionManager)
        {
            _auctionManager = auctionManager;
        }

        /// <summary>
        /// Get order book
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        [ProducesResponseType(typeof(OrderbookResponse), (int)HttpStatusCode.OK)]
        public OrderbookResponse Get()
        {
            var orderbook = _auctionManager.GetOrderbook();

            return new OrderbookResponse
            {
                Price = orderbook.LkkPriceChf,
                InMoneyVolume = orderbook.InMoneyVolumeLkk,
                OutOfTheMoneyVolume = orderbook.OutOfTheMoneyVolumeLkk,
                InMoneyOrders = orderbook.InMoneyOrders
                    .Select(Map),
                OutOfTheMoneyOrders = orderbook.OutOfMoneyOrders
                    .Select(Map)
            };
        }

        private static OrderbookResponse.Order Map(Order order)
        {
            return new OrderbookResponse.Order
            {
                Price = order.Price,
                Investors = order.Investors,
                Volume = order.Volume
            };
        }
    }
}