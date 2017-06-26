using System;
using System.Threading.Tasks;
using DutchAuction.Core.Services.Assets;
using Lykke.Service.Assets.Client.Custom;

namespace DutchAuction.Services.Assets
{
    public class AssetPairsManager : IAssetPairsManager
    {
        private readonly ICachedAssetsService _service;

        public AssetPairsManager(ICachedAssetsService service)
        {
            _service = service;
        }

        public async Task<IAssetPair> GetEnabledPairAsync(string assetPairId)
        {
            var pair = await _service.TryGetAssetPairAsync(assetPairId);

            if (pair.IsDisabled)
            {
                throw new ArgumentException(nameof(assetPairId), $"Asset pair {assetPairId} is disabled");
            }

            return pair;
        }
    }
}