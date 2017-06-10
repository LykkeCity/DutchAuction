namespace DutchAuction.Core.Domain.Asset
{
    public class AssetPair : IAssetPair
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string BaseAssetId { get; set; }
        public string QuotingAssetId { get; set; }
        public int Accuracy { get; set; }
        public int InvertedAccuracy { get; set; }
        public string Source { get; set; }
        public string Source2 { get; set; }
        public bool IsDisabled { get; set; }

        public static AssetPair Create(IAssetPair src)
        {
            return new AssetPair
            {
                Id = src.Id,
                Name = src.Name,
                BaseAssetId = src.BaseAssetId,
                QuotingAssetId = src.QuotingAssetId,
                Accuracy = src.Accuracy,
                InvertedAccuracy = src.InvertedAccuracy,
                Source = src.Source,
                Source2 = src.Source2,
                IsDisabled = src.IsDisabled
            };
        }
    }
}