using System.Collections.Generic;

namespace DutchAuction.Core
{
    public interface IAuctionLotCacheService
    {
        IAuctionLot[] GetAllAsync();
        Order[] GetOrderbook(string assetId = null);
        void InitCache(List<IAuctionLot> lots);
        void Add(IAuctionLot lot);
    }
}
