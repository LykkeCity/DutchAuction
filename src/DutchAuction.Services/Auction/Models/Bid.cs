using System.Collections.Generic;

namespace DutchAuction.Services.Auction.Models
{
    internal class Bid
    {
        public double Price { get; set; }
        public Dictionary<string, double> AssetVolumes { get; } = new Dictionary<string, double>();
    }
}