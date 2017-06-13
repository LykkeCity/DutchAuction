using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DutchAuction.Core;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DutchAuction.Api.Models
{
    /// <summary>
    /// Set client's asset volume
    /// </summary>
    public class SetVolumeModel
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

            if (Volume <= 0)
            {
                modelState.AddModelError(nameof(Volume), "Volume should be positive number");
            }
        }
    }
}