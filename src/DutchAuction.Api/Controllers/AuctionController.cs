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
        private readonly IAuctionManager _auctionManager;

        public BidsController(
            ApplicationSettings.DutchAuctionSettings settings,
            IAuctionManager auctionManager,
            IOrderbookService orderbookService)
        {
            _auctionManager = auctionManager;
        }      

        /// <summary>
        /// Start client's bidding
        /// </summary>
        /// <param name="model">Model</param>
        /// <returns></returns>
        [HttpPost("")]
        public IActionResult StartBidding([FromBody]StartBiddingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(model);
            }

            _auctionManager.StartBidding(model.ClientId, model.AssetId, model.Price, model.Volume, model.Date);

            return Ok();
        }

        /// <summary>
        /// Update client's max price
        /// </summary>
        /// <param name="model">Model</param>
        /// <returns></returns>
        [HttpPut("price")]
        public IActionResult SetPrice([FromBody]SetPriceModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(model);
            }

            _auctionManager.AcceptPriceBid(model.ClientId, model.Price, model.Date);

            return Ok();
        }

        /// <summary>
        /// Set client's asset volume
        /// </summary>
        /// <param name="model">Model</param>
        /// <returns></returns>
        [HttpPut("volume")]
        public IActionResult SetVolume([FromBody]SetVolumeModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(model);
            }

            _auctionManager.AcceptVolumeBid(model.ClientId, model.AssetId, model.Volume, model.Date);

            return Ok();
        }
    }
}
