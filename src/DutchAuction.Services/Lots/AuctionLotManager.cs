using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using DutchAuction.Core;
using DutchAuction.Core.Domain.Lots;
using DutchAuction.Core.Services.Assets;
using DutchAuction.Core.Services.Lots;

namespace DutchAuction.Services.Lots
{
    public class AuctionLotManager :
        IAuctionLotManager,
        IDisposable
    {
        private readonly IAuctionLotRepository _repository;
        private readonly IAuctionLotCacheService _cacheService;
        private readonly IAssetExchangeService _assetrExchangeService;
        private readonly ConcurrentQueue<AuctionLot> _persistQueue;
        private readonly ILog _log;

        private CancellationTokenSource _persistQueuePumpingCancellationTokenSource;

        public AuctionLotManager(
            IAuctionLotRepository auctionLotRepository,
            IAuctionLotCacheService cacheService,
            IAssetExchangeService assetrExchangeService, 
            ILog log)
        {
            _repository = auctionLotRepository;
            _cacheService = cacheService;
            _assetrExchangeService = assetrExchangeService;
            _log = log;

            _persistQueue = new ConcurrentQueue<AuctionLot>();
        }

        public void Start()
        {
            try
            {
                UpdateCache().Wait();

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

        public void Add(string clientId, string assetId, double price, double volume)
        {
            var lot = new AuctionLot
            {
                ClientId = clientId,
                AssetId = assetId,
                Date = DateTime.UtcNow,
                Price = price,
                Volume = volume
            };
            
            _cacheService.Add(lot);
            _persistQueue.Enqueue(lot);
        }

        public Order[] GetOrderbook()
        {
            return _cacheService
                .GetAllAsync()
                .Select(item => new
                {
                    Price = _assetrExchangeService.Exchange(item.Price, item.AssetId, "CHF"),
                    ClientId = item.ClientId,
                    Volume = item.Volume
                })
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

        private async Task PumpPersistQueueAsync()
        {
            while (!_persistQueuePumpingCancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    AuctionLot lot;

                    if (_persistQueue.TryPeek(out lot))
                    {
                        await _repository.AddAsync(lot);
                    }

                    _persistQueue.TryDequeue(out lot);
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

        private async Task UpdateCache()
        {
            var lots = await _repository.GetAllAsync();

            _cacheService.InitCache(lots.ToList());
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
