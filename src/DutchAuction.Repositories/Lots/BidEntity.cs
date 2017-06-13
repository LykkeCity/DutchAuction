using System;
using System.Globalization;
using DutchAuction.Core.Domain.Auction;
using Microsoft.WindowsAzure.Storage.Table;

namespace DutchAuction.Repositories.Lots
{
    public class BidEntity : TableEntity, IBid
    {
        public BidType Type { get; set; }
        public string TypeCode
        {
            get => Type.ToString();
            set => Type = (BidType) Enum.Parse(typeof(BidType), value);
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

        public static BidEntity Create(IBid src)
        {
            return new BidEntity
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