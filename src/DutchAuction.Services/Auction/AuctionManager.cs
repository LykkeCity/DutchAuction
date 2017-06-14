using System;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using DutchAuction.Core;
using DutchAuction.Core.Domain.Auction;
using DutchAuction.Core.Services.Auction;

namespace DutchAuction.Services.Auction
{
    public class AuctionManager : 
        IAuctionManager,
        IDisposable
    {
        private IAuctionEventsManager _auctionEventsManager;
        private readonly IBidsService _bidsService;
        private readonly IOrderbookService _orderbookService;
        private readonly IPriceHistoryService _priceHistoryService;
        private readonly ILog _log;
        private readonly TimeSpan _orderbookUpdatePeriod;
        private Timer _orderbookUpdateTimer;
        private Orderbook _latestOrderbook;

        public AuctionManager(
            IAuctionEventsManager auctionEventsManager, 
            IBidsService bidsService,
            IOrderbookService orderbookService,
            IPriceHistoryService priceHistoryService,
            ILog log,
            TimeSpan orderbookUpdatePeriod)
        {
            _auctionEventsManager = auctionEventsManager;
            _bidsService = bidsService;
            _orderbookService = orderbookService;
            _priceHistoryService = priceHistoryService;
            _log = log;
            _orderbookUpdatePeriod = orderbookUpdatePeriod;
        }

        public void Start()
        {
            try
            {
                var bids = _auctionEventsManager.GetAllAsync().Result;

                var realBidsManager = _auctionEventsManager;

                _auctionEventsManager = new DummyAuctionEventsManager();

                foreach (var bid in bids)
                {
                    ReplayBid(bid);
                }

                _auctionEventsManager = realBidsManager;

                UpdateOrderbookAsync().Wait();

                _orderbookUpdateTimer = new Timer(async s => await OnUpdateOrderbookTimerAsync(), null, _orderbookUpdatePeriod, Timeout.InfiniteTimeSpan);
            }
            catch (Exception ex)
            {
                _log.WriteErrorAsync(Constants.ComponentName, null, null, ex).Wait();
                throw;
            }
        }

        public Orderbook GetOrderbook()
        {
            return _latestOrderbook;
        }

        public IBid TryGetBid(string clientId)
        {
            return _bidsService.TryGetBid(clientId);
        }

        public AuctionOperationResult StartBidding(string clientId, string assetId, double price, double volume, DateTime date)
        {
            var result = _bidsService.StartBidding(clientId, assetId, price, volume);

            if (result != AuctionOperationResult.Ok)
            {
                return result;
            }

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

            _auctionEventsManager.Add(AuctionEvent.CreateSetAssetVolume(clientId, assetId, volume, date));

            return AuctionOperationResult.Ok;
        }

        public void Dispose()
        {
            _orderbookUpdateTimer?.Dispose();
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

        private async Task OnUpdateOrderbookTimerAsync()
        {
            _orderbookUpdateTimer.Change(Timeout.Infinite, Timeout.Infinite);

            try
            {
                await UpdateOrderbookAsync();
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync(Constants.ComponentName, null, null, ex);
            }
            finally
            {
                _orderbookUpdateTimer.Change(_orderbookUpdatePeriod, Timeout.InfiniteTimeSpan);
            }
        }

        private async Task UpdateOrderbookAsync()
        {
            var orderbook = _orderbookService.Render();

            _latestOrderbook = orderbook;

            await _priceHistoryService.PublishAsync(
                orderbook.CurrentPrice,
                orderbook.CurrentInMoneyVolume,
                orderbook.CurrentOutOfTheMoneyVolume);
        }
    }
}