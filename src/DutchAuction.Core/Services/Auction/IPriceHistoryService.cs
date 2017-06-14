using System.Threading.Tasks;
using Autofac;

namespace DutchAuction.Core.Services.Auction
{
    public interface IPriceHistoryService : IStartable
    {
        Task PublishAsync(double price, double inMoneyVolume, double outOfTheMoneyVolume);
    }
}