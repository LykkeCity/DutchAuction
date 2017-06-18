using System;
using DutchAuction.Core.Domain.Auction;
using Microsoft.WindowsAzure.Storage.Table;

namespace DutchAuction.Repositories.AuctionEvents
{
    public class AuctionEventEntity : TableEntity, IAuctionEvent
    {
        public AuctionEventType Type { get; set; }
        public string TypeCode
        {
            get => Type.ToString();
            set => Type = (AuctionEventType) Enum.Parse(typeof(AuctionEventType), value);
        }
        public string ClientId { get; set; }
        public string AssetId { get; set; }
        public double Volume { get; set; }
        public double Price { get; set; }
        public DateTime Date { get; set; }

        public static string GeneratePartitionKey(string clientId)
        {
            return clientId;
        }

        public static string GenerateRowKey(IAuctionEvent @event)
        {
            switch (@event.Type)
            {
                case AuctionEventType.StartBidding:
                    return $"{@event.Date.Ticks:X32}";

                case AuctionEventType.SetPrice:
                    return $"{@event.Date.Ticks:X32}-{@event.Price:e}";

                case AuctionEventType.SetAssetVolume:
                    return $"{@event.Date.Ticks:X32}-{@event.AssetId}-{@event.Volume:e}";

                default:
                    throw new ArgumentOutOfRangeException(nameof(@event.Type), @event.Type, string.Empty);
            }           
        }

        public static AuctionEventEntity Create(IAuctionEvent src)
        {
            return new AuctionEventEntity
            {
                PartitionKey = GeneratePartitionKey(src.ClientId),
                RowKey = GenerateRowKey(src),
                Type = src.Type,
                AssetId = src.AssetId,
                ClientId = src.ClientId,
                Date = src.Date,
                Price = src.Price,
                Volume = src.Volume
            };
        }
    }
}