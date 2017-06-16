using System.Collections.Generic;
using System.Collections.Immutable;

namespace DutchAuction.Core.Domain.Auction
{
    public interface IClientBid
    {
        string ClientId { get; }
        double LimitPriceChf { get; }
        IImmutableList<KeyValuePair<string, double>> AssetVolumes { get; }
    }
}