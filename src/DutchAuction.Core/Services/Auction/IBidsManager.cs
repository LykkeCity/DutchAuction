using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using DutchAuction.Core.Domain.Auction;

namespace DutchAuction.Core.Services.Auction
{
    public interface IBidsManager : IStartable
    {
        void Add(IBid bid);
        Task<IEnumerable<IBid>> GetAllAsync();
    }
}