using DutchAuction.Core.Domain.Auction;

namespace DutchAuction.Core.Services.Auction
{
    public interface IOrderbookService
    {
        Orderbook Render();
    }
}