using System.Collections.Generic;
using DutchAuction.Core.Domain.Auction;

namespace DutchAuction.Api.Models
{
    public class BidResponse
    {
        public string ClientId { get; set; }
        public double Price { get; set; }
        public IReadOnlyDictionary<string, double> AssetVolumes { get; set; }
        public BidState State { get; set; }
        public IReadOnlyDictionary<string, double> InMoneyAssetVolumes { get; set; }
    }
}