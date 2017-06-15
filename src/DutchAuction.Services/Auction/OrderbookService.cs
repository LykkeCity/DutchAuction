﻿using System.Collections.Generic;
using System.Linq;
using DutchAuction.Core;
using DutchAuction.Core.Domain.Auction;
using DutchAuction.Core.Services.Assets;
using DutchAuction.Core.Services.Auction;

namespace DutchAuction.Services.Auction
{
    public class OrderbookService : IOrderbookService
    {
        private class BidVolume
        {
            public string ClientId { get; set; }
            public double VolumeChf { get; set; }
            public IBid Bid { get; set; }
        }

        private class AuctionPriceLevel
        {
            public double PriceChf { get; set; }
            public BidVolume[] BidVolumes { get; set; }
        }

        private class RenderContext
        {
            public double AuctionVolumeChf { get; set; }
            public double PrevTestPriceLevelAuctionVolumeChf { get; set; }
            public double AuctionInMoneyVolumeLkk { get; set; }
            public double AuctionOutOfTheMoneyVolumeLkk { get; set; }
            public double AuctionPriceChf { get; set; }
            public bool IsAllLotsSold { get; set; }
            public bool IsAutoFitPriceCase { get; set; }
        }

        private readonly IAssetExchangeService _assetExchangeService;
        private readonly IBidsService _bidsService;
        private readonly double _totalAuctionVolumeLkk;
        private readonly double _minClosingBidCutoffVolumeLkk;

        public OrderbookService(
            IAssetExchangeService assetExchangeService,
            IBidsService bidsService,
            double totalAuctionVolumeLkk,
            double minClosingBidCutoffVolumeLkk)
        {
            _assetExchangeService = assetExchangeService;
            _bidsService = bidsService;
            _totalAuctionVolumeLkk = totalAuctionVolumeLkk;
            _minClosingBidCutoffVolumeLkk = minClosingBidCutoffVolumeLkk;
        }

        public Orderbook Render()
        {
            var priceLevels = GetPriceLevels();
            var context = new RenderContext();

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

            return new Orderbook
            {
                CurrentPrice = context.AuctionPriceChf,
                CurrentInMoneyVolume = context.AuctionInMoneyVolumeLkk,
                CurrentOutOfTheMoneyVolume = context.AuctionOutOfTheMoneyVolumeLkk,
                InMoneyOrders = priceLevels
                    .Where(p => p.PriceChf.IsApparentlyGreateOrEquals(context.AuctionPriceChf))
                    .Select(p => new Order
                    {
                        Investors = p.BidVolumes.Count(b => b.Bid.State == BidState.InMoney || b.Bid.State == BidState.PartiallyInMoney),
                        Price = p.PriceChf,
                        Volume = CalculateInMoneyOrderVolume(p, context)

                    })
                    .ToArray(),
                OutOfMoneyOrders = priceLevels
                    .Where(p => p.PriceChf.IsApparentlyLessOrEquals(context.AuctionPriceChf))
                    .Select(p => new Order
                    {
                        Investors = p.BidVolumes.Count(b => b.Bid.State == BidState.OutOfTheMoney || b.Bid.State == BidState.PartiallyInMoney),
                        Price = p.PriceChf,
                        Volume = CalculateOutOfTheMoneyOrderVolume(p, context)
                    })
                    .ToArray()
            };
        }

        private double CalculateInMoneyOrderVolume(AuctionPriceLevel p, RenderContext context)
        {
            var inMoneyValueChf = p.BidVolumes
                .Where(b => b.Bid.State == BidState.InMoney)
                .Sum(b => b.VolumeChf);

            var partiallyInMoneyValueChf = p.BidVolumes
                .Where(b => b.Bid.State == BidState.PartiallyInMoney)
                .Select(b => b.Bid.InMoneyAssetVolumes.Sum(a => _assetExchangeService.Exchange(a.Value, a.Key, "CHF")))
                .Sum(v => v);

            // Convert volume to LKK
            return (inMoneyValueChf + partiallyInMoneyValueChf) / context.AuctionPriceChf;
        }

        private double CalculateOutOfTheMoneyOrderVolume(AuctionPriceLevel p, RenderContext context)
        {
            var outOfTheMoneyValueChf = p.BidVolumes
                .Where(b => b.Bid.State == BidState.OutOfTheMoney)
                .Sum(b => b.VolumeChf);

            var partiallyInMoneyValueChf = p.BidVolumes
                .Where(b => b.Bid.State == BidState.PartiallyInMoney)
                .Select(b => b.VolumeChf - b.Bid.InMoneyAssetVolumes.Sum(a => _assetExchangeService.Exchange(a.Value, a.Key, "CHF")))
                .Sum(v => v);

            // Convert volume to LKK
            return (outOfTheMoneyValueChf + partiallyInMoneyValueChf) / context.AuctionPriceChf;
        }

        private void TrySaleWithPriceLevel(RenderContext context, int testPriceLevel, AuctionPriceLevel[] priceLevels)
        {
            context.AuctionPriceChf = priceLevels[testPriceLevel].PriceChf;
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

                SalePriceLevel(context, priceLevels[aggregationPriceLevel]);
            }
        }

        private void SalePriceLevel(RenderContext context, AuctionPriceLevel aggregationPriceLevel)
        {
            if (!context.IsAllLotsSold)
            {
                // Small bids first
                var currentPriceBids = aggregationPriceLevel
                    .BidVolumes
                    .OrderBy(b => b.VolumeChf);

                foreach (var bidVolume in currentPriceBids)
                {
                    ProcessBid(context, bidVolume, aggregationPriceLevel);
                }
            }
            else
            {
                foreach (var bid in aggregationPriceLevel.BidVolumes)
                {
                    context.AuctionOutOfTheMoneyVolumeLkk += bid.VolumeChf / context.AuctionPriceChf;
                    _bidsService.MarkBidAsOutOfTheMoney(bid.ClientId);
                }
            }
        }

        private void ProcessBid(RenderContext context, BidVolume bidVolume, AuctionPriceLevel aggregationPriceLevel)
        {
            if (context.IsAllLotsSold)
            {
                context.AuctionOutOfTheMoneyVolumeLkk += bidVolume.VolumeChf / context.AuctionPriceChf;
                _bidsService.MarkBidAsOutOfTheMoney(bidVolume.ClientId);
                return;
            }

            context.AuctionVolumeChf += bidVolume.VolumeChf;

            while (true)
            {
                var nextAuctionVolumeLkk = context.AuctionVolumeChf / context.AuctionPriceChf;

                if (nextAuctionVolumeLkk >= _totalAuctionVolumeLkk)
                {
                    if (!context.IsAutoFitPriceCase && aggregationPriceLevel.PriceChf > context.AuctionPriceChf)
                    {
                        // All LKK sold, but we don`t reach bids in test price level yet,
                        // Calculate price which allow to sale exactly all LKK to bids up to
                        // current aggregation price level
                        context.IsAutoFitPriceCase = true;
                        context.AuctionPriceChf = context.PrevTestPriceLevelAuctionVolumeChf / _totalAuctionVolumeLkk;
                        // And continue calculating from current bid (recalculate it again)
                        continue;
                    }

                    ProcessClosingBid(context, bidVolume, nextAuctionVolumeLkk);

                    break;
                }

                context.AuctionInMoneyVolumeLkk = nextAuctionVolumeLkk;
                _bidsService.MarkBidAsInMoney(bidVolume.ClientId);

                break;
            }
        }

        private void ProcessClosingBid(RenderContext context, BidVolume bidVolume, double nextAuctionVolumeLkk)
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
                    var inMoneyBidRate = inMoneyBidVolumeLkk * context.AuctionPriceChf / bidVolume.VolumeChf;
                    var inMoneyBidAssetVolumes = bidVolume.Bid.AssetVolumes
                        .Select(i => new KeyValuePair<string, double>(i.Key, i.Value * inMoneyBidRate));

                    _bidsService.MarkBidAsPartiallyInMoney(bidVolume.ClientId, inMoneyBidAssetVolumes);
                }
                else
                {
                    _bidsService.MarkBidAsInMoney(bidVolume.ClientId);
                }
            }
            else
            {
                context.AuctionOutOfTheMoneyVolumeLkk += bidVolume.VolumeChf / context.AuctionPriceChf;
                _bidsService.MarkBidAsOutOfTheMoney(bidVolume.ClientId);
            }

            context.IsAllLotsSold = true;
        }

        private AuctionPriceLevel[] GetPriceLevels()
        {
            return _bidsService
                .GetAll()
                .Select(i => new
                {
                    ClientId = i.ClientId,
                    Price = i.Price,
                    // Convert volume to CHF
                    Volume = i.AssetVolumes.Sum(a => _assetExchangeService.Exchange(a.Value, a.Key, "CHF")),
                    Bid = i
                })
                .GroupBy(i => i.Price)
                .OrderByDescending(g => g.Key)
                .Select(g => new AuctionPriceLevel
                {
                    PriceChf = g.Key,
                    BidVolumes = g
                        .Select(i => new BidVolume
                        {
                            ClientId = i.ClientId,
                            VolumeChf = i.Volume,
                            Bid = i.Bid
                        })
                        .ToArray()
                })
                .ToArray();
        }
    }
}