using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using DutchAuction.Core.Domain.Auction;

namespace DutchAuction.Core.Services.Auction
{
    public interface IAuctionEventsManager : IStartable
    {
        void Add(IAuctionEvent auctionEvent);
        Task<IEnumerable<IAuctionEvent>> GetAllAsync();
        int AuctionEventsPersistQueueLength { get; }
    }
}