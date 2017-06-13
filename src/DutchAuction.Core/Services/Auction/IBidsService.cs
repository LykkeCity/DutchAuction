namespace DutchAuction.Core.Services.Auction
{
    public interface IBidsService
    {
        AuctionOperationResult StartBidding(string clientId, string assetId, double price, double volume);
        AuctionOperationResult SetPrice(string clientId, double price);
        AuctionOperationResult SetAssetVolume(string clientId, string assetId, double volume);
    }
}