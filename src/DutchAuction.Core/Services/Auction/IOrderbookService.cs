using System.Collections.Immutable;
using DutchAuction.Core.Domain.Auction;

namespace DutchAuction.Core.Services.Auction
{
    public interface IOrderbookService
    {
        IOrderbook Render(IImmutableList<IClientBid> clientBids);
    }
}