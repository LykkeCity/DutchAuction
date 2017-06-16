using System.Collections.Generic;

namespace DutchAuction.Core.Domain.Auction
{
    public interface IBid
    {
        string ClientId { get; }
        double LimitPriceChf { get; }
        double LkkPriceChf { get; }
        IReadOnlyCollection<KeyValuePair<string, double>> AssetVolumes { get; }
        BidState State { get; }
        IReadOnlyCollection<KeyValuePair<string, double>> AssetVolumesLkk { get; }
        IReadOnlyCollection<KeyValuePair<string, double>> InMoneyAssetVolumesLkk { get; }
    }
}