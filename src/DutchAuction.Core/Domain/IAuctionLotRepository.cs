using System.Collections.Generic;
using System.Threading.Tasks;

namespace DutchAuction.Core.Domain
{
    public interface IAuctionLotRepository
    {
        Task AddAsync(IAuctionLot lot);
        Task<IEnumerable<IAuctionLot>> GetAllAsync();
    }
}