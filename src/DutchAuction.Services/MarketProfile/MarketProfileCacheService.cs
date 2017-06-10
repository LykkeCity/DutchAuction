using System.Collections.Generic;
using System.Linq;
using DutchAuction.Core.Services.MarketProfile;
using Lykke.MarketProfileService.Client.Models;

namespace DutchAuction.Services.MarketProfile
{
    public class MarketProfileCacheService : IMarketProfileCacheService
    {
        private Dictionary<string, AssetPairModel> _pairs = new Dictionary<string, AssetPairModel>();

        public void Update(IEnumerable<AssetPairModel> pairs)
        {
            _pairs = pairs.ToDictionary(p => p.AssetPair, p => p);
        }

        public AssetPairModel TryGetPair(string assetPairId)
        {
            _pairs.TryGetValue(assetPairId, out AssetPairModel pair);

            return pair;
        }
    }
}