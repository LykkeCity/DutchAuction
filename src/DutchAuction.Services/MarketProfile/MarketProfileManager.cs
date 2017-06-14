using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using DutchAuction.Core;
using DutchAuction.Core.Domain.MarketProfile;
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

        public MarketProfileManager(
            ILykkeMarketProfileServiceAPI api,
            IMarketProfileCacheService cache,
            TimeSpan cacheUpdatePeriod, 
            ILog log)
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

                _cacheUpdateTimer = new Timer(async s => await OnUpdateCacheTimerAsync(), null, _cacheUpdatePeriod, Timeout.InfiniteTimeSpan);
            }
            catch (Exception ex)
            {
                _log.WriteErrorAsync(Constants.ComponentName, null, null, ex).Wait();
                throw;
            }
        }

        public MarketProfileAssetPair TryGetPair(string baseAssetId, string targetAssetId)
        {
            var assetPairId = string.Concat(baseAssetId, targetAssetId);

            return Map(_cache.TryGetPair(assetPairId));
        }

        public void Dispose()
        {
            _cacheUpdateTimer?.Dispose();
            _api?.Dispose();
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
            var pairs = await _api.ApiMarketProfileGetAsync();

            _cache.Update(pairs);
        }

        private static MarketProfileAssetPair Map(AssetPairModel source)
        {
            if (source == null)
            {
                return null;
            }

            return new MarketProfileAssetPair
            {
                AssetPair = source.AssetPair,
                AskPrice = source.AskPrice,
                BidPrice = source.BidPrice,
                AskPriceTimestamp = source.AskPriceTimestamp,
                BidPriceTimestamp = source.BidPriceTimestamp
            };
        }
    }
}