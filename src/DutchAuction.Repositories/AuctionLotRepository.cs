using DutchAuction.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Microsoft.WindowsAzure.Storage.Table;

namespace DutchAuction.Repositories
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
            return date.ToString(CultureInfo.InvariantCulture);
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

    public class AuctionLotRepository : IAuctionLotRepository
    {
        private readonly INoSQLTableStorage<AuctionLotEntity> _tableStorage;

        public AuctionLotRepository(INoSQLTableStorage<AuctionLotEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task AddAsync(IAuctionLot lot)
        {
            await _tableStorage.InsertOrReplaceAsync(AuctionLotEntity.Create(lot));
        }

        public async Task<IEnumerable<IAuctionLot>> GetAllAsync()
        {
            return (await _tableStorage.GetDataAsync()).Select(AuctionLot.Create);
        }
    }
}
