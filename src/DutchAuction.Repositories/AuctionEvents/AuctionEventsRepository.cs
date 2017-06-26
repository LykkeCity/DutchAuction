using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using DutchAuction.Core.Domain.Auction;

namespace DutchAuction.Repositories.AuctionEvents
{
    public class AuctionEventsRepository : IAuctionEventsRepository
    {
        private readonly INoSQLTableStorage<AuctionEventEntity> _tableStorage;

        public AuctionEventsRepository(INoSQLTableStorage<AuctionEventEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task AddAsync(IAuctionEvent auctionEvent)
        {
            var entity = AuctionEventEntity.Create(auctionEvent);
            await _tableStorage.InsertAsync(entity);
        }

        public async Task<IEnumerable<IAuctionEvent>> GetAllAsync()
        {
            return (await _tableStorage.GetDataAsync()).Select(AuctionEvent.Create);
        }
    }
}
