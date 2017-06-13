using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DutchAuction.Core.Domain.Auction;
using DutchAuction.Core.Services.Assets;
using DutchAuction.Core.Services.Auction;
using DutchAuction.Services.Auction.Models;

namespace DutchAuction.Services.Auction
{
    public class OrderbookService : IOrderbookService
    {
        private class BidVolume
        {
            public string ClientId { get; set; }
            public double Volume { get; set; }
        }

        private class PriceBidVolumes
        {
            public double Price { get; set; }
            public BidVolume[] BidVolumes { get; set; }
        }

        private readonly IAssetExchangeService _assetExchangeService;
        private readonly double _totalAuctionVolume;
        private readonly double _minClosingBidCutoffVolume;
        private readonly Dictionary<string, Bid> _bids;
        private readonly ReaderWriterLockSlim _lock;

        public OrderbookService(IAssetExchangeService assetExchangeService, double totalAuctionVolume, double minClosingBidCutoffVolume)
        {
            _assetExchangeService = assetExchangeService;
            _totalAuctionVolume = totalAuctionVolume;
            _minClosingBidCutoffVolume = minClosingBidCutoffVolume;

            _bids = new Dictionary<string, Bid>();
            _lock = new ReaderWriterLockSlim();
        }

        public Orderbook Render()
        {
            _lock.EnterReadLock();

            PriceBidVolumes[] priceBidVolumes;

            try
            {
                priceBidVolumes = GetPriceBidVolumes();
            }
            finally
            {
                _lock.ExitReadLock();
            }

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
                            var cutoffBidVolume = _totalAuctionVolume - auctionInMoneyVolume;
                            var bidRestVolume = nextAuctionVolume - _totalAuctionVolume;

                            // Grand big enought closing bids only
                            if (cutoffBidVolume > _minClosingBidCutoffVolume)
                            {
                                if (bidRestVolume > 0)
                                {
                                    // TODO: Mark bid as partialy granted?
                                    auctionInMoneyVolume += cutoffBidVolume;
                                    auctionOutOfTheMoneyVolume += bidRestVolume;
                                }
                            }
                            else
                            {
                                auctionOutOfTheMoneyVolume += bid.Volume;
                            }

                            isAuctionClosed = true;
                            break;
                        }

                        auctionInMoneyVolume = nextAuctionVolume;

                        // TODO: Mark bid as granted?
                    }
                }
                else
                {
                    auctionOutOfTheMoneyVolume += priceBidVolume.BidVolumes.Sum(b => b.Volume);
                }
            }

            return new Orderbook
            {
                CurrentPrice = auctionPrice,
                CurrentInMoneyVolume = auctionInMoneyVolume,
                CurrentOutOfTheMoneyVolume =auctionOutOfTheMoneyVolume,
                Orders = priceBidVolumes
                    .Select(p => new Order
                    {
                        Investors = p.BidVolumes.Length,
                        Price = p.Price,
                        Volume = p.BidVolumes.Sum(b => b.Volume) * auctionPrice
                    })
                    .ToArray()
            };
        }

        private PriceBidVolumes[] GetPriceBidVolumes()
        {
            return _bids
                .Select(i => new
                {
                    ClientId = i.Key,
                    Price = i.Value.Price,
                    Volume = i.Value
                        .AssetVolumes
                        .Sum(a => _assetExchangeService.Exchange(a.Value, a.Key, "CHF"))
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
                            Volume = i.Volume
                        })
                        .ToArray()
                })
                .ToArray();
        }

        public void OnBidAdded(string clientId, string assetId, double price, double volume)
        {
            _lock.EnterWriteLock();

            try
            {
                var bid = new Bid
                {
                    Price = price
                };

                bid.AssetVolumes[assetId] = volume;

                _bids.Add(clientId, bid);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void OnBidPriceSet(string clientId, double price)
        {
            _lock.EnterWriteLock();

            try
            {
                _bids[clientId].Price = price;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void OnBidAssetVolumeSet(string clientId, string assetId, double volume)
        {
            _lock.EnterWriteLock();

            try
            {
                _bids[clientId].AssetVolumes[assetId] = volume;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}