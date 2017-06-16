using System.Collections.Generic;
using System.Collections.Immutable;

namespace DutchAuction.Core.Domain.Auction
{
    public interface IOrderbookBid
    {
        string ClientId { get; }
        double LimitPriceChf { get; }
        double LkkPriceChf { get; }
        OrderbookBidState State { get; }
        IImmutableList<KeyValuePair<string, double>> AssetVolumes { get; }
        IImmutableList<KeyValuePair<string, double>> AssetVolumesLkk { get; }
        IImmutableList<KeyValuePair<string, double>> InMoneyAssetVolumesLkk { get; }
    }
}