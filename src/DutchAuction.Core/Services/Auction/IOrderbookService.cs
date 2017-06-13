using DutchAuction.Core.Domain.Auction;

namespace DutchAuction.Core.Services.Auction
{
    public interface IOrderbookService
    {
        Order[] Render();
        void OnClientAccountAdded(string clientId, string assetId, double price, double volume);
        void OnPriceSet(string clientId, double price);
        void OnAssetVolumeSet(string clientId, string assetId, double volume);
    }
}