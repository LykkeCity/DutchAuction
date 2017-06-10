using System;
using System.Collections.Generic;
using System.Linq;
using DutchAuction.Core.Domain.Asset;
using DutchAuction.Core.Services.Assets;

namespace DutchAuction.Services.Assets
{
    public class AssetPairsCacheService : IAssetPairsCacheService
    {
        private Dictionary<string, IAssetPair> _pairs = new Dictionary<string, IAssetPair>();

        public void Update(IEnumerable<IAssetPair> pairs)
        {
            _pairs = pairs.ToDictionary(p => p.Id, p => p);
        }

        public IAssetPair GetPair(string assetPairId)
        {
            _pairs.TryGetValue(assetPairId, out IAssetPair pair);

            if (pair == null)
            {
                throw new ArgumentOutOfRangeException(nameof(assetPairId), assetPairId, $"Asset pair not found");
            }

            return pair;
        }
    }
}