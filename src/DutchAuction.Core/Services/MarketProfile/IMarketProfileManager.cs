using Autofac;
using Lykke.MarketProfileService.Client.Models;

namespace DutchAuction.Core.Services.MarketProfile
{
    public interface IMarketProfileManager : IStartable
    {
        AssetPairModel TryGetPair(string baseAssetId, string targetAssetId);
    }
}