using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using DutchAuction.Core;
using DutchAuction.Core.Domain.Auction;
using DutchAuction.Core.Services.Auction;

namespace DutchAuction.Services.Auction
{
    public class BidsManager :
        IBidsManager,
        IDisposable
    {
        private readonly IBidsRepository _repository;
        private readonly ConcurrentQueue<IBid> _persistQueue;
        private readonly ILog _log;

        private CancellationTokenSource _persistQueuePumpingCancellationTokenSource;

        public BidsManager(
            IBidsRepository bidsRepository,
            ILog log)
        {
            _repository = bidsRepository;
            _log = log;

            _persistQueue = new ConcurrentQueue<IBid>();
        }

        public void Start()
        {
            try
            {
                _persistQueuePumpingCancellationTokenSource = new CancellationTokenSource();
                Task.Factory.StartNew(async () => await PumpPersistQueueAsync(),
                    _persistQueuePumpingCancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                _log.WriteErrorAsync(Constants.ComponentName, null, null, ex).Wait();
                throw;
            }
        }

        public void Add(IBid bid)
        {
            _persistQueue.Enqueue(bid);
        }

        public async Task<IEnumerable<IBid>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        private async Task PumpPersistQueueAsync()
        {
            while (!_persistQueuePumpingCancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    IBid bid;

                    if (_persistQueue.TryPeek(out bid))
                    {
                        await _repository.AddAsync(bid);
                    }

                    _persistQueue.TryDequeue(out bid);
                }
                catch(Exception ex)
                {
                    try
                    {
                        await _log.WriteErrorAsync(Constants.ComponentName, null, null, ex);
                    }
                    // ReSharper disable once EmptyGeneralCatchClause
                    catch
                    {
                    }
                }

                Thread.Sleep(1);
            }
        }

        public void Dispose()
        {
            if (_persistQueuePumpingCancellationTokenSource != null)
            {
                _persistQueuePumpingCancellationTokenSource.Cancel();
                _persistQueuePumpingCancellationTokenSource.Dispose();
            }
        }
    }
}
