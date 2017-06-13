using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using DutchAuction.Core.Domain.Auction;

namespace DutchAuction.Repositories.Lots
{
    public class BidsRepository : IBidsRepository
    {
        private readonly INoSQLTableStorage<BidEntity> _tableStorage;

        public BidsRepository(INoSQLTableStorage<BidEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task AddAsync(IBid bid)
        {
            var entity = BidEntity.Create(bid);

            await _tableStorage.InsertAsync(entity);
        }

        public async Task<IEnumerable<IBid>> GetAllAsync()
        {
            return (await _tableStorage.GetDataAsync()).Select(Bid.Create);
        }
    }
}
