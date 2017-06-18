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
    public class AuctionEventsManager :
        IAuctionEventsManager,
        IDisposable
    {
        public int AuctionEventsPersistQueueLength => _persistQueue.Count;

        private readonly IAuctionEventsRepository _repository;
        private readonly ConcurrentQueue<IAuctionEvent> _persistQueue;
        private readonly ILog _log;

        private CancellationTokenSource _persistQueuePumpingCancellationTokenSource;

        public AuctionEventsManager(
            IAuctionEventsRepository auctionEventsRepository,
            ILog log)
        {
            _repository = auctionEventsRepository;
            _log = log;

            _persistQueue = new ConcurrentQueue<IAuctionEvent>();
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

        public void Add(IAuctionEvent auctionEvent)
        {
            _persistQueue.Enqueue(auctionEvent);
        }

        public async Task<IEnumerable<IAuctionEvent>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        private async Task PumpPersistQueueAsync()
        {
            while (!_persistQueuePumpingCancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    IAuctionEvent auctionEvent;

                    if (_persistQueue.TryPeek(out auctionEvent))
                    {
                        await _repository.AddAsync(auctionEvent);
                    }

                    _persistQueue.TryDequeue(out auctionEvent);
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
