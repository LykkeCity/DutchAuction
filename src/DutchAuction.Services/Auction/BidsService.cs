using System.Collections.Generic;
using System.Linq;
using DutchAuction.Core.Domain.Auction;
using DutchAuction.Core.Services.Auction;

namespace DutchAuction.Services.Auction
{
    public class BidsService : IBidsService
    {

        private readonly Dictionary<string, Bid> _bids;
        
        public BidsService()
        {
            _bids = new Dictionary<string, Bid>();
        }

        public IBid[] GetAll()
        {
            lock (_bids)
            {
                return _bids.Values.Cast<IBid>().ToArray();
            }
        }

        public IBid TryGetBid(string clientId)
        {
            lock (_bids)
            {
                _bids.TryGetValue(clientId, out Bid bid);

                return bid;
            }
        }

        public AuctionOperationResult StartBidding(string clientId, string assetId, double price, double volume)
        {
            lock (_bids)
            {
                if(_bids.ContainsKey(clientId))
                {
                    return AuctionOperationResult.ClientHasAlreadyDoneBid;
                }

                var bid = new Bid(clientId, price, assetId, volume);
                
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

                _bids[clientId] = bid.SetPrice(price);
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

                _bids[clientId] = bid.SetVolume(assetId, volume);
            }

            return AuctionOperationResult.Ok;
        }

        public void MarkBidAsPartiallyInMoney(string clientId, IDictionary<string, double> inMoneyBidAssetVolumes)
        {
            lock (_bids)
            {
                _bids[clientId] = _bids[clientId].SetPartiallyInMoneyState(inMoneyBidAssetVolumes);
            }
        }

        public void MarkBidAsInMoney(string clientId)
        {
            lock (_bids)
            {
                _bids[clientId] = _bids[clientId].SetInMoneyState();
            }
        }

        public void MarkBidAsOutOfTheMoney(string clientId)
        {
            lock (_bids)
            {
                _bids[clientId] = _bids[clientId].SetOutOfTheMoneyState();
            }
        }
    }
}