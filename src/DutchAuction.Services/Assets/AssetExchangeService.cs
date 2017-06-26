using System;
using System.Threading.Tasks;
using DutchAuction.Core.Services.Assets;
using DutchAuction.Core.Services.MarketProfile;

namespace DutchAuction.Services.Assets
{
    public class AssetExchangeService : IAssetExchangeService
    {
        private readonly IMarketProfileManager _marketProfileManager;
        private readonly IAssetPairsManager _assetPairsManager;

        public AssetExchangeService(
            IMarketProfileManager marketProfileManager,
            IAssetPairsManager assetPairsManager)
        {
            _marketProfileManager = marketProfileManager;
            _assetPairsManager = assetPairsManager;
        }

        public async Task<double> ExchangeAsync(double baseAmount, string baseAssetId, string targetAssetId)
        {
            if (baseAssetId == targetAssetId)
            {
                return baseAmount;
            }

            var directPair = _marketProfileManager.TryGetPair(baseAssetId, targetAssetId);

            if (directPair != null)
            {
                var assetPair = await _assetPairsManager.GetEnabledPairAsync(directPair.AssetPair);

                return Math.Round(baseAmount * directPair.BidPrice, assetPair.Accuracy);
            }

            var invertedPair = _marketProfileManager.TryGetPair(targetAssetId, baseAssetId);

            if (invertedPair != null)
            {
                var assetPair = await _assetPairsManager.GetEnabledPairAsync(invertedPair.AssetPair);

                return Math.Round(baseAmount / invertedPair.AskPrice, assetPair.InvertedAccuracy);
            }

            throw new ArgumentOutOfRangeException(string.Empty, $"No asset pair {baseAssetId}{targetAssetId} nor {targetAssetId}{baseAssetId} found");
        }
    }
}