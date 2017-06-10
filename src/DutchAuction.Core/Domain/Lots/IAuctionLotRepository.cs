using System.Collections.Generic;
using System.Threading.Tasks;

namespace DutchAuction.Core.Domain.Lots
{
    public interface IAuctionLotRepository
    {
        Task AddAsync(IAuctionLot lot);
        Task<IEnumerable<IAuctionLot>> GetAllAsync();
    }
}