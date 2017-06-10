using System.Collections.Generic;
using DutchAuction.Core.Domain.Asset;

namespace DutchAuction.Core.Services.Assets
{
    public interface IAssetPairsCacheService
    {
        void Update(IEnumerable<IAssetPair> pairs);
        IAssetPair GetPair(string assetPairId);
    }
}