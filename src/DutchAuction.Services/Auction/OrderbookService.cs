using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DutchAuction.Core;
using DutchAuction.Core.Domain.Auction;
using DutchAuction.Core.Services.Assets;
using DutchAuction.Core.Services.Auction;

namespace DutchAuction.Services.Auction
{
    public class OrderbookService : IOrderbookService
    {
        private enum CalculationContinuation
        {
            RestartCurrentTestPriceLevel,
            Continue
        }

        private enum BidCalculationState
        {
            NotCalculatedYet,
            InMoney,
            OutOfTheMoney,
            PartiallyInMoney
        }

        private class PriceLevelBidLkkVolumeCalculation
        {
            public AuctionPriceLevel PriceLevel { get; }
            public BidLkkVolumesCalculation[] BidLkkVolumeCalculations { get; }

            public PriceLevelBidLkkVolumeCalculation(AuctionPriceLevel priceLevel, BidLkkVolumesCalculation[] bidLkkVolumeCalculations)
            {
                PriceLevel = priceLevel;
                BidLkkVolumeCalculations = bidLkkVolumeCalculations;
            }
        }

        private class BidLkkVolumesCalculation
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

                if (bidCalculation.State == BidCalculationState.OutOfTheMoney)
                {
                    InMoneyAssetVolumesLkk = bidCalculation
                        .AssetVolumes
                        .Select(item => new KeyValuePair<string, double>(item.Key, 0d))
                        .ToImmutableArray();
                }
                else if (bidCalculation.State == BidCalculationState.InMoney ||
                         bidCalculation.State == BidCalculationState.PartiallyInMoney)
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

        private class BidCalculation
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
                VolumeChf = AssetVolumes.Sum(a => assetExchangeService.Exchange(a.Value, a.Key, "CHF"));

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

        private class AuctionPriceLevel
        {
            public double PriceChf { get; set; }
            public BidCalculation[] BidCalculations { get; set; }
        }

        private class RenderContext
        {
            public double AuctionVolumeChf { get; set; }
            public double PrevTestPriceLevelAuctionVolumeChf { get; set; }
            public double AuctionInMoneyVolumeLkk { get; set; }
            public double AuctionOutOfTheMoneyVolumeLkk { get; set; }
            public double LkkPriceChf { get; set; }
            public bool IsAllLotsSold { get; set; }
            public bool IsAutoFitPriceCase { get; set; }
        }

        private readonly IAssetExchangeService _assetExchangeService;
        private readonly double _totalAuctionVolumeLkk;
        private readonly double _minClosingBidCutoffVolumeLkk;

        public OrderbookService(
            IAssetExchangeService assetExchangeService,
            double totalAuctionVolumeLkk,
            double minClosingBidCutoffVolumeLkk)
        {
            _assetExchangeService = assetExchangeService;
            _totalAuctionVolumeLkk = totalAuctionVolumeLkk;
            _minClosingBidCutoffVolumeLkk = minClosingBidCutoffVolumeLkk;
        }

        public IOrderbook Render(IImmutableList<IClientBid> clientBids)
        {
            var context = new RenderContext();
            var priceLevels = GetPriceLevels(clientBids);

            // Iterate through price levels (from high to low price) that we try to sale all given LKK
            // with as small price as possible. Name it "Test price level"
            for (var testPriceLevel = 0; testPriceLevel < priceLevels.Length; testPriceLevel++)
            {
                if (context.IsAllLotsSold)
                {
                    break;
                }

                TrySaleWithPriceLevel(context, testPriceLevel, priceLevels);
            }

            var bidLkkVolumePriceLevels = CalculatePriceLevelBidLkkVolumes(priceLevels, context.LkkPriceChf);

            return new Orderbook(
                lkkPriceChf: context.LkkPriceChf,
                inMoneyVolumeLkk: context.AuctionInMoneyVolumeLkk,
                outOfTheMoneyVolumeLkk: context.AuctionOutOfTheMoneyVolumeLkk,
                inMoneyOrders: bidLkkVolumePriceLevels
                    // TODO: Тут сравнивать уровни, а не значения цен, но не забыть про вычисляемый уровень
                    .Where(p => p.PriceLevel.PriceChf.IsApparentlyGreateOrEquals(context.LkkPriceChf))
                    .Select(p => CreateInMoneyOrder(p.BidLkkVolumeCalculations, p.PriceLevel.PriceChf,
                        context.LkkPriceChf))
                    .ToImmutableArray(),
                outOfMoneyOrders: bidLkkVolumePriceLevels
                    // TODO: Тут сравнивать уровни, а не значения цен, но не забыть про вычисляемый уровень
                    .Where(p => p.PriceLevel.PriceChf.IsApparentlyLessOrEquals(context.LkkPriceChf))
                    .Select(p => CreateOutOfTheMoneyOrder(p.BidLkkVolumeCalculations, p.PriceLevel.PriceChf,
                        context.LkkPriceChf))
                    .ToImmutableArray(),
                bids: bidLkkVolumePriceLevels
                    .SelectMany(p => p.BidLkkVolumeCalculations)
                    .ToImmutableDictionary(
                        b => b.BidCalculation.ClientId,
                        b => new OrderbookBid(
                            b.BidCalculation.ClientId,
                            b.BidCalculation.LimitPriceChf,
                            context.LkkPriceChf,
                            GetOrderbookBidState(b.BidCalculation.State),
                            b.BidCalculation.AssetVolumes,
                            b.AssetVolumesLkk,
                            b.InMoneyAssetVolumesLkk))
            );
        }

        private static OrderbookBidState GetOrderbookBidState(BidCalculationState sourceState)
        {
            switch (sourceState)
            {
                case BidCalculationState.InMoney:
                    return OrderbookBidState.InMoney;

                case BidCalculationState.OutOfTheMoney:
                    return OrderbookBidState.OutOfTheMoney;

                case BidCalculationState.PartiallyInMoney:
                    return OrderbookBidState.PartiallyInMoney;

                default:
                    throw new ArgumentOutOfRangeException(nameof(sourceState), sourceState, "Invalid BidCalculationState");
            }
        }

        private PriceLevelBidLkkVolumeCalculation[] CalculatePriceLevelBidLkkVolumes(AuctionPriceLevel[] priceLevels, double lkkPriceChf)
        {
            return priceLevels
                .Select(p => new PriceLevelBidLkkVolumeCalculation(p, p.BidCalculations
                    .Select(b => new BidLkkVolumesCalculation(_assetExchangeService, b, lkkPriceChf))
                    .ToArray()))
                .ToArray();
        }

        private static Order CreateInMoneyOrder(IEnumerable<BidLkkVolumesCalculation> bidLkkVolumesCalculations, double priceLevelChf, double lkkPriceChf)
        {
            var investors = 0;
            var volumeChf = 0d;
            var volumeLkk = 0d;

            foreach (var volumeCalculation in bidLkkVolumesCalculations)
            {
                if (volumeCalculation.BidCalculation.State == BidCalculationState.InMoney)
                {
                    volumeChf += volumeCalculation.BidCalculation.VolumeChf;
                    investors++;
                }
                else if (volumeCalculation.BidCalculation.State == BidCalculationState.PartiallyInMoney)
                {
                    volumeLkk += volumeCalculation.InMoneyAssetVolumesLkk.Sum(a => a.Value);
                    investors++;
                }
            }
 
            return new Order
            {
                Investors = investors,
                Price = priceLevelChf,
                Volume = volumeChf / lkkPriceChf + volumeLkk
            };
        }

        private static Order CreateOutOfTheMoneyOrder(IEnumerable<BidLkkVolumesCalculation> bidLkkVolumesCalculations, double priceLevelChf, double lkkPriceChf)
        {
            var investors = 0;
            var volumeChf = 0d;
            var volumeLkk = 0d;

            foreach (var volumeCalculation in bidLkkVolumesCalculations)
            {
                if (volumeCalculation.BidCalculation.State == BidCalculationState.OutOfTheMoney)
                {
                    volumeChf += volumeCalculation.BidCalculation.VolumeChf;
                    investors++;
                }
                else if (volumeCalculation.BidCalculation.State == BidCalculationState.PartiallyInMoney)
                {
                    volumeLkk += volumeCalculation.BidCalculation.VolumeChf / lkkPriceChf -
                                 volumeCalculation.InMoneyAssetVolumesLkk.Sum(a => a.Value);
                    investors++;
                }
            }

            return new Order
            {
                Investors = investors,
                Price = priceLevelChf,
                Volume = volumeChf / lkkPriceChf + volumeLkk
            };
        }

        private void TrySaleWithPriceLevel(RenderContext context, int testPriceLevel, AuctionPriceLevel[] priceLevels)
        {
            context.LkkPriceChf = priceLevels[testPriceLevel].PriceChf;
            context.AuctionInMoneyVolumeLkk = 0d;
            context.AuctionOutOfTheMoneyVolumeLkk = 0d;
            context.PrevTestPriceLevelAuctionVolumeChf = context.AuctionVolumeChf;
            context.AuctionVolumeChf = 0d;

            // Iterate through price levels (from high to low price) up to current test price level
            // and aggregate their bid volumes, to see if we scored enough bid volumes to sale all LKK
            // at current testPriceLevel. Name it "Aggregation price level"
            for (var aggregationPriceLevel = 0; aggregationPriceLevel < priceLevels.Length; ++aggregationPriceLevel)
            {
                // Calculate lower levels only when all LKK sold, and correct price is found
                if (!context.IsAllLotsSold && aggregationPriceLevel > testPriceLevel)
                {
                    return;
                }

                var result = SalePriceLevel(context, priceLevels[aggregationPriceLevel]);

                switch (result)
                {
                    case CalculationContinuation.RestartCurrentTestPriceLevel:
                        aggregationPriceLevel = -1;
                        context.AuctionInMoneyVolumeLkk = 0d;
                        context.AuctionOutOfTheMoneyVolumeLkk = 0d;
                        context.AuctionVolumeChf = 0d;
                        break;

                    case CalculationContinuation.Continue:
                        break;

                    default:
                        throw new IndexOutOfRangeException($"Unknown CalculationContinuation value: {result}");
                }
            }
        }

        private CalculationContinuation SalePriceLevel(RenderContext context, AuctionPriceLevel aggregationPriceLevel)
        {
            if (!context.IsAllLotsSold)
            {
                // Small bids first
                var currentPriceBidCalculations = aggregationPriceLevel
                    .BidCalculations
                    .OrderBy(b => b.VolumeChf)
                    .ToArray();

                foreach (var bidCalculation in currentPriceBidCalculations)
                {
                    var result = ProcessBid(context, bidCalculation, aggregationPriceLevel);

                    switch (result)
                    {
                        case CalculationContinuation.RestartCurrentTestPriceLevel:
                            return result;

                        case CalculationContinuation.Continue:
                            break;

                        default:
                            throw new IndexOutOfRangeException($"Unknown CalculationContinuation value: {result}");
                    }
                }
            }
            else
            {
                foreach (var bid in aggregationPriceLevel.BidCalculations)
                {
                    context.AuctionOutOfTheMoneyVolumeLkk += bid.VolumeChf / context.LkkPriceChf;
                    bid.SetOutOfTheMoneyState();
                }
            }

            return CalculationContinuation.Continue;
        }

        private CalculationContinuation ProcessBid(RenderContext context, BidCalculation bidCalculation, AuctionPriceLevel aggregationPriceLevel)
        {
            if (context.IsAllLotsSold)
            {
                context.AuctionOutOfTheMoneyVolumeLkk += bidCalculation.VolumeChf / context.LkkPriceChf;
                bidCalculation.SetOutOfTheMoneyState();

                return CalculationContinuation.Continue;
            }

            context.AuctionVolumeChf += bidCalculation.VolumeChf;

            var nextAuctionVolumeLkk = context.AuctionVolumeChf / context.LkkPriceChf;

            if (nextAuctionVolumeLkk >= _totalAuctionVolumeLkk)
            {
                if (!context.IsAutoFitPriceCase && aggregationPriceLevel.PriceChf > context.LkkPriceChf)
                {
                    // All LKK sold, but we don`t reach bids in test price level yet,
                    // Calculate price which allow to sale exactly all LKK to bids up to
                    // current aggregation price level
                    context.IsAutoFitPriceCase = true;
                    context.LkkPriceChf = context.PrevTestPriceLevelAuctionVolumeChf / _totalAuctionVolumeLkk;
                    // And continue calculating from the current aggregation price level (recalculate it again)
                    return CalculationContinuation.RestartCurrentTestPriceLevel;
                }

                ProcessClosingBid(context, bidCalculation, nextAuctionVolumeLkk);

                return CalculationContinuation.Continue;
            }

            context.AuctionInMoneyVolumeLkk = nextAuctionVolumeLkk;
            bidCalculation.SetInMoneyState();

            return CalculationContinuation.Continue;
        }

        private void ProcessClosingBid(RenderContext context, BidCalculation bidCalculation, double nextAuctionVolumeLkk)
        {
            var inMoneyBidVolumeLkk = _totalAuctionVolumeLkk - context.AuctionInMoneyVolumeLkk;
            var outOfTheMoneyBidVolumeLkk = nextAuctionVolumeLkk - _totalAuctionVolumeLkk;

            // Grand big enought closing bid cut offs only or entire bid despite of it`s volume
            if (inMoneyBidVolumeLkk > _minClosingBidCutoffVolumeLkk || outOfTheMoneyBidVolumeLkk.IsApparentlyEquals(0))
            {
                context.AuctionInMoneyVolumeLkk += inMoneyBidVolumeLkk;

                if (outOfTheMoneyBidVolumeLkk > 0)
                {
                    context.AuctionOutOfTheMoneyVolumeLkk += outOfTheMoneyBidVolumeLkk;

                    // Take every asset proportionaly to rest of the bid
                    var inMoneyBidRate = inMoneyBidVolumeLkk * context.LkkPriceChf / bidCalculation.VolumeChf;
                    var inMoneyBidAssetVolumes = bidCalculation.AssetVolumes
                        .Select(i => new KeyValuePair<string, double>(i.Key, i.Value * inMoneyBidRate))
                        .ToImmutableArray();

                    bidCalculation.SetPartiallyInMoneyState(inMoneyBidAssetVolumes);
                }
                else
                {
                    bidCalculation.SetInMoneyState();
                }
            }
            else
            {
                context.AuctionOutOfTheMoneyVolumeLkk += bidCalculation.VolumeChf / context.LkkPriceChf;
                bidCalculation.SetOutOfTheMoneyState();
            }

            context.IsAllLotsSold = true;
        }

        private AuctionPriceLevel[] GetPriceLevels(IImmutableList<IClientBid> clientBids)
        {
            return clientBids
                .GroupBy(i => i.LimitPriceChf)
                .OrderByDescending(g => g.Key)
                .Select(g => new AuctionPriceLevel
                {
                    PriceChf = g.Key,
                    BidCalculations = g
                        .Select(i => new BidCalculation(
                            _assetExchangeService,
                            i.ClientId,
                            i.LimitPriceChf,
                            i.AssetVolumes))
                        .ToArray()
                })
                .ToArray();
        }
    }
}