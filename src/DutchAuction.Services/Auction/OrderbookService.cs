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
            public double Volume { get; set; }
            public IReadOnlyDictionary<string, double> AssetVolumes { get; set; }
        }

        private class PriceBidVolumes
        {
            public double Price { get; set; }
            public BidVolume[] BidVolumes { get; set; }
        }

        private readonly IAssetExchangeService _assetExchangeService;
        private readonly IBidsService _bidsService;
        private readonly double _totalAuctionVolume;
        private readonly double _minClosingBidCutoffVolume;

        public OrderbookService(
            IAssetExchangeService assetExchangeService,
            IBidsService bidsService,
            double totalAuctionVolume,
            double minClosingBidCutoffVolume)
        {
            _assetExchangeService = assetExchangeService;
            _bidsService = bidsService;
            _totalAuctionVolume = totalAuctionVolume;
            _minClosingBidCutoffVolume = minClosingBidCutoffVolume;
        }

        public Orderbook Render()
        {
            var priceBidVolumes = GetPriceBidVolumes();
            var auctionInMoneyVolume = 0d;
            var auctionOutOfTheMoneyVolume = 0d;
            var auctionPrice = 0d;
            var isAuctionClosed = false;

            foreach (var priceBidVolume in priceBidVolumes)
            {
                if (!isAuctionClosed)
                {
                    auctionPrice = priceBidVolume.Price;
                    // Small bids first
                    var currentPriceBids = priceBidVolume
                        .BidVolumes
                        .OrderBy(b => b.Volume);

                    foreach (var bid in currentPriceBids)
                    {
                        var nextAuctionVolume = auctionInMoneyVolume + bid.Volume;

                        if (nextAuctionVolume >= _totalAuctionVolume)
                        {
                            var inMoneyBidVolume = _totalAuctionVolume - auctionInMoneyVolume;
                            var outOfTheMoneyBidVolume = nextAuctionVolume - _totalAuctionVolume;

                            // Grand big enought closing bids only
                            if (inMoneyBidVolume > _minClosingBidCutoffVolume)
                            {
                                if (outOfTheMoneyBidVolume > 0)
                                {
                                    auctionInMoneyVolume += inMoneyBidVolume;
                                    auctionOutOfTheMoneyVolume += outOfTheMoneyBidVolume;

                                    // Take every asset proportionaly to rest of the bid
                                    var inMoneyBidRate = inMoneyBidVolume / bid.Volume;
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
                                auctionOutOfTheMoneyVolume += bid.Volume;
                                _bidsService.MarkBidAsOutOfTheMoney(bid.ClientId);
                            }

                            isAuctionClosed = true;
                            break;
                        }

                        auctionInMoneyVolume = nextAuctionVolume;

                        _bidsService.MarkBidAsInMoney(bid.ClientId);
                    }
                }
                else
                {
                    foreach (var bid in priceBidVolume.BidVolumes)
                    {
                        auctionOutOfTheMoneyVolume += bid.Volume;
                        _bidsService.MarkBidAsOutOfTheMoney(bid.ClientId);
                    }
                }
            }

            return new Orderbook
            {
                CurrentPrice = auctionPrice,
                CurrentInMoneyVolume = auctionInMoneyVolume,
                CurrentOutOfTheMoneyVolume = auctionOutOfTheMoneyVolume,
                Orders = priceBidVolumes
                    .Select(p => new Order
                    {
                        Investors = p.BidVolumes.Length,
                        Price = p.Price,
                        // Convert volume to LKK
                        Volume = p.BidVolumes.Sum(b => b.Volume) * auctionPrice
                    })
                    .ToArray()
            };
        }

        private PriceBidVolumes[] GetPriceBidVolumes()
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
                .Select(g => new PriceBidVolumes
                {
                    Price = g.Key,
                    BidVolumes = g
                        .Select(i => new BidVolume
                        {
                            ClientId = i.ClientId,
                            Volume = i.Volume,
                            AssetVolumes = i.AssetVolumes
                        })
                        .ToArray()
                })
                .ToArray();
        }
    }
}