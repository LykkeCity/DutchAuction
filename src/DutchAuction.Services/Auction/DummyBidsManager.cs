using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DutchAuction.Core.Domain.Auction;
using DutchAuction.Core.Services.Auction;

namespace DutchAuction.Services.Auction
{
    internal class DummyBidsManager : IBidsManager
    {
        public void Start()
        {
        }

        public void Add(IBid bid)
        {
        }

        public Task<IEnumerable<IBid>> GetAllAsync()
        {
            return Task.FromResult(Enumerable.Empty<IBid>());
        }
    }
}