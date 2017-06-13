namespace DutchAuction.Core.Services.Auction
{
    public enum AuctionOperationResult
    {
        Ok,
        ClientHasAlreadyDoneBid,
        BidNotFound,
        PriceIsLessThanCurrentBidPrice,
        VolumeIsLessThanCurrentBidAssetVolume
    }
}