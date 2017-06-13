using System;
using DutchAuction.Core.Domain.Auction;
using DutchAuction.Core.Services.Auction;

namespace DutchAuction.Services.Auction
{
    public class AuctionManager : IAuctionManager
    {
        private IAuctionEventsManager _auctionEventsManager;
        private readonly IBidsService _bidsService;
        private readonly IOrderbookService _orderbookService;

        public AuctionManager(
            IAuctionEventsManager auctionEventsManager, 
            IBidsService bidsService,
            IOrderbookService orderbookService)
        {
            _auctionEventsManager = auctionEventsManager;
            _bidsService = bidsService;
            _orderbookService = orderbookService;
        }

        public void Start()
        {
            var bids = _auctionEventsManager.GetAllAsync().Result;

            var realBidsManager = _auctionEventsManager;

            _auctionEventsManager = new DummyAuctionEventsManager();

            foreach (var bid in bids)
            {
                ReplayBid(bid);
            }

            _auctionEventsManager = realBidsManager;
        }

        public AuctionOperationResult StartBidding(string clientId, string assetId, double price, double volume, DateTime date)
        {
            var result = _bidsService.StartBidding(clientId, assetId, price, volume);

            if (result != AuctionOperationResult.Ok)
            {
                return result;
            }

            _orderbookService.OnBidAdded(clientId, assetId, price, volume);

            _auctionEventsManager.Add(AuctionEvent.CreateStartBidding(clientId, assetId, price, volume, date));

            return AuctionOperationResult.Ok;
        }

        public AuctionOperationResult AcceptPriceBid(string clientId, double price, DateTime date)
        {
            var result = _bidsService.SetPrice(clientId, price);

            if (result != AuctionOperationResult.Ok)
            {
                return result;
            }

            _orderbookService.OnBidPriceSet(clientId, price);

            _auctionEventsManager.Add(AuctionEvent.CreateSetPrice(clientId, price, date));

            return AuctionOperationResult.Ok;
        }

        public AuctionOperationResult AcceptVolumeBid(string clientId, string assetId, double volume, DateTime date)
        {
            var result = _bidsService.SetAssetVolume(clientId, assetId, volume);

            if (result != AuctionOperationResult.Ok)
            {
                return result;
            }

            _orderbookService.OnBidAssetVolumeSet(clientId, assetId, volume);

            _auctionEventsManager.Add(AuctionEvent.CreateSetAssetVolume(clientId, assetId, volume, date));

            return AuctionOperationResult.Ok;
        }

        private void ReplayBid(IAuctionEvent auctionEvent)
        {
            switch (auctionEvent.Type)
            {
                case AuctionEventType.StartBidding:
                    StartBidding(auctionEvent.ClientId, auctionEvent.AssetId, auctionEvent.Price, auctionEvent.Volume, auctionEvent.Date);
                    break;

                case AuctionEventType.SetPrice:
                    AcceptPriceBid(auctionEvent.ClientId, auctionEvent.Price, auctionEvent.Date);
                    break;

                case AuctionEventType.SetAssetVolume:
                    AcceptVolumeBid(auctionEvent.ClientId, auctionEvent.AssetId, auctionEvent.Volume, auctionEvent.Date);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(auctionEvent), auctionEvent.Type, "Unknown bid type");
            }
        }
    }
}