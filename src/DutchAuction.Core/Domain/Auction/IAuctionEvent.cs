using System;

namespace DutchAuction.Core.Domain.Auction
{
    public interface IAuctionEvent
    {
        AuctionEventType Type { get; }
        string ClientId { get; }
        string AssetId { get; }
        double Volume { get; }
        double Price { get; }
        DateTime Date { get; }
    }
}
