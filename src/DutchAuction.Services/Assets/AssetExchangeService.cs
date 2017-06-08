using DutchAuction.Core.Services.Assets;
using DutchAuction.Core.Services.MarketProfile;

namespace DutchAuction.Services.Assets
{
    public class AssetExchangeService : IAssetExchangeService
    {
        private readonly IMarketProfileManager _marketProfileManager;

        public AssetExchangeService(IMarketProfileManager marketProfileManager)
        {
            _marketProfileManager = marketProfileManager;
        }

        public double Exchange(double baseAmount, string baseAssetId, string targetAssetId)
        {
            var directPair = _marketProfileManager.TryGetPair(baseAssetId, targetAssetId);

            if (directPair != null)
            {
                return baseAmount * directPair.AskPrice ?? 0;
            }

            var invertedPair = _marketProfileManager.TryGetPair(targetAssetId, baseAssetId);

            if (invertedPair != null)
            {
                return baseAmount / invertedPair.BidPrice ?? 0;
            }

            // TODO: throw?
            return baseAmount;
        }
    }
}