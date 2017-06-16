using System.Collections.Immutable;

namespace DutchAuction.Core.Domain.Auction
{
    public interface IOrderbook
    {
        double LkkPriceChf { get; }
        double InMoneyVolumeLkk { get; }
        double OutOfTheMoneyVolumeLkk { get; }
        IImmutableList<Order> InMoneyOrders { get; }
        IImmutableList<Order> OutOfMoneyOrders { get; }

        IOrderbookBid TryGetBid(string clientId);
    }
}