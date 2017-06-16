using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DutchAuction.Core.Domain.Auction;
using DutchAuction.Core.Services.Auction;

namespace DutchAuction.Services.Auction
{
    public class BidsService : IBidsService
    {
        private readonly Dictionary<string, ClientBid> _bids;
        
        public BidsService()
        {
            _bids = new Dictionary<string, ClientBid>();
        }

        public IImmutableList<IClientBid> GetAll()
        {
            lock (_bids)
            {
                return _bids.Values.Cast<IClientBid>().ToImmutableArray();
            }
        }

        public IClientBid TryGetBid(string clientId)
        {
            lock (_bids)
            {
                _bids.TryGetValue(clientId, out ClientBid bid);

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

                var bid = new ClientBid(clientId, price, assetId, volume);
                
                _bids.Add(clientId, bid);
            }

            return AuctionOperationResult.Ok;
        }

        public AuctionOperationResult SetPrice(string clientId, double price)
        {
            lock (_bids)
            {
                if (!_bids.TryGetValue(clientId, out ClientBid bid))
                {
                    return AuctionOperationResult.BidNotFound;
                }

                if (bid.LimitPriceChf > price)
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
                if (!_bids.TryGetValue(clientId, out ClientBid bid))
                {
                    return AuctionOperationResult.BidNotFound;
                }

                var oldVolume = bid.TryGetVolume(assetId);

                if (oldVolume > volume)
                {
                    return AuctionOperationResult.VolumeIsLessThanCurrentBidAssetVolume;
                }

                _bids[clientId] = bid.SetVolume(assetId, volume);
            }

            return AuctionOperationResult.Ok;
        }
    }
}