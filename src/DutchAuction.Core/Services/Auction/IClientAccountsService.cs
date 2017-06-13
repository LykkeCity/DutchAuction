using System;

namespace DutchAuction.Core.Services.Auction
{
    public interface IClientAccountsService
    {
        void Add(string clientId, string assetId, double price, double volume);
        void SetPrice(string clientId, double price);
        void SetAssetVolume(string clientId, string assetId, double volume);
    }
}