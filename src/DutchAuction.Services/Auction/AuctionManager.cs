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

        public void StartBidding(string clientId, string assetId, double price, double volume, DateTime date)
        {
            _accountsService.Add(clientId, assetId, price, volume);
            _orderbookService.OnClientAccountAdded(clientId, assetId, price, volume);

            _bidsManager.Add(Bid.CreateStartBidding(clientId, assetId, price, volume, date));
        }

        public void AcceptPriceBid(string clientId, double price, DateTime date)
        {
            _accountsService.SetPrice(clientId, price);
            _orderbookService.OnPriceSet(clientId, price);

            _bidsManager.Add(Bid.CreateSetPrice(clientId, price, date));
        }

        public void AcceptVolumeBid(string clientId, string assetId, double volume, DateTime date)
        {
            _accountsService.SetAssetVolume(clientId, assetId, volume);
            _orderbookService.OnAssetVolumeSet(clientId, assetId, volume);

            _bidsManager.Add(Bid.CreateSetAssetVolume(clientId, assetId, volume, date));
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