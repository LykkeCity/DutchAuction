using System.Threading.Tasks;
using Autofac;

namespace DutchAuction.Core.Services.Lots
{
    public interface IAuctionLotManager : IStartable
    {
        Task AddAsync(string clientId, string assetId, double price, double volume);
        Order[] GetOrderbook();
    }
}