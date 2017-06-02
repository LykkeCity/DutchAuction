using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DutchAuction.Core;

namespace DutchAuction.Services
{
    public class AuctionLotCacheService : IAuctionLotCacheService
    {
        private List<IAuctionLot> _lots = new List<IAuctionLot>();
        private readonly ReaderWriterLockSlim _lockSlim = new ReaderWriterLockSlim();

        ~AuctionLotCacheService()
        {
            _lockSlim?.Dispose();
        }

        public Order[] GetOrderbook(string assetId = null)
        {
            _lockSlim.EnterReadLock();

            try
            {
                var query = _lots.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(assetId))
                {
                    query = query.Where(item => item.AssetId == assetId);
                }

                return query
                    .GroupBy(item => item.Price)
                    .Select(group => new Order
                    {
                        Investors = group.Select(item => item.ClientId).Distinct().Count(),
                        Price = group.Key,
                        Volume = group.Sum(item => item.Volume)
                    })
                    .OrderByDescending(order => order.Price)
                    .ToArray();
            }
            finally
            {
                _lockSlim.ExitReadLock();
            }
        }

        public void InitCache(List<IAuctionLot> lots)
        {
            _lockSlim.EnterWriteLock();

            try
            {
                _lots = lots;
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        public void Add(IAuctionLot lot)
        {
            _lockSlim.EnterWriteLock();

            try
            {
                _lots.Add(lot);
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        public IAuctionLot[] GetAllAsync()
        {
            _lockSlim.EnterWriteLock();

            try
            {
                return _lots.ToArray();
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }
    }
}
