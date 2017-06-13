using System;
using System.ComponentModel.DataAnnotations;
using DutchAuction.Core;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Linq;

namespace DutchAuction.Api.Models
{
    /// <summary>
    /// Start client's bidding
    /// </summary>
    public class StartBiddingModel
    {
        /// <summary>
        /// Client ID
        /// </summary>
        [Required]
        public string ClientId { get; set; }

        /// <summary>
        /// Asset ID (CHF, USD...)
        /// </summary>
        [Required]
        public string AssetId { get; set; }

        /// <summary>
        /// Volume of given asset
        /// </summary>
        [Required]
        public double Volume { get; set; }

        /// <summary>
        /// Bid price in CHF
        /// </summary>
        [Required]
        public double Price { get; set; }

        /// <summary>
        /// Bid date
        /// </summary>
        [Required]
        public DateTime Date { get; set; }

        public void Validate(ModelStateDictionary modelState, ApplicationSettings.DutchAuctionSettings settings)
        {
            if (string.IsNullOrEmpty(ClientId))
            {
                modelState.AddModelError(nameof(ClientId), "Client is required");
            }

            if (string.IsNullOrEmpty(AssetId))
            {
                modelState.AddModelError(nameof(AssetId), "Asset is required");
            }

            if (!settings.Assets.Contains(AssetId))
            {
                modelState.AddModelError(nameof(AssetId), "Not allowed Asset");
            }

            if (Price <= 0)
            {
                modelState.AddModelError(nameof(Price), "Price should be positive number");
            }

            if (Volume <= 0)
            {
                modelState.AddModelError(nameof(Volume), "Volume should be positive number");
            }
        }
    }
}
