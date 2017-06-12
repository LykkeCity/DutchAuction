using System;
using System.Globalization;
using DutchAuction.Core.Domain.Lots;
using Microsoft.WindowsAzure.Storage.Table;

namespace DutchAuction.Repositories.Lots
{
    public class AuctionLotEntity : TableEntity, IAuctionLot
    {
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

        public static AuctionLotEntity Create(IAuctionLot src)
        {
            return new AuctionLotEntity
            {
                PartitionKey = GeneratePartitionKey(src.ClientId),
                RowKey = GenerateRowKey(src.Date),
                AssetId = src.AssetId,
                ClientId = src.ClientId,
                Date = src.Date,
                Price = src.Price,
                Volume = src.Volume
            };
        }
    }
}