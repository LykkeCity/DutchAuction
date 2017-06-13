using System.Collections.Generic;
using DutchAuction.Core.Services.Assets;
using DutchAuction.Core.Services.Auction;
using DutchAuction.Services.Auction.Models;

namespace DutchAuction.Services.Auction
{
    public class BidsService : IBidsService
    {
        private readonly IAssetPairsManager _assetPairsManager;
        private readonly Dictionary<string, Bid> _bids;
        
        public BidsService(IAssetPairsManager assetPairsManager)
        {
            _assetPairsManager = assetPairsManager;

            _bids = new Dictionary<string, Bid>();
        }

        public AuctionOperationResult StartBidding(string clientId, string assetId, double price, double volume)
        {
            lock (_bids)
            {
                if(_bids.ContainsKey(clientId))
                {
                    return AuctionOperationResult.ClientHasAlreadyDoneBid;
                }

                var bid = new Bid
                {
                    Price = price
                };

                bid.AssetVolumes.Add(assetId, volume);

                _bids.Add(clientId, bid);
            }

            return AuctionOperationResult.Ok;
        }

        public AuctionOperationResult SetPrice(string clientId, double price)
        {
            lock (_bids)
            {
                if (!_bids.TryGetValue(clientId, out Bid bid))
                {
                    return AuctionOperationResult.BidNotFound;
                }

                if (bid.Price > price)
                {
                    return AuctionOperationResult.PriceIsLessThanCurrentBidPrice;
                }

                bid.Price = price;
            }

            return AuctionOperationResult.Ok;
        }

        public AuctionOperationResult SetAssetVolume(string clientId, string assetId, double volume)
        {
            lock (_bids)
            {
                if (!_bids.TryGetValue(clientId, out Bid bid))
                {
                    return AuctionOperationResult.BidNotFound;
                }

                bid.AssetVolumes.TryGetValue(assetId, out double oldVolume);

                if (oldVolume > volume)
                {
                    return AuctionOperationResult.VolumeIsLessThanCurrentBidAssetVolume;
                }

                bid.AssetVolumes[assetId] = volume;
            }

            return AuctionOperationResult.Ok;
        }
    }
}