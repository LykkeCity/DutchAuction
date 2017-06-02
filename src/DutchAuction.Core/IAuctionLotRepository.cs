using System.Collections.Generic;
using System.Threading.Tasks;

namespace DutchAuction.Core
{
    public interface IAuctionLotRepository
    {
        Task AddAsync(IAuctionLot lot);
        Task<IEnumerable<IAuctionLot>> GetAllAsync();
    }
}