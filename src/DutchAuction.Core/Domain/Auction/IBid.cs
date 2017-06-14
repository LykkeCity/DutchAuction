using System.Collections.Generic;

namespace DutchAuction.Core.Domain.Auction
{
    public interface IBid
    {
        string ClientId { get; }
        double Price { get; }
        IReadOnlyDictionary<string, double> AssetVolumes { get; }
        BidState State { get; }
        IReadOnlyDictionary<string, double> InMoneyAssetVolumes { get; }
    }
}