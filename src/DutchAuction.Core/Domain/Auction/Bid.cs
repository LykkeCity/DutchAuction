using System;

namespace DutchAuction.Core.Domain.Auction
{
    public class Bid : IBid
    {
        public BidType Type { get; private set; }
        public string ClientId { get; private set; }
        public string AssetId { get; private set; }
        public double Volume { get; private set; }
        public double Price { get; private set; }
        public DateTime Date { get; private set; }

        public static Bid Create(IBid src)
        {
            return new Bid
            {
                Type = src.Type,
                AssetId = src.AssetId,
                ClientId = src.ClientId,
                Date = src.Date,
                Price = src.Price,
                Volume = src.Volume
            };
        }

        public static Bid CreateStartBidding(string clientId, string assetId, double price, double volume, DateTime date)
        {
            return new Bid
            {
                Type = BidType.StartBidding,
                ClientId = clientId,
                AssetId = assetId,
                Price = price,
                Volume = volume,
                Date = date
            };
        }

        public static Bid CreateSetPrice(string clientId, double price, DateTime date)
        {
            return new Bid
            {
                Type = BidType.SetPrice,
                ClientId = clientId,
                Price = price,
                Date = date
            };
        }

        public static Bid CreateSetAssetVolume(string clientId, string assetId, double volume, DateTime date)
        {
            return new Bid
            {
                Type = BidType.SetAssetVolume,
                ClientId = clientId,
                AssetId = assetId,
                Volume = volume,
                Date = date
            };
        }
    }
}