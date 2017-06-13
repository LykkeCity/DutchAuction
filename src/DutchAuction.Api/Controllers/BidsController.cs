using System.Net;
using DutchAuction.Api.Models;
using DutchAuction.Core;
using DutchAuction.Core.Services.Auction;
using Microsoft.AspNetCore.Mvc;

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
    }
}
