using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using DutchAuction.Core;
using DutchAuction.Core.Services.MarketProfile;
using Lykke.MarketProfileService.Client;
using Lykke.MarketProfileService.Client.Models;

namespace DutchAuction.Services.MarketProfile
{
    public class MarketProfileManager : 
        IMarketProfileManager,
        IDisposable
    {
        private readonly ILykkeMarketProfileServiceAPI _api;
        private readonly IMarketProfileCacheService _cache;
        private readonly TimeSpan _cacheUpdatePeriod;
        private readonly ILog _log;

        private Timer _cacheUpdateTimer;

        public MarketProfileManager(ILykkeMarketProfileServiceAPI api, IMarketProfileCacheService cache, TimeSpan cacheUpdatePeriod, ILog log)
        {
            _api = api;
            _cache = cache;
            _cacheUpdatePeriod = cacheUpdatePeriod;
            _log = log;
        }

        public void Start()
        {
            try
            {
                UpdateCacheAsync().Wait();

                _cacheUpdateTimer = new Timer(async s => await OnUpdateCacheTimerAsync(), null, _cacheUpdatePeriod,
                    _cacheUpdatePeriod);
            }
            catch (Exception ex)
            {
                _log.WriteErrorAsync(Constants.ComponentName, null, null, ex).Wait();
                throw;
            }
        }

        public AssetPairModel TryGetPair(string baseAssetId, string targetAssetId)
        {
            var assetPairId = string.Concat(baseAssetId, targetAssetId);

            return _cache.TryGetPair(assetPairId);
        }

        private async Task OnUpdateCacheTimerAsync()
        {
            try
            {
                await UpdateCacheAsync();
            }
            catch (Exception ex)
            {
                _log.WriteErrorAsync(Constants.ComponentName, null, null, ex).Wait();
            }
        }

        private async Task UpdateCacheAsync()
        {
            var pairs = await _api.ApiMarketProfileGetAsync();

            _cache.Update(pairs);
        }

        public void Dispose()
        {
            _cacheUpdateTimer?.Dispose();
            _api?.Dispose();
        }
    }
}