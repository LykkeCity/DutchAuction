using System;
using System.Collections.Generic;
using System.Threading;
using DutchAuction.Core.Domain.Lots;
using DutchAuction.Core.Services.Lots;

namespace DutchAuction.Services.Lots
{
    public class AuctionLotCacheService : 
        IAuctionLotCacheService,
        IDisposable
    {
        private List<IAuctionLot> _lots = new List<IAuctionLot>();
        private readonly ReaderWriterLockSlim _lockSlim = new ReaderWriterLockSlim();

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
            _lockSlim.EnterReadLock();

            try
            {
                return _lots.ToArray();
            }
            finally
            {
                _lockSlim.ExitReadLock();
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _lockSlim?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
