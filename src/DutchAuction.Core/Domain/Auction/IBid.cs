using System.Collections.Generic;

namespace DutchAuction.Core.Domain.Auction
{
    public interface IBid
    {
        string ClientId { get; }
        double Price { get; }
        IReadOnlyCollection<KeyValuePair<string, double>> AssetVolumes { get; }
        BidState State { get; }
        IReadOnlyCollection<KeyValuePair<string, double>> InMoneyAssetVolumes { get; }
    }
}