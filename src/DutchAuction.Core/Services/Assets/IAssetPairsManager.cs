using System.Threading.Tasks;
using Lykke.Service.Assets.Client.Custom;

namespace DutchAuction.Core.Services.Assets
{
    public interface IAssetPairsManager
    {
        Task<IAssetPair> GetEnabledPairAsync(string assetPairId);
    }
}