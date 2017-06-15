using System.Collections.Generic;
using System.Linq;
using DutchAuction.Core.Domain.Auction;
using DutchAuction.Core.Services.Assets;
using DutchAuction.Core.Services.Auction;
using DutchAuction.Services.Auction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DutchAuction.UnitTests
{
    [TestClass]
    public class OrderbookTests
    {
        private const double Delta = 0.0000000001;

        private Mock<IAssetExchangeService> _assetExchangeServiceMock;
        private OrderbookService _orderbookService;
        private Mock<IBidsService> _bidsServiceMock;
        private List<Bid> _bids;

        [TestInitialize]
        public void InitializeTest()
        {
            _assetExchangeServiceMock = new Mock<IAssetExchangeService>();
            _bidsServiceMock = new Mock<IBidsService>();

            _bids = new List<Bid>();

            _bidsServiceMock
                .Setup(s => s.GetAll())
                .Returns(() => _bids.Cast<IBid>().ToArray());
            _bidsServiceMock
                .Setup(s => s.MarkBidAsInMoney(It.IsAny<string>()))
                .Callback<string>(clientId => _bids.Single(b => b.ClientId == clientId).SetInMoneyState());
            _bidsServiceMock
                .Setup(s => s.MarkBidAsOutOfTheMoney(It.IsAny<string>()))
                .Callback<string>(clientId => _bids.Single(b => b.ClientId == clientId).SetOutOfTheMoneyState());
            _bidsServiceMock
                .Setup(s => s.MarkBidAsPartiallyInMoney(It.IsAny<string>(), It.IsAny<IEnumerable<KeyValuePair<string, double>>>()))
                .Callback<string, IEnumerable<KeyValuePair<string, double>>>((clientId, outOfTheMoneyAssetValues) => 
                    _bids.Single(b => b.ClientId == clientId).SetPartiallyInMoneyState(outOfTheMoneyAssetValues));

            _orderbookService = new OrderbookService(_assetExchangeServiceMock.Object, _bidsServiceMock.Object, 
                totalAuctionVolumeLkk: 5000, 
                minClosingBidCutoffVolumeLkk: 10);
        }

        [TestMethod]
        public void Is_two_bids_with_one_price_perofrms_single_order()
        {
            // Arrange
            _assetExchangeServiceMock
                .Setup(s => s.Exchange(It.IsAny<double>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns<double, string, string>((amount, baseAssetId, targetAssetId) => amount);

            _bids.AddRange(new[]
            {
                new Bid("client1", 100, "USD", 10),
                new Bid("client2", 100, "USD", 20)
            });

            // Act
            var orderbook = _orderbookService.Render();

            // Assert
            Assert.AreEqual(1, orderbook.InMoneyOrders.Length);
            Assert.AreEqual(100d, orderbook.InMoneyOrders[0].Price, Delta);
        }

        [TestMethod]
        public void Is_orderbook_correct_when_not_all_lkk_sold()
        {
            // Asset
            _assetExchangeServiceMock
                .Setup(s => s.Exchange(It.IsAny<double>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns<double, string, string>((amount, baseAssetId, targetAssetId) => amount);

            _bids.AddRange(new []
            {
                new Bid("client1", 1.5, "USD", 100).SetVolumeFluently("CHF", 250),
                new Bid("client2", 2.0, "USD", 200),
                new Bid("client3", 1.2, "USD", 300).SetVolumeFluently("EUR", 150),
                new Bid("client4", 1.2, "USD", 300),
            });
            
            // Act
            var orderbook = _orderbookService.Render();

            // Assert
            Assert.AreEqual(1.2, orderbook.CurrentPrice, Delta);
            Assert.AreEqual(1300 / 1.2, orderbook.CurrentInMoneyVolume, Delta);
            Assert.AreEqual(0.0, orderbook.CurrentOutOfTheMoneyVolume, Delta);

            Assert.AreEqual(3, orderbook.InMoneyOrders.Length);

            Assert.AreEqual(2.0, orderbook.InMoneyOrders[0].Price, Delta);
            Assert.AreEqual(1, orderbook.InMoneyOrders[0].Investors);
            Assert.AreEqual(200 / 1.2, orderbook.InMoneyOrders[0].Volume, Delta);

            Assert.AreEqual(1.5, orderbook.InMoneyOrders[1].Price, Delta);
            Assert.AreEqual(1, orderbook.InMoneyOrders[1].Investors);
            Assert.AreEqual(350 / 1.2, orderbook.InMoneyOrders[1].Volume, Delta);

            Assert.AreEqual(BidState.InMoney, _bids[0].State);
            Assert.AreEqual(BidState.InMoney, _bids[1].State);
            Assert.AreEqual(BidState.InMoney, _bids[2].State);
            Assert.AreEqual(BidState.InMoney, _bids[3].State);
        }

        [TestMethod]
        public void Is_orderbook_correct_when_all_lkk_sold_and_entire_closing_bid_fit_in()
        {
            // Asset
            _assetExchangeServiceMock
                .Setup(s => s.Exchange(It.IsAny<double>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns<double, string, string>((amount, baseAssetId, targetAssetId) => amount);

            _bids.AddRange(new[]
            {
                new Bid("client1", 1.5, "USD", 250).SetVolumeFluently("CHF", 125),
                new Bid("client2", 2.0, "USD", 125),
                new Bid("client3", 1.2, "USD", 300).SetVolumeFluently("EUR", 150),
                new Bid("client4", 1.2, "USD", 400),
                new Bid("client5", 0.5, "USD", 250).SetVolumeFluently("EUR", 500),
                new Bid("client6", 0.5, "EUR", 400)
            });

            // Act
            var orderbook = _orderbookService.Render();

            // Assert
            Assert.AreEqual(0.5, orderbook.CurrentPrice, Delta);
            Assert.AreEqual(2500 / 0.5, orderbook.CurrentInMoneyVolume, Delta);
            Assert.AreEqual(0.0, orderbook.CurrentOutOfTheMoneyVolume, Delta);

            Assert.AreEqual(4, orderbook.InMoneyOrders.Length);

            Assert.AreEqual(2.0, orderbook.InMoneyOrders[0].Price, Delta);
            Assert.AreEqual(1, orderbook.InMoneyOrders[0].Investors);
            Assert.AreEqual(125 / 0.5, orderbook.InMoneyOrders[0].Volume, Delta);

            Assert.AreEqual(1.5, orderbook.InMoneyOrders[1].Price, Delta);
            Assert.AreEqual(1, orderbook.InMoneyOrders[1].Investors);
            Assert.AreEqual(375 / 0.5, orderbook.InMoneyOrders[1].Volume, Delta);

            Assert.AreEqual(1.2, orderbook.InMoneyOrders[2].Price, Delta);
            Assert.AreEqual(2, orderbook.InMoneyOrders[2].Investors);
            Assert.AreEqual(850 / 0.5, orderbook.InMoneyOrders[2].Volume, Delta);

            Assert.AreEqual(0.5, orderbook.InMoneyOrders[3].Price, Delta);
            Assert.AreEqual(2, orderbook.InMoneyOrders[3].Investors);
            Assert.AreEqual(1150 / 0.5, orderbook.InMoneyOrders[3].Volume, Delta);

            Assert.AreEqual(BidState.InMoney, _bids[0].State);
            Assert.AreEqual(BidState.InMoney, _bids[1].State);
            Assert.AreEqual(BidState.InMoney, _bids[2].State);
            Assert.AreEqual(BidState.InMoney, _bids[3].State);
            Assert.AreEqual(BidState.InMoney, _bids[4].State);
            Assert.AreEqual(BidState.InMoney, _bids[5].State);
        }

        [TestMethod]
        public void Is_orderbook_correct_when_all_lkk_sold_and_not_all_bids_fit_in()
        {
            // Asset
            _assetExchangeServiceMock
                .Setup(s => s.Exchange(It.IsAny<double>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns<double, string, string>((amount, baseAssetId, targetAssetId) => amount);

            _bids.AddRange(new[]
            {
                new Bid("client1", 1.5, "USD", 250).SetVolumeFluently("CHF", 125),
                new Bid("client2", 2.0, "USD", 125),
                new Bid("client3", 1.2, "USD", 300).SetVolumeFluently("EUR", 150),
                new Bid("client4", 1.2, "USD", 400),
                new Bid("client5", 0.5, "USD", 250).SetVolumeFluently("EUR", 500),
                new Bid("client6", 0.5, "EUR", 800).SetVolumeFluently("USD", 80), // <- Cut off here, 400 in money
                new Bid("client7", 0.5, "EUR", 1000),
                new Bid("client8", 0.4, "USD", 100)
            });

            // Act
            var orderbook = _orderbookService.Render();

            // Assert
            Assert.AreEqual(0.5, orderbook.CurrentPrice, Delta);
            Assert.AreEqual(2500 / 0.5, orderbook.CurrentInMoneyVolume, Delta);
            Assert.AreEqual(1580 / 0.5, orderbook.CurrentOutOfTheMoneyVolume, Delta);

            Assert.AreEqual(4, orderbook.InMoneyOrders.Length);
            
            Assert.AreEqual(2.0, orderbook.InMoneyOrders[0].Price, Delta);
            Assert.AreEqual(1, orderbook.InMoneyOrders[0].Investors);
            Assert.AreEqual(125 / 0.5, orderbook.InMoneyOrders[0].Volume, Delta);

            Assert.AreEqual(1.5, orderbook.InMoneyOrders[1].Price, Delta);
            Assert.AreEqual(1, orderbook.InMoneyOrders[1].Investors);
            Assert.AreEqual(375 / 0.5, orderbook.InMoneyOrders[1].Volume, Delta);

            Assert.AreEqual(1.2, orderbook.InMoneyOrders[2].Price, Delta);
            Assert.AreEqual(2, orderbook.InMoneyOrders[2].Investors);
            Assert.AreEqual(850 / 0.5, orderbook.InMoneyOrders[2].Volume, Delta);

            Assert.AreEqual(0.5, orderbook.InMoneyOrders[3].Price, Delta);
            Assert.AreEqual(2, orderbook.InMoneyOrders[3].Investors);
            Assert.AreEqual(1150 / 0.5, orderbook.InMoneyOrders[3].Volume, Delta);

            Assert.AreEqual(2, orderbook.OutOfMoneyOrders.Length);

            Assert.AreEqual(0.5, orderbook.OutOfMoneyOrders[0].Price, Delta);
            Assert.AreEqual(2, orderbook.OutOfMoneyOrders[0].Investors);
            Assert.AreEqual(1480 / 0.5, orderbook.OutOfMoneyOrders[0].Volume, Delta);

            Assert.AreEqual(0.4, orderbook.OutOfMoneyOrders[1].Price, Delta);
            Assert.AreEqual(1, orderbook.OutOfMoneyOrders[1].Investors);
            Assert.AreEqual(100 / 0.5, orderbook.OutOfMoneyOrders[1].Volume, Delta);

            Assert.AreEqual(BidState.InMoney, _bids[0].State);
            Assert.AreEqual(BidState.InMoney, _bids[1].State);
            Assert.AreEqual(BidState.InMoney, _bids[2].State);
            Assert.AreEqual(BidState.InMoney, _bids[3].State);
            Assert.AreEqual(BidState.InMoney, _bids[4].State);
            Assert.AreEqual(BidState.PartiallyInMoney, _bids[5].State);
            Assert.AreEqual(2, _bids[5].InMoneyAssetVolumes.Count);
            Assert.AreEqual(1, _bids[5].InMoneyAssetVolumes.Count(v => v.Key == "EUR"));
            Assert.AreEqual(1, _bids[5].InMoneyAssetVolumes.Count(v => v.Key == "USD"));
            Assert.AreEqual(400d / 880d * 800d, _bids[5].InMoneyAssetVolumes.Single(v => v.Key == "EUR").Value, Delta);
            Assert.AreEqual(400d / 880d * 80d, _bids[5].InMoneyAssetVolumes.Single(v => v.Key == "USD").Value, Delta);
            Assert.AreEqual(BidState.OutOfTheMoney, _bids[6].State);
            Assert.AreEqual(BidState.OutOfTheMoney, _bids[7].State);
        }

        [TestMethod]
        public void Is_orderbook_correct_when_all_lkk_sold_and_closing_bid_cut_off_to_small()
        {
            // Asset
            _assetExchangeServiceMock
                .Setup(s => s.Exchange(It.IsAny<double>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns<double, string, string>((amount, baseAssetId, targetAssetId) => amount);

            _bids.AddRange(new[]
            {
                new Bid("client1", 1.5, "USD", 250).SetVolumeFluently("CHF", 125),
                new Bid("client2", 2.0, "USD", 125),
                new Bid("client3", 1.2, "USD", 300).SetVolumeFluently("EUR", 150),
                new Bid("client4", 1.2, "USD", 400),
                new Bid("client5", 0.5, "USD", 250).SetVolumeFluently("EUR", 895),
                // Cut off here
                new Bid("client6", 0.5, "EUR", 800).SetVolumeFluently("USD", 700),
                new Bid("client7", 0.5, "EUR", 2000),
                new Bid("client8", 0.4, "USD", 100)
            });

            // Act
            var orderbook = _orderbookService.Render();

            // Assert
            Assert.AreEqual(0.5, orderbook.CurrentPrice, Delta);
            Assert.AreEqual(2495 / 0.5, orderbook.CurrentInMoneyVolume, Delta);
            Assert.AreEqual(3600 / 0.5, orderbook.CurrentOutOfTheMoneyVolume, Delta);

            Assert.AreEqual(4, orderbook.InMoneyOrders.Length);

            Assert.AreEqual(2.0, orderbook.InMoneyOrders[0].Price, Delta);
            Assert.AreEqual(1, orderbook.InMoneyOrders[0].Investors);
            Assert.AreEqual(125 / 0.5, orderbook.InMoneyOrders[0].Volume, Delta);

            Assert.AreEqual(1.5, orderbook.InMoneyOrders[1].Price, Delta);
            Assert.AreEqual(1, orderbook.InMoneyOrders[1].Investors);
            Assert.AreEqual(375 / 0.5, orderbook.InMoneyOrders[1].Volume, Delta);

            Assert.AreEqual(1.2, orderbook.InMoneyOrders[2].Price, Delta);
            Assert.AreEqual(2, orderbook.InMoneyOrders[2].Investors);
            Assert.AreEqual(850 / 0.5, orderbook.InMoneyOrders[2].Volume, Delta);

            Assert.AreEqual(0.5, orderbook.InMoneyOrders[3].Price, Delta);
            Assert.AreEqual(1, orderbook.InMoneyOrders[3].Investors);
            Assert.AreEqual(1145 / 0.5, orderbook.InMoneyOrders[3].Volume, Delta);

            Assert.AreEqual(2, orderbook.OutOfMoneyOrders.Length);

            Assert.AreEqual(0.5, orderbook.OutOfMoneyOrders[0].Price, Delta);
            Assert.AreEqual(2, orderbook.OutOfMoneyOrders[0].Investors);
            Assert.AreEqual(3500 / 0.5, orderbook.OutOfMoneyOrders[0].Volume, Delta);

            Assert.AreEqual(0.4, orderbook.OutOfMoneyOrders[1].Price, Delta);
            Assert.AreEqual(1, orderbook.OutOfMoneyOrders[1].Investors);
            Assert.AreEqual(100 / 0.5, orderbook.OutOfMoneyOrders[1].Volume, Delta);

            Assert.AreEqual(BidState.InMoney, _bids[0].State);
            Assert.AreEqual(BidState.InMoney, _bids[1].State);
            Assert.AreEqual(BidState.InMoney, _bids[2].State);
            Assert.AreEqual(BidState.InMoney, _bids[3].State);
            Assert.AreEqual(BidState.InMoney, _bids[4].State);
            Assert.AreEqual(BidState.OutOfTheMoney, _bids[5].State);
            Assert.AreEqual(BidState.OutOfTheMoney, _bids[6].State);
            Assert.AreEqual(BidState.OutOfTheMoney, _bids[7].State);
        }

        [TestMethod]
        public void Is_orderbook_correct_when_all_lkk_sold_and_closing_bid_to_small()
        {
            // Asset
            _assetExchangeServiceMock
                .Setup(s => s.Exchange(It.IsAny<double>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns<double, string, string>((amount, baseAssetId, targetAssetId) => amount);

            _bids.AddRange(new[]
            {
                new Bid("client1", 1.5, "USD", 250).SetVolumeFluently("CHF", 125),
                new Bid("client2", 2.0, "USD", 125),
                new Bid("client3", 1.2, "USD", 300).SetVolumeFluently("EUR", 150),
                new Bid("client4", 1.2, "USD", 660).SetVolumeFluently("EUR", 880),
                new Bid("client5", 0.5, "USD", 5),
                new Bid("client6", 0.5, "EUR", 5),
                // Cut off here
                new Bid("client7", 0.5, "EUR", 2000),
                new Bid("client8", 0.4, "USD", 100)
            });

            // Act
            var orderbook = _orderbookService.Render();

            // Assert
            Assert.AreEqual(0.5, orderbook.CurrentPrice, Delta);
            Assert.AreEqual(2500 / 0.5, orderbook.CurrentInMoneyVolume, Delta);
            Assert.AreEqual(2100 / 0.5, orderbook.CurrentOutOfTheMoneyVolume, Delta);

            Assert.AreEqual(4, orderbook.InMoneyOrders.Length);

            Assert.AreEqual(2.0, orderbook.InMoneyOrders[0].Price, Delta);
            Assert.AreEqual(1, orderbook.InMoneyOrders[0].Investors);
            Assert.AreEqual(125 / 0.5, orderbook.InMoneyOrders[0].Volume, Delta);

            Assert.AreEqual(1.5, orderbook.InMoneyOrders[1].Price, Delta);
            Assert.AreEqual(1, orderbook.InMoneyOrders[1].Investors);
            Assert.AreEqual(375 / 0.5, orderbook.InMoneyOrders[1].Volume, Delta);

            Assert.AreEqual(1.2, orderbook.InMoneyOrders[2].Price, Delta);
            Assert.AreEqual(2, orderbook.InMoneyOrders[2].Investors);
            Assert.AreEqual(1990 / 0.5, orderbook.InMoneyOrders[2].Volume, Delta);

            Assert.AreEqual(0.5, orderbook.InMoneyOrders[3].Price, Delta);
            Assert.AreEqual(2, orderbook.InMoneyOrders[3].Investors);
            Assert.AreEqual(10 / 0.5, orderbook.InMoneyOrders[3].Volume, Delta);

            Assert.AreEqual(2, orderbook.OutOfMoneyOrders.Length);

            Assert.AreEqual(0.5, orderbook.OutOfMoneyOrders[0].Price, Delta);
            Assert.AreEqual(1, orderbook.OutOfMoneyOrders[0].Investors);
            Assert.AreEqual(2000 / 0.5, orderbook.OutOfMoneyOrders[0].Volume, Delta);

            Assert.AreEqual(0.4, orderbook.OutOfMoneyOrders[1].Price, Delta);
            Assert.AreEqual(1, orderbook.OutOfMoneyOrders[1].Investors);
            Assert.AreEqual(100 / 0.5, orderbook.OutOfMoneyOrders[1].Volume, Delta);

            Assert.AreEqual(BidState.InMoney, _bids[0].State);
            Assert.AreEqual(BidState.InMoney, _bids[1].State);
            Assert.AreEqual(BidState.InMoney, _bids[2].State);
            Assert.AreEqual(BidState.InMoney, _bids[3].State);
            Assert.AreEqual(BidState.InMoney, _bids[4].State);
            Assert.AreEqual(BidState.InMoney, _bids[5].State);
            Assert.AreEqual(BidState.OutOfTheMoney, _bids[6].State);
            Assert.AreEqual(BidState.OutOfTheMoney, _bids[7].State);
        }

        [TestMethod]
        public void Is_orderbook_correct_when_all_lkk_sold_to_high_price_orders_with_lower_lkk_price()
        {
            // Asset
            _assetExchangeServiceMock
                .Setup(s => s.Exchange(It.IsAny<double>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns<double, string, string>((amount, baseAssetId, targetAssetId) => amount);

            _bids.AddRange(new[]
            {
                new Bid("client1", 2.2, "USD", 1500),
                new Bid("client2", 2.0, "USD", 1000).SetVolumeFluently("CHF", 500),
                new Bid("client3", 2.0, "USD", 800).SetVolumeFluently("EUR", 600),
                new Bid("client4", 1.0, "USD", 100),
                // Cut off here with price 0.9 (= 4500 / 5000)
                new Bid("client5", 0.5, "USD", 500),
                new Bid("client6", 0.5, "EUR", 250),
                new Bid("client7", 0.4, "USD", 100)
            });

            // Act
            var orderbook = _orderbookService.Render();

            // Assert
            Assert.AreEqual(0.9, orderbook.CurrentPrice, Delta);
            Assert.AreEqual(4500 / 0.9, orderbook.CurrentInMoneyVolume, Delta);
            Assert.AreEqual(850 / 0.9, orderbook.CurrentOutOfTheMoneyVolume, Delta);

            Assert.AreEqual(3, orderbook.InMoneyOrders.Length);

            Assert.AreEqual(2.2, orderbook.InMoneyOrders[0].Price, Delta);
            Assert.AreEqual(1, orderbook.InMoneyOrders[0].Investors);
            Assert.AreEqual(1500 / 0.9, orderbook.InMoneyOrders[0].Volume, Delta);

            Assert.AreEqual(2.0, orderbook.InMoneyOrders[1].Price, Delta);
            Assert.AreEqual(2, orderbook.InMoneyOrders[1].Investors);
            Assert.AreEqual(2900 / 0.9, orderbook.InMoneyOrders[1].Volume, Delta);

            Assert.AreEqual(1.0, orderbook.InMoneyOrders[2].Price, Delta);
            Assert.AreEqual(1, orderbook.InMoneyOrders[2].Investors);
            Assert.AreEqual(100 / 0.9, orderbook.InMoneyOrders[2].Volume, Delta);

            Assert.AreEqual(2, orderbook.OutOfMoneyOrders.Length);

            Assert.AreEqual(0.5, orderbook.OutOfMoneyOrders[0].Price, Delta);
            Assert.AreEqual(2, orderbook.OutOfMoneyOrders[0].Investors);
            Assert.AreEqual(750 / 0.9, orderbook.OutOfMoneyOrders[0].Volume, Delta);

            Assert.AreEqual(0.4, orderbook.OutOfMoneyOrders[1].Price, Delta);
            Assert.AreEqual(1, orderbook.OutOfMoneyOrders[1].Investors);
            Assert.AreEqual(100 / 0.9, orderbook.OutOfMoneyOrders[1].Volume, Delta);

            Assert.AreEqual(BidState.InMoney, _bids[0].State);
            Assert.AreEqual(BidState.InMoney, _bids[1].State);
            Assert.AreEqual(BidState.InMoney, _bids[2].State);
            Assert.AreEqual(BidState.InMoney, _bids[3].State);
            Assert.AreEqual(BidState.OutOfTheMoney, _bids[4].State);
            Assert.AreEqual(BidState.OutOfTheMoney, _bids[5].State);
            Assert.AreEqual(BidState.OutOfTheMoney, _bids[6].State);
        }
    }
}