using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DutchAuction.Core.Domain.Auction;

namespace DutchAuction.Api.Models
{
    public class BidResponse
    {
        [Required]
        public string ClientId { get; set; }

        /// <summary>
        /// Client`s limit price in CHF
        /// </summary>
        [Required]
        public double LimitPriceChf { get; set; }

        /// <summary>
        /// LKK price in CHF, that bid volumes in LKK was calculated with
        /// </summary>
        [Required]
        public double LkkPriceChf { get; set; }

        [Required]
        public IEnumerable<KeyValuePair<string, double>> AssetVolumes { get; set; }

        [Required]
        [EnumDataType(typeof(BidState))]
        public BidState State { get; set; }

        [Required]
        public IEnumerable<KeyValuePair<string, double>> AssetVolumesLkk { get; set; }

        [Required]
        public IEnumerable<KeyValuePair<string, double>> InMoneyAssetVolumesLkk { get; set; }
    }
}