using DutchAuction.Core.Domain.Auction;

namespace DutchAuction.Core.Services.Auction
{
    public interface IOrderbookService
    {
        Orderbook Render();
        void OnBidAdded(string clientId, string assetId, double price, double volume);
        void OnBidPriceSet(string clientId, double price);
        void OnBidAssetVolumeSet(string clientId, string assetId, double volume);
    }
}