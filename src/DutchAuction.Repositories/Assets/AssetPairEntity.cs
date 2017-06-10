using DutchAuction.Core.Domain.Asset;
using Microsoft.WindowsAzure.Storage.Table;

namespace DutchAuction.Repositories.Assets
{
    public class AssetPairEntity : 
        TableEntity, 
        IAssetPair
    {
        public static string GeneratePartitionKey()
        {
            return "AssetPair";
        }

        public string Id => RowKey;
        public string Name { get; set; }
        public string BaseAssetId { get; set; }
        public string QuotingAssetId { get; set; }
        public int Accuracy { get; set; }
        public int InvertedAccuracy { get; set; }
        public string Source { get; set; }
        public string Source2 { get; set; }
        public bool IsDisabled { get; set; }
    }
}