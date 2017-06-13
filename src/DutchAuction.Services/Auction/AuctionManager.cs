using System;
using DutchAuction.Core.Domain.Auction;
using DutchAuction.Core.Services.Auction;

namespace DutchAuction.Services.Auction
{
    public class AuctionManager : IAuctionManager
    {
        private IBidsManager _bidsManager;
        private readonly IClientAccountsService _accountsService;
        private readonly IOrderbookService _orderbookService;

        public AuctionManager(
            IBidsManager bidsManager, 
            IClientAccountsService accountsService,
            IOrderbookService orderbookService)
        {
            _bidsManager = bidsManager;
            _accountsService = accountsService;
            _orderbookService = orderbookService;
        }

        public void Start()
        {
            var bids = _bidsManager.GetAllAsync().Result;

            var realBidsManager = _bidsManager;

            _bidsManager = new DummyBidsManager();

            foreach (var bid in bids)
            {
                ReplayBid(bid);
            }

            _bidsManager = realBidsManager;
        }

        public AuctionOperationResult StartBidding(string clientId, string assetId, double price, double volume, DateTime date)
        {
            var result = _accountsService.StartBidding(clientId, assetId, price, volume);

            if (result != AuctionOperationResult.Ok)
            {
                return result;
            }

            _orderbookService.OnClientAccountAdded(clientId, assetId, price, volume);

            _bidsManager.Add(Bid.CreateStartBidding(clientId, assetId, price, volume, date));

            return AuctionOperationResult.Ok;
        }

        public AuctionOperationResult AcceptPriceBid(string clientId, double price, DateTime date)
        {
            var result = _accountsService.SetPrice(clientId, price);

            if (result != AuctionOperationResult.Ok)
            {
                return result;
            }

            _orderbookService.OnPriceSet(clientId, price);

            _bidsManager.Add(Bid.CreateSetPrice(clientId, price, date));

            return AuctionOperationResult.Ok;
        }

        public AuctionOperationResult AcceptVolumeBid(string clientId, string assetId, double volume, DateTime date)
        {
            var result = _accountsService.SetAssetVolume(clientId, assetId, volume);

            if (result != AuctionOperationResult.Ok)
            {
                return result;
            }

            _orderbookService.OnAssetVolumeSet(clientId, assetId, volume);

            _bidsManager.Add(Bid.CreateSetAssetVolume(clientId, assetId, volume, date));

            return AuctionOperationResult.Ok;
        }

        private void ReplayBid(IBid bid)
        {
            switch (bid.Type)
            {
                case BidType.StartBidding:
                    StartBidding(bid.ClientId, bid.AssetId, bid.Price, bid.Volume, bid.Date);
                    break;

                case BidType.SetPrice:
                    AcceptPriceBid(bid.ClientId, bid.Price, bid.Date);
                    break;

                case BidType.SetAssetVolume:
                    AcceptVolumeBid(bid.ClientId, bid.AssetId, bid.Volume, bid.Date);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(bid), bid.Type, "Unknown bid type");
            }
        }
    }
}