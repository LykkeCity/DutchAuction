using System;
using System.ComponentModel.DataAnnotations;

namespace DutchAuction.Api.Models
{
    /// <summary>
    /// Auction lot
    /// </summary>
    public class AuctionLotModel
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
        /// Lot volume
        /// </summary>
        [Required]
        public double Volume { get; set; }
        /// <summary>
        /// Lot price
        /// </summary>
        [Required]
        public double Price { get; set; }
        /// <summary>
        /// Lot date
        /// </summary>
        public DateTime Date { get; set; }
    }
}
