using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DutchAuction.Core.Domain.Auction;

namespace DutchAuction.Api.Models
{
    public class BidResponse
    {
        [Required]
        public string ClientId { get; set; }

        [Required]
        public double Price { get; set; }

        [Required]
        public IReadOnlyCollection<KeyValuePair<string, double>> AssetVolumes { get; set; }

        [Required]
        [EnumDataType(typeof(BidState))]
        public BidState State { get; set; }

        [Required]
        public IReadOnlyCollection<KeyValuePair<string, double>> InMoneyAssetVolumes { get; set; }
    }
}