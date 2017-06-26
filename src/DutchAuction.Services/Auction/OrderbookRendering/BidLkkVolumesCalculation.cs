using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using DutchAuction.Core.Services.Assets;

namespace DutchAuction.Services.Auction.OrderbookRendering
{
    internal class BidLkkVolumesCalculation
    {
        public BidCalculation BidCalculation { get; }
        public IImmutableList<KeyValuePair<string, double>> AssetVolumesLkk { get; }
        public IImmutableList<KeyValuePair<string, double>> InMoneyAssetVolumesLkk { get; }

        public BidLkkVolumesCalculation(IAssetExchangeService assetExchangeService, BidCalculation bidCalculation, double lkkPriceChf)
        {
            BidCalculation = bidCalculation;

            AssetVolumesLkk = Task
                .WhenAll(bidCalculation
                    .AssetVolumes
                    .Select(async item => new KeyValuePair<string, double>(item.Key, await assetExchangeService.ExchangeAsync(item.Value, item.Key, "CHF") / lkkPriceChf)))
                .Result
                .ToImmutableArray();

            if (bidCalculation.State == BidCalculationState.InMoney)
            {
                InMoneyAssetVolumesLkk = AssetVolumesLkk;
            }
            else if (bidCalculation.State == BidCalculationState.OutOfTheMoney)
            {
                InMoneyAssetVolumesLkk = bidCalculation
                    .AssetVolumes
                    .Select(item => new KeyValuePair<string, double>(item.Key, 0d))
                    .ToImmutableArray();
            }
            else if (bidCalculation.State == BidCalculationState.PartiallyInMoney)
            {

                InMoneyAssetVolumesLkk = Task
                    .WhenAll(bidCalculation
                        .InMoneyAssetVolumes
                        .Select(async item => new KeyValuePair<string, double>(item.Key, await assetExchangeService.ExchangeAsync(item.Value, item.Key, "CHF") / lkkPriceChf)))
                    .Result
                    .ToImmutableArray();
            }
            else
            {
                throw new InvalidOperationException($"Invalid bid calculation state: {bidCalculation.State}");
            }
        }
    }
}