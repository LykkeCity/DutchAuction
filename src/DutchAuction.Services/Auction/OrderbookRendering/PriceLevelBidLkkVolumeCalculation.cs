namespace DutchAuction.Services.Auction.OrderbookRendering
{
    internal class PriceLevelBidLkkVolumeCalculation
    {
        public AuctionPriceLevel PriceLevel { get; }
        public BidLkkVolumesCalculation[] BidLkkVolumeCalculations { get; }

        public PriceLevelBidLkkVolumeCalculation(AuctionPriceLevel priceLevel, BidLkkVolumesCalculation[] bidLkkVolumeCalculations)
        {
            PriceLevel = priceLevel;
            BidLkkVolumeCalculations = bidLkkVolumeCalculations;
        }
    }
}