using System;

namespace DutchAuction.Core.Domain.Auction
{
    public class AuctionEvent : IAuctionEvent
    {
        public AuctionEventType Type { get; private set; }
        public string ClientId { get; private set; }
        public string AssetId { get; private set; }
        public double Volume { get; private set; }
        public double Price { get; private set; }
        public DateTime Date { get; private set; }

        public static AuctionEvent Create(IAuctionEvent src)
        {
            return new AuctionEvent
            {
                Type = src.Type,
                AssetId = src.AssetId,
                ClientId = src.ClientId,
                Date = src.Date,
                Price = src.Price,
                Volume = src.Volume
            };
        }

        public static AuctionEvent CreateStartBidding(string clientId, string assetId, double price, double volume, DateTime date)
        {
            return new AuctionEvent
            {
                Type = AuctionEventType.StartBidding,
                ClientId = clientId,
                AssetId = assetId,
                Price = price,
                Volume = volume,
                Date = date
            };
        }

        public static AuctionEvent CreateSetPrice(string clientId, double price, DateTime date)
        {
            return new AuctionEvent
            {
                Type = AuctionEventType.SetPrice,
                ClientId = clientId,
                Price = price,
                Date = date
            };
        }

        public static AuctionEvent CreateSetAssetVolume(string clientId, string assetId, double volume, DateTime date)
        {
            return new AuctionEvent
            {
                Type = AuctionEventType.SetAssetVolume,
                ClientId = clientId,
                AssetId = assetId,
                Volume = volume,
                Date = date
            };
        }
    }
}