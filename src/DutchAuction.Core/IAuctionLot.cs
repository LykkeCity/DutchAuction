using System;

namespace DutchAuction.Core
{
    public interface IAuctionLot
    {
        string ClientId { get; }
        string AssetId { get; }
        double Volume { get; }
        double Price { get; }
        DateTime Date { get; }
    }
}
