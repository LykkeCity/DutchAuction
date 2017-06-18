using System;
using DutchAuction.Api.Models;
using DutchAuction.Core.Services.Auction;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.PlatformAbstractions;

namespace DutchAuction.Api.Controllers
{
    /// <summary>
    /// Controller to test service is alive
    /// </summary>
    [Route("api/[controller]")]
    public class IsAliveController : Controller
    {
        private readonly IAuctionManager _auctionManager;
        private readonly IAuctionEventsManager _auctionEventsManager;

        public IsAliveController(IAuctionManager auctionManager, IAuctionEventsManager auctionEventsManager)
        {
            _auctionManager = auctionManager;
            _auctionEventsManager = auctionEventsManager;
        }

        /// <summary>
        /// Checks service is alive
        /// </summary>
        [HttpGet]
        public IsAliveResponse Get()
        {
            var orderbook = _auctionManager.GetOrderbook();

            return new IsAliveResponse
            {
                Version = PlatformServices.Default.Application.ApplicationVersion,
                Env = Environment.GetEnvironmentVariable("ENV_INFO"),
                AuctionEventsPersistQueueLength = _auctionEventsManager.AuctionEventsPersistQueueLength,
                LastOrderbookRenderDuration = orderbook.RenderDuration,
                LastOrderbookBidsCount = orderbook.BidsCount,
            };
        }
    }
}