using System;
using System.Threading;
using System.Threading.Tasks;
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
        private Timer _caheUpdateTimer;

        public AssetPairsManager(IAssetPairsRepository repository, IAssetPairsCacheService cache, TimeSpan cacheUpdatePeriod)
        {
            _repository = repository;
            _cache = cache;
            _cacheUpdatePeriod = cacheUpdatePeriod;
        }

        public void Start()
        {
            UpdateCache().Wait();

            _caheUpdateTimer = new Timer(async s => await UpdateCache(), null, _cacheUpdatePeriod, _cacheUpdatePeriod);
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

        private async Task UpdateCache()
        {
            var pairs = await _repository.GetAllAsync();

            _cache.Update(pairs);
        }

        public void Dispose()
        {
            _caheUpdateTimer?.Dispose();
        }
    }
}