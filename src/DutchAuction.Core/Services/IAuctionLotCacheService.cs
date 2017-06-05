using System.Collections.Generic;
using DutchAuction.Core.Domain;

namespace DutchAuction.Core.Services
{
    public interface IAuctionLotCacheService
    {
        IAuctionLot[] GetAllAsync();
        Order[] GetOrderbook();
        void InitCache(List<IAuctionLot> lots);
        void Add(IAuctionLot lot);
    }
}
