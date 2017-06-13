using Autofac;
using DutchAuction.Core.Domain.MarketProfile;

namespace DutchAuction.Core.Services.MarketProfile
{
    public interface IMarketProfileManager : IStartable
    {
        MarketProfileAssetPair TryGetPair(string baseAssetId, string targetAssetId);
    }
}