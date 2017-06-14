using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using DutchAuction.Core;
using DutchAuction.Core.Domain.Asset;
using DutchAuction.Core.Services.Assets;

namespace DutchAuction.Services.Assets
{
    public class AssetPairsManager : 
        IAssetPairsManager,
        IDisposable
    {
        private readonly IAssetPairsRepository _repository;
        private readonly IAssetPairsCacheService _cache;
        private readonly TimeSpan _cacheUpdatePeriod;
        private readonly ILog _log;

        private Timer _cacheUpdateTimer;

        public AssetPairsManager(IAssetPairsRepository repository, IAssetPairsCacheService cache, TimeSpan cacheUpdatePeriod, ILog log)
        {
            _repository = repository;
            _cache = cache;
            _cacheUpdatePeriod = cacheUpdatePeriod;
            _log = log;
        }

        public void Start()
        {
            try
            {
                UpdateCacheAsync().Wait();

                _cacheUpdateTimer = new Timer(async s => await OnUpdateCacheTimerAsync(), null, _cacheUpdatePeriod, Timeout.InfiniteTimeSpan);
            }
            catch (Exception ex)
            {
                _log.WriteErrorAsync(Constants.ComponentName, null, null, ex).Wait();
                throw;
            }
        }

        public IAssetPair GetEnabledPair(string assetPairId)
        {
            var pair = _cache.GetPair(assetPairId);

            if (pair.IsDisabled)
            {
                throw new ArgumentException(nameof(assetPairId), $"Asset pair {assetPairId} is disabled");
            }

            return pair;
        }

        private async Task OnUpdateCacheTimerAsync()
        {
            _cacheUpdateTimer.Change(Timeout.Infinite, Timeout.Infinite);

            try
            {
                await UpdateCacheAsync();
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(Constants.ComponentName, null, null, ex);
            }
            finally
            {
                _cacheUpdateTimer.Change(_cacheUpdatePeriod, Timeout.InfiniteTimeSpan);
            }
        }

        private async Task UpdateCacheAsync()
        {
            var pairs = await _repository.GetAllAsync();

            _cache.Update(pairs);
        }

        public void Dispose()
        {
            _cacheUpdateTimer?.Dispose();
        }
    }
}