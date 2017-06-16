using System.Collections.Generic;
using System.Collections.Immutable;

namespace DutchAuction.Core.Domain.Auction
{
    public class OrderbookBid : IOrderbookBid
    {
        public string ClientId { get; }
        public double LimitPriceChf { get; }
        public double LkkPriceChf { get; }
        public OrderbookBidState State { get; }
        public IImmutableList<KeyValuePair<string, double>> AssetVolumes { get; }
        public IImmutableList<KeyValuePair<string, double>> AssetVolumesLkk { get; }
        public IImmutableList<KeyValuePair<string, double>> InMoneyAssetVolumesLkk { get; }
        
        public OrderbookBid(
            string clientId,
            double limitPriceChf, 
            double lkkPriceChf, 
            OrderbookBidState state, 
            IImmutableList<KeyValuePair<string, double>> assetVolumes,
            IImmutableList<KeyValuePair<string, double>> assetVolumesLkk,
            IImmutableList<KeyValuePair<string, double>> inMoneyAssetVolumesLkk)
        {
            ClientId = clientId;
            State = state;
            LimitPriceChf = limitPriceChf;
            LkkPriceChf = lkkPriceChf;
            AssetVolumes = assetVolumes;
            AssetVolumesLkk = assetVolumesLkk;
            InMoneyAssetVolumesLkk = inMoneyAssetVolumesLkk;
        }
    }
}