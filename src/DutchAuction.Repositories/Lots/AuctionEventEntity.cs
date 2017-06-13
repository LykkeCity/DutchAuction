using System;
using System.Globalization;
using DutchAuction.Core.Domain.Auction;
using Microsoft.WindowsAzure.Storage.Table;

namespace DutchAuction.Repositories.Lots
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

        public static string GenerateRowKey(DateTime date)
        {
            return date.ToString("yyyy.MM.dd HH:mm:ss.fffffff", CultureInfo.InvariantCulture);
        }

        public static AuctionEventEntity Create(IAuctionEvent src)
        {
            return new AuctionEventEntity
            {
                PartitionKey = GeneratePartitionKey(src.ClientId),
                RowKey = GenerateRowKey(src.Date),
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