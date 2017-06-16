using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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

            AssetVolumesLkk = bidCalculation
                .AssetVolumes
                .Select(item => new KeyValuePair<string, double>(item.Key, assetExchangeService.Exchange(item.Value, item.Key, "CHF") / lkkPriceChf))
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

                InMoneyAssetVolumesLkk = bidCalculation
                    .InMoneyAssetVolumes
                    .Select(item => new KeyValuePair<string, double>(item.Key,
                        assetExchangeService.Exchange(item.Value, item.Key, "CHF") / lkkPriceChf))
                    .ToImmutableArray();
            }
            else
            {
                throw new InvalidOperationException($"Invalid bid calculation state: {bidCalculation.State}");
            }
        }
    }
}