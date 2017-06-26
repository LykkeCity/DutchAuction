using System;
using System.Net;
using DutchAuction.Api.Models;
using DutchAuction.Core;
using DutchAuction.Core.Domain.Auction;
using DutchAuction.Core.Services.Auction;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.SwaggerGen.Annotations;

namespace DutchAuction.Api.Controllers
{
    /// <summary>
    /// Controller for bids
    /// </summary>
    [Route("api/[controller]")]
    public class BidsController : Controller
    {
        private readonly ApplicationSettings.DutchAuctionSettings _settings;
        private readonly IAuctionManager _auctionManager;

        public BidsController(
            ApplicationSettings.DutchAuctionSettings settings,
            IAuctionManager auctionManager,
            IOrderbookService orderbookService)
        {
            _settings = settings;
            _auctionManager = auctionManager;
        }

#if DEBUG
        [HttpPost("testData/{bidsCount:int}")]
        public IActionResult GenerateTestBids(int bidsCount)
        {
            if (_auctionManager.GetOrderbook().BidsCount != 0)
            {
                return BadRequest("Clean auction events and restart service first");
            }

            var assets = new[] { "USD", "EUR", "CHF", "BTC" };
            var volumes = new[] { 10, 40, 50, 150 };
            var assetsCount = new[] { 1, 2, 3, 4 };

            for (var i = 0; i < bidsCount; ++i)
            {
                _auctionManager.StartBidding(
                    $"client_{i + 1}",
                    assets[i % assets.Length],
                    Math.Round(0.5 + i / (double)bidsCount, 3),
                    volumes[i % volumes.Length],
                    DateTime.UtcNow);

                for (var k = 0; k < assetsCount[i % assetsCount.Length]; ++k)
                {
                    _auctionManager.AcceptVolumeBid(
                        $"client_{i + 1}",
                        assets[(i + k) % assets.Length],
                        volumes[(i + k) % volumes.Length],
                        DateTime.UtcNow);
                }
            }

            return Ok();
        }
#endif

        [HttpGet("{clientId}")]
        [SwaggerOperation("GetBid")]
        [ProducesResponseType(typeof(BidResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        public IActionResult GetBid(string clientId)
        {
            if (string.IsNullOrEmpty(clientId))
            {
                return BadRequest(ErrorResponse.Create($"{nameof(clientId)} is required"));
            }

            var bid = _auctionManager.TryGetBid(clientId);
            if (bid == null)
            {
                return NotFound(ErrorResponse.Create("Bid not found"));
            }

            return Ok(new BidResponse
            {
                ClientId = bid.ClientId,
                LimitPriceChf = bid.LimitPriceChf,
                LkkPriceChf = bid.LkkPriceChf,
                AssetVolumes = bid.AssetVolumes,
                State = Map(bid.State),
                AssetVolumesLkk = bid.AssetVolumesLkk,
                InMoneyAssetVolumesLkk = bid.InMoneyAssetVolumesLkk
            });
        }

        /// <summary>
        /// Start client's bidding
        /// </summary>
        /// <param name="model">Model</param>
        /// <returns></returns>
        [HttpPost("")]
        [SwaggerOperation("StartBidding")]
        [ProducesResponseType(typeof(AuctionOperationResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public IActionResult StartBidding([FromBody]StartBiddingModel model)
        {
            model.Validate(ModelState, _settings);

            if (!ModelState.IsValid)
            {
                return BadRequest(ErrorResponse.Create(ModelState));
            }

            var result = _auctionManager.StartBidding(model.ClientId, model.AssetId, model.Price, model.Volume, model.Date);

            return Ok(AuctionOperationResponse.Create(result));
        }

        /// <summary>
        /// Update client's max price
        /// </summary>
        /// <param name="model">Model</param>
        /// <returns></returns>
        [HttpPut("price")]
        [SwaggerOperation("SetBidPrice")]
        [ProducesResponseType(typeof(AuctionOperationResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public IActionResult SetPrice([FromBody]SetPriceModel model)
        {
            model.Validate(ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ErrorResponse.Create(ModelState));
            }

            var result = _auctionManager.AcceptPriceBid(model.ClientId, model.Price, model.Date);

            return Ok(AuctionOperationResponse.Create(result));
        }

        /// <summary>
        /// Set client's asset volume
        /// </summary>
        /// <param name="model">Model</param>
        /// <returns></returns>
        [HttpPut("volume")]
        [SwaggerOperation("SetBidAssetVolume")]
        [ProducesResponseType(typeof(AuctionOperationResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public IActionResult SetVolume([FromBody]SetVolumeModel model)
        {
            model.Validate(ModelState, _settings);

            if (!ModelState.IsValid)
            {
                return BadRequest(ErrorResponse.Create(ModelState));
            }

            var result = _auctionManager.AcceptVolumeBid(model.ClientId, model.AssetId, model.Volume, model.Date);

            return Ok(AuctionOperationResponse.Create(result));
        }

        private Models.BidState Map(OrderbookBidState bidState)
        {
            switch (bidState)
            {
                case OrderbookBidState.InMoney:
                    return Models.BidState.InMoney;

                case OrderbookBidState.OutOfTheMoney:
                    return Models.BidState.OutOfTheMoney;

                case OrderbookBidState.PartiallyInMoney:
                    return Models.BidState.PartiallyInMoney;

                default:
                    throw new ArgumentOutOfRangeException(nameof(bidState), bidState, null);
            }
        }
    }
}
