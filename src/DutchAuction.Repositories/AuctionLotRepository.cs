using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using DutchAuction.Core.Domain;
using DutchAuction.Repositories.Entities;

namespace DutchAuction.Repositories
{
    public class AuctionLotRepository : IAuctionLotRepository
    {
        private readonly INoSQLTableStorage<AuctionLotEntity> _tableStorage;

        public AuctionLotRepository(INoSQLTableStorage<AuctionLotEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task AddAsync(IAuctionLot lot)
        {
            await _tableStorage.InsertOrReplaceAsync(AuctionLotEntity.Create(lot));
        }

        public async Task<IEnumerable<IAuctionLot>> GetAllAsync()
        {
            return (await _tableStorage.GetDataAsync()).Select(AuctionLot.Create);
        }
    }
}
