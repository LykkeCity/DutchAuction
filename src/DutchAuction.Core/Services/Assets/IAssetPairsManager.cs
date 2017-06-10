using Autofac;
using DutchAuction.Core.Domain.Asset;

namespace DutchAuction.Core.Services.Assets
{
    public interface IAssetPairsManager : IStartable
    {
        IAssetPair GetEnabledPair(string assetPairId);
    }
}