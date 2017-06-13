namespace DutchAuction.Core.Services.Auction
{
    public enum AuctionOperationResult
    {
        Ok,
        AccountAlreadyExist,
        AccountNotFound,
        PriceIsLessThanCurrent,
        VolumeIsLessThanCurrent
    }
}