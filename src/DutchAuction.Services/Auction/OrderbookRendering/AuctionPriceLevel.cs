namespace DutchAuction.Services.Auction.OrderbookRendering
{
    internal class AuctionPriceLevel
    {
        public double PriceChf { get; set; }
        public BidCalculation[] BidCalculations { get; set; }
    }
}