using System.Collections.Immutable;
using DutchAuction.Core.Domain.Auction;

namespace DutchAuction.Core.Services.Auction
{
    public interface IBidsService
    {
        IImmutableList<IClientBid> GetAll();
        IClientBid TryGetBid(string clientId);
        AuctionOperationResult StartBidding(string clientId, string assetId, double price, double volume);
        AuctionOperationResult SetPrice(string clientId, double price);
        AuctionOperationResult SetAssetVolume(string clientId, string assetId, double volume);
    }
}