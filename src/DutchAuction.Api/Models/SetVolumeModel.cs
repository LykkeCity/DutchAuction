using System;
using System.ComponentModel.DataAnnotations;

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
    }
}