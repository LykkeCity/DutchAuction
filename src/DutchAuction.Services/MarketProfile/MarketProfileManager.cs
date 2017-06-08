using System;
using System.Threading;
using System.Threading.Tasks;
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
        private Timer _cacheUpdateTimer;

        public MarketProfileManager(ILykkeMarketProfileServiceAPI api, IMarketProfileCacheService cache, TimeSpan cacheUpdatePeriod)
        {
            _api = api;
            _cache = cache;
            _cacheUpdatePeriod = cacheUpdatePeriod;
        }

        public void Start()
        {
            UpdateCache().Wait();

            _cacheUpdateTimer = new Timer(async s => await UpdateCache(), null, _cacheUpdatePeriod, _cacheUpdatePeriod);
        }

        public AssetPairModel TryGetPair(string baseAssetId, string targetAssetId)
        {
            var pairCode = string.Concat(baseAssetId, targetAssetId);

            return _cache.TryGetPair(pairCode);
        }

        private async Task UpdateCache()
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