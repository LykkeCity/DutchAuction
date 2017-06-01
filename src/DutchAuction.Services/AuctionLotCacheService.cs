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
            _lockSlim.EnterWriteLock();

            try
            {
                var orders = new List<Order>();

                var prices = _lots.Select(item => item.Price).Distinct().OrderByDescending(item => item);

                foreach (var price in prices)
                {
                    var lots = _lots.Where(item => item.Price == price).ToList();

                    var order = new Order
                    {
                        Investors = lots.Select(item => item.ClientId).Distinct().Count(),
                        Price = price,
                        Volume = lots.Sum(item => item.Volume)
                    };

                    orders.Add(order);
                }

                return orders.ToArray();
            }
            finally
            {
                _lockSlim.ExitWriteLock();
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
