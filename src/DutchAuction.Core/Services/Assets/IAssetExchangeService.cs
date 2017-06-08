namespace DutchAuction.Core.Services.Assets
{
    public interface IAssetExchangeService
    {
        double Exchange(double baseAmount, string baseAssetId, string targetAssetId);
    }
}