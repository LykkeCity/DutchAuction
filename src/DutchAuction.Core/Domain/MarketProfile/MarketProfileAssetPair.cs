using System;

namespace DutchAuction.Core.Domain.MarketProfile
{
    public class MarketProfileAssetPair
    {
        public string AssetPair { get; set; }

        public double BidPrice { get; set; }

        public double AskPrice { get; set; }

        public DateTime BidPriceTimestamp { get; set; }

        public DateTime AskPriceTimestamp { get; set; }
    }
}