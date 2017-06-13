using System.Collections.Generic;
using System.Threading.Tasks;

namespace DutchAuction.Core.Domain.Auction
{
    public interface IAuctionEventsRepository
    {
        Task AddAsync(IAuctionEvent auctionEvent);
        Task<IEnumerable<IAuctionEvent>> GetAllAsync();
    }
}