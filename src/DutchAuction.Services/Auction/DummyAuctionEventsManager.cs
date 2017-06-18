using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DutchAuction.Core.Domain.Auction;
using DutchAuction.Core.Services.Auction;

namespace DutchAuction.Services.Auction
{
    internal class DummyAuctionEventsManager : IAuctionEventsManager
    {
        public int AuctionEventsPersistQueueLength { get; } = 0;

        public void Start()
        {
        }

        public void Add(IAuctionEvent auctionEvent)
        {
        }

        public Task<IEnumerable<IAuctionEvent>> GetAllAsync()
        {
            return Task.FromResult(Enumerable.Empty<IAuctionEvent>());
        }
    }
}