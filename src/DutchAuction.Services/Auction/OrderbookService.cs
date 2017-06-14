using System.Collections.Generic;
using System.Linq;
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
            public IReadOnlyDictionary<string, double> AssetVolumes { get; set; }
        }

        private class ActionPriceLevel
        {
            public double PriceChf { get; set; }
            public BidVolume[] BidVolumes { get; set; }
        }

        private class RenderContext
        {
            public double AuctionVolumeChf { get; set; }
            public double AuctionInMoneyVolumeLkk { get; set; }
            public double AuctionOutOfTheMoneyVolumeLkk { get; set; }
            public double AuctionPriceChf { get; set; }
            public bool IsAllLotsSold { get; set; }
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
            var priceBidVolumes = GetPriceBidVolumes();
            var context = new RenderContext();
            
            foreach (var priceBidVolume in priceBidVolumes)
            {
                ProcessPriceLevel(context, priceBidVolume);
            }

            return new Orderbook
            {
                CurrentPrice = context.AuctionPriceChf,
                CurrentInMoneyVolume = context.AuctionInMoneyVolumeLkk,
                CurrentOutOfTheMoneyVolume = context.AuctionOutOfTheMoneyVolumeLkk,
                Orders = priceBidVolumes
                    .Select(p => new Order
                    {
                        Investors = p.BidVolumes.Length,
                        Price = p.PriceChf,
                        // Convert volume to LKK
                        Volume = p.BidVolumes.Sum(b => b.VolumeChf) / context.AuctionPriceChf
                    })
                    .ToArray()
            };
        }

        private void ProcessPriceLevel(RenderContext context, ActionPriceLevel actionPriceLevel)
        {
            if (!context.IsAllLotsSold)
            {
                context.AuctionPriceChf = actionPriceLevel.PriceChf;
                // Small bids first
                var currentPriceBids = actionPriceLevel
                    .BidVolumes
                    .OrderBy(b => b.VolumeChf);

                foreach (var bid in currentPriceBids)
                {
                    if (ProcessBid(context, bid))
                    {
                        break;
                    }
                }
            }
            else
            {
                foreach (var bid in actionPriceLevel.BidVolumes)
                {
                    context.AuctionOutOfTheMoneyVolumeLkk += bid.VolumeChf;
                    _bidsService.MarkBidAsOutOfTheMoney(bid.ClientId);
                }
            }
        }

        private bool ProcessBid(RenderContext context, BidVolume bid)
        {
            context.AuctionVolumeChf += bid.VolumeChf;

            var nextAuctionVolumeLkk = context.AuctionInMoneyVolumeLkk +
                                       context.AuctionVolumeChf / context.AuctionPriceChf;

            if (nextAuctionVolumeLkk >= _totalAuctionVolumeLkk)
            {
                var inMoneyBidVolumeLkk = _totalAuctionVolumeLkk - context.AuctionInMoneyVolumeLkk;
                var outOfTheMoneyBidVolumeLkk = nextAuctionVolumeLkk - _totalAuctionVolumeLkk;

                // Grand big enought closing bids only
                if (inMoneyBidVolumeLkk > _minClosingBidCutoffVolumeLkk)
                {
                    context.AuctionInMoneyVolumeLkk += inMoneyBidVolumeLkk;

                    if (outOfTheMoneyBidVolumeLkk > 0)
                    {
                        context.AuctionOutOfTheMoneyVolumeLkk += outOfTheMoneyBidVolumeLkk;

                        // Take every asset proportionaly to rest of the bid
                        var inMoneyBidRate = inMoneyBidVolumeLkk * context.AuctionPriceChf / bid.VolumeChf;
                        var inMoneyBidAssetVolumes = bid.AssetVolumes
                            .ToDictionary(i => i.Key, i => i.Value * inMoneyBidRate);

                        _bidsService.MarkBidAsPartiallyInMoney(bid.ClientId, inMoneyBidAssetVolumes);
                    }
                    else
                    {
                        _bidsService.MarkBidAsInMoney(bid.ClientId);
                    }
                }
                else
                {
                    context.AuctionOutOfTheMoneyVolumeLkk += bid.VolumeChf / context.AuctionPriceChf;
                    _bidsService.MarkBidAsOutOfTheMoney(bid.ClientId);
                }

                context.IsAllLotsSold = true;

                return true;
            }

            context.AuctionInMoneyVolumeLkk = nextAuctionVolumeLkk;

            _bidsService.MarkBidAsInMoney(bid.ClientId);

            return false;
        }

        private ActionPriceLevel[] GetPriceBidVolumes()
        {
            return _bidsService
                .GetAll()
                .Select(i => new
                {
                    ClientId = i.ClientId,
                    Price = i.Price,
                    // Convert volume to CHF
                    Volume = i.AssetVolumes.Sum(a => _assetExchangeService.Exchange(a.Value, a.Key, "CHF")),
                    AssetVolumes = i.AssetVolumes
                })
                .GroupBy(i => i.Price)
                .OrderByDescending(g => g.Key)
                .Select(g => new ActionPriceLevel
                {
                    PriceChf = g.Key,
                    BidVolumes = g
                        .Select(i => new BidVolume
                        {
                            ClientId = i.ClientId,
                            VolumeChf = i.Volume,
                            AssetVolumes = i.AssetVolumes
                        })
                        .ToArray()
                })
                .ToArray();
        }
    }
}