namespace DutchAuction.Services.Auction.OrderbookRendering
{
    internal class RenderContext
    {
        public double AuctionVolumeChf { get; set; }
        public double PrevTestPriceLevelAuctionVolumeChf { get; set; }
        public double AuctionInMoneyVolumeLkk { get; set; }
        public double AuctionOutOfTheMoneyVolumeLkk { get; set; }
        public double LkkPriceChf { get; set; }
        public bool IsAllLotsSold { get; set; }
        public bool IsAutoFitPriceCase { get; set; }
    }
}