using System;
using System.Net;
using DutchAuction.Api.Models;
using DutchAuction.Core;
using DutchAuction.Core.Domain.Auction;
using DutchAuction.Core.Services.Auction;
using Microsoft.AspNetCore.Mvc;
using BidState = DutchAuction.Api.Models.BidState;

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

        [HttpGet("{clientId}")]
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

        private BidState Map(OrderbookBidState bidState)
        {
            switch (bidState)
            {
                case OrderbookBidState.InMoney:
                    return BidState.InMoney;

                case OrderbookBidState.OutOfTheMoney:
                    return BidState.OutOfTheMoney;

                case OrderbookBidState.PartiallyInMoney:
                    return BidState.PartiallyInMoney;

                default:
                    throw new ArgumentOutOfRangeException(nameof(bidState), bidState, null);
            }
        }
    }
}
