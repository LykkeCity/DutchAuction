using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using DutchAuction.Core.Services.Assets;

namespace DutchAuction.Services.Auction.OrderbookRendering
{
    internal class BidCalculation
    {
        public string ClientId { get; }
        public double LimitPriceChf { get; }
        public double VolumeChf { get; }
        public BidCalculationState State { get; private set; }
        public IImmutableList<KeyValuePair<string, double>> AssetVolumes { get; }
        public IImmutableList<KeyValuePair<string, double>> InMoneyAssetVolumes { get; private set; }

        public BidCalculation(
            IAssetExchangeService assetExchangeService,
            string clientId,
            double limitPriceChf,
            IImmutableList<KeyValuePair<string, double>> assetVolumes)
        {
            ClientId = clientId;
            LimitPriceChf = limitPriceChf;
            AssetVolumes = assetVolumes;

            // Convert volume to CHF
            VolumeChf = Task
                .WhenAll(AssetVolumes.Select(async a => await assetExchangeService.ExchangeAsync(a.Value, a.Key, "CHF")))
                .Result
                .Sum(amount => amount);

            State = BidCalculationState.NotCalculatedYet;
        }

        public void SetInMoneyState()
        {
            State = BidCalculationState.InMoney;
            InMoneyAssetVolumes = AssetVolumes;
        }

        public void SetOutOfTheMoneyState()
        {
            State = BidCalculationState.OutOfTheMoney;
            InMoneyAssetVolumes = null;
        }

        public void SetPartiallyInMoneyState(IImmutableList<KeyValuePair<string, double>> inMoneyAssetVolumes)
        {
            State = BidCalculationState.PartiallyInMoney;

            InMoneyAssetVolumes = inMoneyAssetVolumes;
        }
    }
}