using System.Collections.Generic;
using System.Threading.Tasks;

namespace DutchAuction.Core.Domain.Asset
{
    public interface IAssetPairsRepository
    {
        Task<IEnumerable<IAssetPair>> GetAllAsync();
    }
}