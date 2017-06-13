using System.Collections.Generic;
using System.Threading.Tasks;

namespace DutchAuction.Core.Domain.Auction
{
    public interface IBidsRepository
    {
        Task AddAsync(IBid bid);
        Task<IEnumerable<IBid>> GetAllAsync();
    }
}