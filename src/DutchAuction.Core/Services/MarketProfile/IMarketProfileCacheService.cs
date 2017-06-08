using System.Collections.Generic;
using Lykke.MarketProfileService.Client.Models;

namespace DutchAuction.Core.Services.MarketProfile
{
    public interface IMarketProfileCacheService
    {
        void Update(IEnumerable<AssetPairModel> pairs);
        AssetPairModel TryGetPair(string pairCode);
    }
}