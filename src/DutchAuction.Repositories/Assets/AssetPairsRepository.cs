using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using DutchAuction.Core.Domain.Asset;

namespace DutchAuction.Repositories.Assets
{
    public class AssetPairsRepository : IAssetPairsRepository
    {
        private readonly INoSQLTableStorage<AssetPairEntity> _tableStorage;

        public AssetPairsRepository(INoSQLTableStorage<AssetPairEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<IEnumerable<IAssetPair>> GetAllAsync()
        {
            var partitionKey = AssetPairEntity.GeneratePartitionKey();

            return (await _tableStorage.GetDataAsync(partitionKey)).Select(AssetPair.Create);
        }
    }
}