using System.Collections.Generic;
using DutchAuction.Core.Domain.Auction;

namespace DutchAuction.Core.Services.Auction
{
    public interface IBidsService
    {
        IBid[] GetAll();
        IBid TryGetBid(string clientId);
        AuctionOperationResult StartBidding(string clientId, string assetId, double price, double volume);
        AuctionOperationResult SetPrice(string clientId, double price);
        AuctionOperationResult SetAssetVolume(string clientId, string assetId, double volume);
        void MarkBidAsPartiallyInMoney(string clientId, IDictionary<string, double> inMoneyBidAssetVolumes);
        void MarkBidAsInMoney(string clientId);
        void MarkBidAsOutOfTheMoney(string clientId);
    }
}