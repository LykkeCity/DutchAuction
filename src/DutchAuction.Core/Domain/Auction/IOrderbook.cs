using System;
using System.Collections.Immutable;

namespace DutchAuction.Core.Domain.Auction
{
    public interface IOrderbook
    {
        TimeSpan RenderDuration { get; }
        int BidsCount { get; }
        double LkkPriceChf { get; }
        double InMoneyVolumeLkk { get; }
        double OutOfTheMoneyVolumeLkk { get; }
        IImmutableList<Order> InMoneyOrders { get; }
        IImmutableList<Order> OutOfTheMoneyOrders { get; }

        IOrderbookBid TryGetBid(string clientId);
    }
}