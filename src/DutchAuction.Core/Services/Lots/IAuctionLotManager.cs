using Autofac;

namespace DutchAuction.Core.Services.Lots
{
    public interface IAuctionLotManager : IStartable
    {
        void Add(string clientId, string assetId, double price, double volume);
        Order[] GetOrderbook();
    }
}