using System.Threading.Tasks;

namespace DutchAuction.Core.Services.Assets
{
    public interface IAssetExchangeService
    {
        Task<double> ExchangeAsync(double baseAmount, string baseAssetId, string targetAssetId);
    }
}