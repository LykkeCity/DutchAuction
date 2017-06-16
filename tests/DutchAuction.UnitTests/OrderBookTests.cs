using System.Collections.Immutable;
using System.Linq;
using DutchAuction.Core.Domain.Auction;
using DutchAuction.Core.Services.Assets;
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

        [TestInitialize]
        public void InitializeTest()
        {
            _assetExchangeServiceMock = new Mock<IAssetExchangeService>();
            
            _assetExchangeServiceMock
                .Setup(s => s.Exchange(It.IsAny<double>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns<double, string, string>((amount, baseAssetId, targetAssetId) => amount);
           
            _orderbookService = new OrderbookService(_assetExchangeServiceMock.Object, 
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

            var clientBids = new IClientBid[]
            {
                new ClientBid("client1", 100, "USD", 10),
                new ClientBid("client2", 100, "USD", 20)
            };

            // Act
            var orderbook = _orderbookService.Render(clientBids.ToImmutableArray());

            // Assert
            Assert.AreEqual(1, orderbook.InMoneyOrders.Count);
            Assert.AreEqual(100d, orderbook.InMoneyOrders[0].Price, Delta);
        }

        [TestMethod]
        public void Is_orderbook_correct_when_not_all_lkk_sold()
        {
            // Asset
            _assetExchangeServiceMock
                .Setup(s => s.Exchange(It.IsAny<double>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns<double, string, string>((amount, baseAssetId, targetAssetId) => amount);

            var clientBids = new IClientBid[]
            {
                new ClientBid("client1", 1.5, "USD", 100).SetVolume("CHF", 250),
                new ClientBid("client2", 2.0, "USD", 200),
                new ClientBid("client3", 1.2, "USD", 300).SetVolume("EUR", 150),
                new ClientBid("client4", 1.2, "USD", 300),
            };
            
            // Act
            var orderbook = _orderbookService.Render(clientBids.ToImmutableArray());

            // Assert
            Assert.AreEqual(1.2, orderbook.LkkPriceChf, Delta);
            Assert.AreEqual(1300 / 1.2, orderbook.InMoneyVolumeLkk, Delta);
            Assert.AreEqual(0.0, orderbook.OutOfTheMoneyVolumeLkk, Delta);

            Assert.AreEqual(3, orderbook.InMoneyOrders.Count);

            Assert.AreEqual(2.0, orderbook.InMoneyOrders[0].Price, Delta);
            Assert.AreEqual(1, orderbook.InMoneyOrders[0].Investors);
            Assert.AreEqual(200 / 1.2, orderbook.InMoneyOrders[0].Volume, Delta);

            Assert.AreEqual(1.5, orderbook.InMoneyOrders[1].Price, Delta);
            Assert.AreEqual(1, orderbook.InMoneyOrders[1].Investors);
            Assert.AreEqual(350 / 1.2, orderbook.InMoneyOrders[1].Volume, Delta);

            Assert.AreEqual(OrderbookBidState.InMoney, orderbook.TryGetBid("client1").State);
            Assert.AreEqual(OrderbookBidState.InMoney, orderbook.TryGetBid("client2").State);
            Assert.AreEqual(OrderbookBidState.InMoney, orderbook.TryGetBid("client3").State);
            Assert.AreEqual(OrderbookBidState.InMoney, orderbook.TryGetBid("client4").State);

            Assert.AreEqual(1.2, orderbook.TryGetBid("client1").LkkPriceChf, Delta);
            Assert.AreEqual(1.2, orderbook.TryGetBid("client2").LkkPriceChf, Delta);
            Assert.AreEqual(1.2, orderbook.TryGetBid("client3").LkkPriceChf, Delta);
            Assert.AreEqual(1.2, orderbook.TryGetBid("client4").LkkPriceChf, Delta);

            Assert.AreEqual(2, orderbook.TryGetBid("client1").AssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client2").AssetVolumesLkk.Count);
            Assert.AreEqual(2, orderbook.TryGetBid("client3").AssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client4").AssetVolumesLkk.Count);

            Assert.AreEqual(100 / 1.2, orderbook.TryGetBid("client1").AssetVolumesLkk.Single(a => a.Key == "USD").Value, Delta);
            Assert.AreEqual(250 / 1.2, orderbook.TryGetBid("client1").AssetVolumesLkk.Single(a => a.Key == "CHF").Value, Delta);
            Assert.AreEqual(200 / 1.2, orderbook.TryGetBid("client2").AssetVolumesLkk.Single().Value);
            Assert.AreEqual(300 / 1.2, orderbook.TryGetBid("client3").AssetVolumesLkk.Single(a => a.Key == "USD").Value, Delta);
            Assert.AreEqual(150 / 1.2, orderbook.TryGetBid("client3").AssetVolumesLkk.Single(a => a.Key == "EUR").Value, Delta);
            Assert.AreEqual(300 / 1.2, orderbook.TryGetBid("client4").AssetVolumesLkk.Single().Value);

            Assert.AreEqual(2, orderbook.TryGetBid("client1").InMoneyAssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client2").InMoneyAssetVolumesLkk.Count);
            Assert.AreEqual(2, orderbook.TryGetBid("client3").InMoneyAssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client4").InMoneyAssetVolumesLkk.Count);

            Assert.AreEqual(100 / 1.2, orderbook.TryGetBid("client1").InMoneyAssetVolumesLkk.Single(a => a.Key == "USD").Value, Delta);
            Assert.AreEqual(250 / 1.2, orderbook.TryGetBid("client1").InMoneyAssetVolumesLkk.Single(a => a.Key == "CHF").Value, Delta);
            Assert.AreEqual(200 / 1.2, orderbook.TryGetBid("client2").InMoneyAssetVolumesLkk.Single().Value);
            Assert.AreEqual(300 / 1.2, orderbook.TryGetBid("client3").InMoneyAssetVolumesLkk.Single(a => a.Key == "USD").Value, Delta);
            Assert.AreEqual(150 / 1.2, orderbook.TryGetBid("client3").InMoneyAssetVolumesLkk.Single(a => a.Key == "EUR").Value, Delta);
            Assert.AreEqual(300 / 1.2, orderbook.TryGetBid("client4").InMoneyAssetVolumesLkk.Single().Value);
        }

        [TestMethod]
        public void Is_orderbook_correct_when_all_lkk_sold_and_entire_closing_bid_fit_in()
        {
            // Asset
            _assetExchangeServiceMock
                .Setup(s => s.Exchange(It.IsAny<double>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns<double, string, string>((amount, baseAssetId, targetAssetId) => amount);

            var clientBids = new IClientBid[]
            {
                new ClientBid("client1", 1.5, "USD", 250).SetVolume("CHF", 125),
                new ClientBid("client2", 2.0, "USD", 125),
                new ClientBid("client3", 1.2, "USD", 300).SetVolume("EUR", 150),
                new ClientBid("client4", 1.2, "USD", 400),
                new ClientBid("client5", 0.5, "USD", 250).SetVolume("EUR", 500),
                new ClientBid("client6", 0.5, "EUR", 400)
            };

            // Act
            var orderbook = _orderbookService.Render(clientBids.ToImmutableArray());

            // Assert
            Assert.AreEqual(0.5, orderbook.LkkPriceChf, Delta);
            Assert.AreEqual(2500 / 0.5, orderbook.InMoneyVolumeLkk, Delta);
            Assert.AreEqual(0.0, orderbook.OutOfTheMoneyVolumeLkk, Delta);

            Assert.AreEqual(4, orderbook.InMoneyOrders.Count);

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

            Assert.AreEqual(OrderbookBidState.InMoney, orderbook.TryGetBid("client1").State);
            Assert.AreEqual(OrderbookBidState.InMoney, orderbook.TryGetBid("client2").State);
            Assert.AreEqual(OrderbookBidState.InMoney, orderbook.TryGetBid("client3").State);
            Assert.AreEqual(OrderbookBidState.InMoney, orderbook.TryGetBid("client4").State);
            Assert.AreEqual(OrderbookBidState.InMoney, orderbook.TryGetBid("client5").State);
            Assert.AreEqual(OrderbookBidState.InMoney, orderbook.TryGetBid("client6").State);

            Assert.AreEqual(0.5, orderbook.TryGetBid("client1").LkkPriceChf, Delta);
            Assert.AreEqual(0.5, orderbook.TryGetBid("client2").LkkPriceChf, Delta);
            Assert.AreEqual(0.5, orderbook.TryGetBid("client3").LkkPriceChf, Delta);
            Assert.AreEqual(0.5, orderbook.TryGetBid("client4").LkkPriceChf, Delta);
            Assert.AreEqual(0.5, orderbook.TryGetBid("client5").LkkPriceChf, Delta);
            Assert.AreEqual(0.5, orderbook.TryGetBid("client6").LkkPriceChf, Delta);

            Assert.AreEqual(2, orderbook.TryGetBid("client1").AssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client2").AssetVolumesLkk.Count);
            Assert.AreEqual(2, orderbook.TryGetBid("client3").AssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client4").AssetVolumesLkk.Count);
            Assert.AreEqual(2, orderbook.TryGetBid("client5").AssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client6").AssetVolumesLkk.Count);

            Assert.AreEqual(250 / 0.5, orderbook.TryGetBid("client1").AssetVolumesLkk.Single(a => a.Key == "USD").Value, Delta);
            Assert.AreEqual(125 / 0.5, orderbook.TryGetBid("client1").AssetVolumesLkk.Single(a => a.Key == "CHF").Value, Delta);
            Assert.AreEqual(125 / 0.5, orderbook.TryGetBid("client2").AssetVolumesLkk.Single().Value);
            Assert.AreEqual(300 / 0.5, orderbook.TryGetBid("client3").AssetVolumesLkk.Single(a => a.Key == "USD").Value, Delta);
            Assert.AreEqual(150 / 0.5, orderbook.TryGetBid("client3").AssetVolumesLkk.Single(a => a.Key == "EUR").Value, Delta);
            Assert.AreEqual(400 / 0.5, orderbook.TryGetBid("client4").AssetVolumesLkk.Single().Value);
            Assert.AreEqual(250 / 0.5, orderbook.TryGetBid("client5").AssetVolumesLkk.Single(a => a.Key == "USD").Value, Delta);
            Assert.AreEqual(500 / 0.5, orderbook.TryGetBid("client5").AssetVolumesLkk.Single(a => a.Key == "EUR").Value, Delta);
            Assert.AreEqual(400 / 0.5, orderbook.TryGetBid("client6").AssetVolumesLkk.Single().Value);

            Assert.AreEqual(2, orderbook.TryGetBid("client1").InMoneyAssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client2").InMoneyAssetVolumesLkk.Count);
            Assert.AreEqual(2, orderbook.TryGetBid("client3").InMoneyAssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client4").InMoneyAssetVolumesLkk.Count);
            Assert.AreEqual(2, orderbook.TryGetBid("client5").InMoneyAssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client6").InMoneyAssetVolumesLkk.Count);

            Assert.AreEqual(250 / 0.5, orderbook.TryGetBid("client1").InMoneyAssetVolumesLkk.Single(a => a.Key == "USD").Value, Delta);
            Assert.AreEqual(125 / 0.5, orderbook.TryGetBid("client1").InMoneyAssetVolumesLkk.Single(a => a.Key == "CHF").Value, Delta);
            Assert.AreEqual(125 / 0.5, orderbook.TryGetBid("client2").InMoneyAssetVolumesLkk.Single().Value);
            Assert.AreEqual(300 / 0.5, orderbook.TryGetBid("client3").InMoneyAssetVolumesLkk.Single(a => a.Key == "USD").Value, Delta);
            Assert.AreEqual(150 / 0.5, orderbook.TryGetBid("client3").InMoneyAssetVolumesLkk.Single(a => a.Key == "EUR").Value, Delta);
            Assert.AreEqual(400 / 0.5, orderbook.TryGetBid("client4").InMoneyAssetVolumesLkk.Single().Value);
            Assert.AreEqual(250 / 0.5, orderbook.TryGetBid("client5").InMoneyAssetVolumesLkk.Single(a => a.Key == "USD").Value, Delta);
            Assert.AreEqual(500 / 0.5, orderbook.TryGetBid("client5").InMoneyAssetVolumesLkk.Single(a => a.Key == "EUR").Value, Delta);
            Assert.AreEqual(400 / 0.5, orderbook.TryGetBid("client6").InMoneyAssetVolumesLkk.Single().Value);
        }

        [TestMethod]
        public void Is_orderbook_correct_when_all_lkk_sold_and_not_all_bids_fit_in()
        {
            // Asset
            var clientBids = new IClientBid[]
            {
                new ClientBid("client1", 1.5, "USD", 250).SetVolume("CHF", 125),
                new ClientBid("client2", 2.0, "USD", 125),
                new ClientBid("client3", 1.2, "USD", 300).SetVolume("EUR", 150),
                new ClientBid("client4", 1.2, "USD", 400),
                new ClientBid("client5", 0.5, "USD", 250).SetVolume("EUR", 500),
                new ClientBid("client6", 0.5, "EUR", 800).SetVolume("USD", 80), // <- Cut off here, 400 in money
                new ClientBid("client7", 0.5, "EUR", 1000),
                new ClientBid("client8", 0.4, "USD", 100)
            };

            // Act
            var orderbook = _orderbookService.Render(clientBids.ToImmutableArray());

            // Assert
            Assert.AreEqual(0.5, orderbook.LkkPriceChf, Delta);
            Assert.AreEqual(2500 / 0.5, orderbook.InMoneyVolumeLkk, Delta);
            Assert.AreEqual(1580 / 0.5, orderbook.OutOfTheMoneyVolumeLkk, Delta);

            Assert.AreEqual(4, orderbook.InMoneyOrders.Count);
            
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

            Assert.AreEqual(2, orderbook.OutOfMoneyOrders.Count);

            Assert.AreEqual(0.5, orderbook.OutOfMoneyOrders[0].Price, Delta);
            Assert.AreEqual(2, orderbook.OutOfMoneyOrders[0].Investors);
            Assert.AreEqual(1480 / 0.5, orderbook.OutOfMoneyOrders[0].Volume, Delta);

            Assert.AreEqual(0.4, orderbook.OutOfMoneyOrders[1].Price, Delta);
            Assert.AreEqual(1, orderbook.OutOfMoneyOrders[1].Investors);
            Assert.AreEqual(100 / 0.5, orderbook.OutOfMoneyOrders[1].Volume, Delta);

            Assert.AreEqual(OrderbookBidState.InMoney, orderbook.TryGetBid("client1").State);
            Assert.AreEqual(OrderbookBidState.InMoney, orderbook.TryGetBid("client2").State);
            Assert.AreEqual(OrderbookBidState.InMoney, orderbook.TryGetBid("client3").State);
            Assert.AreEqual(OrderbookBidState.InMoney, orderbook.TryGetBid("client4").State);
            Assert.AreEqual(OrderbookBidState.InMoney, orderbook.TryGetBid("client5").State);
            Assert.AreEqual(OrderbookBidState.PartiallyInMoney, orderbook.TryGetBid("client6").State);
            Assert.AreEqual(OrderbookBidState.OutOfTheMoney, orderbook.TryGetBid("client7").State);
            Assert.AreEqual(OrderbookBidState.OutOfTheMoney, orderbook.TryGetBid("client8").State);

            Assert.AreEqual(0.5, orderbook.TryGetBid("client1").LkkPriceChf, Delta);
            Assert.AreEqual(0.5, orderbook.TryGetBid("client2").LkkPriceChf, Delta);
            Assert.AreEqual(0.5, orderbook.TryGetBid("client3").LkkPriceChf, Delta);
            Assert.AreEqual(0.5, orderbook.TryGetBid("client4").LkkPriceChf, Delta);
            Assert.AreEqual(0.5, orderbook.TryGetBid("client5").LkkPriceChf, Delta);
            Assert.AreEqual(0.5, orderbook.TryGetBid("client6").LkkPriceChf, Delta);
            Assert.AreEqual(0.5, orderbook.TryGetBid("client7").LkkPriceChf, Delta);
            Assert.AreEqual(0.5, orderbook.TryGetBid("client8").LkkPriceChf, Delta);
            
            Assert.AreEqual(2, orderbook.TryGetBid("client1").InMoneyAssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client2").InMoneyAssetVolumesLkk.Count);
            Assert.AreEqual(2, orderbook.TryGetBid("client3").InMoneyAssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client4").InMoneyAssetVolumesLkk.Count);
            Assert.AreEqual(2, orderbook.TryGetBid("client5").InMoneyAssetVolumesLkk.Count);
            Assert.AreEqual(2, orderbook.TryGetBid("client6").InMoneyAssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client7").InMoneyAssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client8").InMoneyAssetVolumesLkk.Count);

            Assert.AreEqual(250 / 0.5, orderbook.TryGetBid("client1").InMoneyAssetVolumesLkk.Single(v => v.Key == "USD").Value, Delta);
            Assert.AreEqual(125 / 0.5, orderbook.TryGetBid("client1").InMoneyAssetVolumesLkk.Single(v => v.Key == "CHF").Value, Delta);
            Assert.AreEqual(125 / 0.5, orderbook.TryGetBid("client2").InMoneyAssetVolumesLkk.Single().Value, Delta);
            Assert.AreEqual(300 / 0.5, orderbook.TryGetBid("client3").InMoneyAssetVolumesLkk.Single(v => v.Key == "USD").Value, Delta);
            Assert.AreEqual(150 / 0.5, orderbook.TryGetBid("client3").InMoneyAssetVolumesLkk.Single(v => v.Key == "EUR").Value, Delta);
            Assert.AreEqual(400 / 0.5, orderbook.TryGetBid("client4").InMoneyAssetVolumesLkk.Single().Value, Delta);
            Assert.AreEqual(250 / 0.5, orderbook.TryGetBid("client5").InMoneyAssetVolumesLkk.Single(v => v.Key == "USD").Value, Delta);
            Assert.AreEqual(500 / 0.5, orderbook.TryGetBid("client5").InMoneyAssetVolumesLkk.Single(v => v.Key == "EUR").Value, Delta);
            Assert.AreEqual(400d / 880d * 800d / 0.5, orderbook.TryGetBid("client6").InMoneyAssetVolumesLkk.Single(v => v.Key == "EUR").Value, Delta);
            Assert.AreEqual(400d / 880d * 80d / 0.5, orderbook.TryGetBid("client6").InMoneyAssetVolumesLkk.Single(v => v.Key == "USD").Value, Delta);
            Assert.AreEqual(0, orderbook.TryGetBid("client7").InMoneyAssetVolumesLkk.Single().Value, Delta);
            Assert.AreEqual(0, orderbook.TryGetBid("client8").InMoneyAssetVolumesLkk.Single().Value, Delta);

            Assert.AreEqual(2, orderbook.TryGetBid("client1").AssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client2").AssetVolumesLkk.Count);
            Assert.AreEqual(2, orderbook.TryGetBid("client3").AssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client4").AssetVolumesLkk.Count);
            Assert.AreEqual(2, orderbook.TryGetBid("client5").AssetVolumesLkk.Count);
            Assert.AreEqual(2, orderbook.TryGetBid("client6").AssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client7").AssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client8").AssetVolumesLkk.Count);

            Assert.AreEqual(250 / 0.5, orderbook.TryGetBid("client1").AssetVolumesLkk.Single(v => v.Key == "USD").Value, Delta);
            Assert.AreEqual(125 / 0.5, orderbook.TryGetBid("client1").AssetVolumesLkk.Single(v => v.Key == "CHF").Value, Delta);
            Assert.AreEqual(125 / 0.5, orderbook.TryGetBid("client2").AssetVolumesLkk.Single().Value, Delta);
            Assert.AreEqual(300 / 0.5, orderbook.TryGetBid("client3").AssetVolumesLkk.Single(v => v.Key == "USD").Value, Delta);
            Assert.AreEqual(150 / 0.5, orderbook.TryGetBid("client3").AssetVolumesLkk.Single(v => v.Key == "EUR").Value, Delta);
            Assert.AreEqual(400 / 0.5, orderbook.TryGetBid("client4").AssetVolumesLkk.Single().Value, Delta);
            Assert.AreEqual(250 / 0.5, orderbook.TryGetBid("client5").AssetVolumesLkk.Single(v => v.Key == "USD").Value, Delta);
            Assert.AreEqual(500 / 0.5, orderbook.TryGetBid("client5").AssetVolumesLkk.Single(v => v.Key == "EUR").Value, Delta);
            Assert.AreEqual(800 / 0.5, orderbook.TryGetBid("client6").AssetVolumesLkk.Single(v => v.Key == "EUR").Value, Delta);
            Assert.AreEqual(80 / 0.5, orderbook.TryGetBid("client6").AssetVolumesLkk.Single(v => v.Key == "USD").Value, Delta);
            Assert.AreEqual(1000 / 0.5, orderbook.TryGetBid("client7").AssetVolumesLkk.Single().Value, Delta);
            Assert.AreEqual(100 / 0.5, orderbook.TryGetBid("client8").AssetVolumesLkk.Single().Value, Delta);
        }

        [TestMethod]
        public void Is_orderbook_correct_when_all_lkk_sold_and_closing_bid_cut_off_to_small()
        {
            // Asset
            var clientBids = new IClientBid[]
            {
                new ClientBid("client1", 1.5, "USD", 250).SetVolume("CHF", 125),
                new ClientBid("client2", 2.0, "USD", 125),
                new ClientBid("client3", 1.2, "USD", 300).SetVolume("EUR", 150),
                new ClientBid("client4", 1.2, "USD", 400),
                new ClientBid("client5", 0.5, "USD", 250).SetVolume("EUR", 895),
                // Cut off here
                new ClientBid("client6", 0.5, "EUR", 800).SetVolume("USD", 700),
                new ClientBid("client7", 0.5, "EUR", 2000),
                new ClientBid("client8", 0.4, "USD", 100)
            };

            // Act
            var orderbook = _orderbookService.Render(clientBids.ToImmutableArray());

            // Assert
            Assert.AreEqual(0.5, orderbook.LkkPriceChf, Delta);
            Assert.AreEqual(2495 / 0.5, orderbook.InMoneyVolumeLkk, Delta);
            Assert.AreEqual(3600 / 0.5, orderbook.OutOfTheMoneyVolumeLkk, Delta);

            Assert.AreEqual(4, orderbook.InMoneyOrders.Count);

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

            Assert.AreEqual(2, orderbook.OutOfMoneyOrders.Count);

            Assert.AreEqual(0.5, orderbook.OutOfMoneyOrders[0].Price, Delta);
            Assert.AreEqual(2, orderbook.OutOfMoneyOrders[0].Investors);
            Assert.AreEqual(3500 / 0.5, orderbook.OutOfMoneyOrders[0].Volume, Delta);

            Assert.AreEqual(0.4, orderbook.OutOfMoneyOrders[1].Price, Delta);
            Assert.AreEqual(1, orderbook.OutOfMoneyOrders[1].Investors);
            Assert.AreEqual(100 / 0.5, orderbook.OutOfMoneyOrders[1].Volume, Delta);

            Assert.AreEqual(OrderbookBidState.InMoney, orderbook.TryGetBid("client1").State);
            Assert.AreEqual(OrderbookBidState.InMoney, orderbook.TryGetBid("client2").State);
            Assert.AreEqual(OrderbookBidState.InMoney, orderbook.TryGetBid("client3").State);
            Assert.AreEqual(OrderbookBidState.InMoney, orderbook.TryGetBid("client4").State);
            Assert.AreEqual(OrderbookBidState.InMoney, orderbook.TryGetBid("client5").State);
            Assert.AreEqual(OrderbookBidState.OutOfTheMoney, orderbook.TryGetBid("client6").State);
            Assert.AreEqual(OrderbookBidState.OutOfTheMoney, orderbook.TryGetBid("client7").State);
            Assert.AreEqual(OrderbookBidState.OutOfTheMoney, orderbook.TryGetBid("client8").State);

            Assert.AreEqual(0.5, orderbook.TryGetBid("client1").LkkPriceChf, Delta);
            Assert.AreEqual(0.5, orderbook.TryGetBid("client2").LkkPriceChf, Delta);
            Assert.AreEqual(0.5, orderbook.TryGetBid("client3").LkkPriceChf, Delta);
            Assert.AreEqual(0.5, orderbook.TryGetBid("client4").LkkPriceChf, Delta);
            Assert.AreEqual(0.5, orderbook.TryGetBid("client5").LkkPriceChf, Delta);
            Assert.AreEqual(0.5, orderbook.TryGetBid("client6").LkkPriceChf, Delta);
            Assert.AreEqual(0.5, orderbook.TryGetBid("client7").LkkPriceChf, Delta);
            Assert.AreEqual(0.5, orderbook.TryGetBid("client8").LkkPriceChf, Delta);

            Assert.AreEqual(2, orderbook.TryGetBid("client1").InMoneyAssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client2").InMoneyAssetVolumesLkk.Count);
            Assert.AreEqual(2, orderbook.TryGetBid("client3").InMoneyAssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client4").InMoneyAssetVolumesLkk.Count);
            Assert.AreEqual(2, orderbook.TryGetBid("client5").InMoneyAssetVolumesLkk.Count);
            Assert.AreEqual(2, orderbook.TryGetBid("client6").InMoneyAssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client7").InMoneyAssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client8").InMoneyAssetVolumesLkk.Count);

            Assert.AreEqual(250 / 0.5, orderbook.TryGetBid("client1").InMoneyAssetVolumesLkk.Single(v => v.Key == "USD").Value, Delta);
            Assert.AreEqual(125 / 0.5, orderbook.TryGetBid("client1").InMoneyAssetVolumesLkk.Single(v => v.Key == "CHF").Value, Delta);
            Assert.AreEqual(125 / 0.5, orderbook.TryGetBid("client2").InMoneyAssetVolumesLkk.Single().Value, Delta);
            Assert.AreEqual(300 / 0.5, orderbook.TryGetBid("client3").InMoneyAssetVolumesLkk.Single(v => v.Key == "USD").Value, Delta);
            Assert.AreEqual(150 / 0.5, orderbook.TryGetBid("client3").InMoneyAssetVolumesLkk.Single(v => v.Key == "EUR").Value, Delta);
            Assert.AreEqual(400 / 0.5, orderbook.TryGetBid("client4").InMoneyAssetVolumesLkk.Single().Value, Delta);
            Assert.AreEqual(250 / 0.5, orderbook.TryGetBid("client5").InMoneyAssetVolumesLkk.Single(v => v.Key == "USD").Value, Delta);
            Assert.AreEqual(895 / 0.5, orderbook.TryGetBid("client5").InMoneyAssetVolumesLkk.Single(v => v.Key == "EUR").Value, Delta);
            Assert.AreEqual(0 / 0.5, orderbook.TryGetBid("client6").InMoneyAssetVolumesLkk.Single(v => v.Key == "EUR").Value, Delta);
            Assert.AreEqual(0 / 0.5, orderbook.TryGetBid("client6").InMoneyAssetVolumesLkk.Single(v => v.Key == "USD").Value, Delta);
            Assert.AreEqual(0, orderbook.TryGetBid("client7").InMoneyAssetVolumesLkk.Single().Value, Delta);
            Assert.AreEqual(0, orderbook.TryGetBid("client8").InMoneyAssetVolumesLkk.Single().Value, Delta);

            Assert.AreEqual(2, orderbook.TryGetBid("client1").AssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client2").AssetVolumesLkk.Count);
            Assert.AreEqual(2, orderbook.TryGetBid("client3").AssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client4").AssetVolumesLkk.Count);
            Assert.AreEqual(2, orderbook.TryGetBid("client5").AssetVolumesLkk.Count);
            Assert.AreEqual(2, orderbook.TryGetBid("client6").AssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client7").AssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client8").AssetVolumesLkk.Count);

            Assert.AreEqual(250 / 0.5, orderbook.TryGetBid("client1").AssetVolumesLkk.Single(v => v.Key == "USD").Value, Delta);
            Assert.AreEqual(125 / 0.5, orderbook.TryGetBid("client1").AssetVolumesLkk.Single(v => v.Key == "CHF").Value, Delta);
            Assert.AreEqual(125 / 0.5, orderbook.TryGetBid("client2").AssetVolumesLkk.Single().Value, Delta);
            Assert.AreEqual(300 / 0.5, orderbook.TryGetBid("client3").AssetVolumesLkk.Single(v => v.Key == "USD").Value, Delta);
            Assert.AreEqual(150 / 0.5, orderbook.TryGetBid("client3").AssetVolumesLkk.Single(v => v.Key == "EUR").Value, Delta);
            Assert.AreEqual(400 / 0.5, orderbook.TryGetBid("client4").AssetVolumesLkk.Single().Value, Delta);
            Assert.AreEqual(250 / 0.5, orderbook.TryGetBid("client5").AssetVolumesLkk.Single(v => v.Key == "USD").Value, Delta);
            Assert.AreEqual(895 / 0.5, orderbook.TryGetBid("client5").AssetVolumesLkk.Single(v => v.Key == "EUR").Value, Delta);
            Assert.AreEqual(800 / 0.5, orderbook.TryGetBid("client6").AssetVolumesLkk.Single(v => v.Key == "EUR").Value, Delta);
            Assert.AreEqual(700 / 0.5, orderbook.TryGetBid("client6").AssetVolumesLkk.Single(v => v.Key == "USD").Value, Delta);
            Assert.AreEqual(2000 / 0.5, orderbook.TryGetBid("client7").AssetVolumesLkk.Single().Value, Delta);
            Assert.AreEqual(100 / 0.5, orderbook.TryGetBid("client8").AssetVolumesLkk.Single().Value, Delta);
        }

        [TestMethod]
        public void Is_orderbook_correct_when_all_lkk_sold_and_closing_bid_to_small()
        {
            // Asset
            var clientBids = new IClientBid[]
            {
                new ClientBid("client1", 1.5, "USD", 250).SetVolume("CHF", 125),
                new ClientBid("client2", 2.0, "USD", 125),
                new ClientBid("client3", 1.2, "USD", 300).SetVolume("EUR", 150),
                new ClientBid("client4", 1.2, "USD", 660).SetVolume("EUR", 880),
                new ClientBid("client5", 0.5, "USD", 5),
                new ClientBid("client6", 0.5, "EUR", 5),
                // Cut off here
                new ClientBid("client7", 0.5, "EUR", 2000),
                new ClientBid("client8", 0.4, "USD", 100)
            };

            // Act
            var orderbook = _orderbookService.Render(clientBids.ToImmutableArray());

            // Assert
            Assert.AreEqual(0.5, orderbook.LkkPriceChf, Delta);
            Assert.AreEqual(2500 / 0.5, orderbook.InMoneyVolumeLkk, Delta);
            Assert.AreEqual(2100 / 0.5, orderbook.OutOfTheMoneyVolumeLkk, Delta);

            Assert.AreEqual(4, orderbook.InMoneyOrders.Count);

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

            Assert.AreEqual(2, orderbook.OutOfMoneyOrders.Count);

            Assert.AreEqual(0.5, orderbook.OutOfMoneyOrders[0].Price, Delta);
            Assert.AreEqual(1, orderbook.OutOfMoneyOrders[0].Investors);
            Assert.AreEqual(2000 / 0.5, orderbook.OutOfMoneyOrders[0].Volume, Delta);

            Assert.AreEqual(0.4, orderbook.OutOfMoneyOrders[1].Price, Delta);
            Assert.AreEqual(1, orderbook.OutOfMoneyOrders[1].Investors);
            Assert.AreEqual(100 / 0.5, orderbook.OutOfMoneyOrders[1].Volume, Delta);

            Assert.AreEqual(OrderbookBidState.InMoney, orderbook.TryGetBid("client1").State);
            Assert.AreEqual(OrderbookBidState.InMoney, orderbook.TryGetBid("client2").State);
            Assert.AreEqual(OrderbookBidState.InMoney, orderbook.TryGetBid("client3").State);
            Assert.AreEqual(OrderbookBidState.InMoney, orderbook.TryGetBid("client4").State);
            Assert.AreEqual(OrderbookBidState.InMoney, orderbook.TryGetBid("client5").State);
            Assert.AreEqual(OrderbookBidState.InMoney, orderbook.TryGetBid("client6").State);
            Assert.AreEqual(OrderbookBidState.OutOfTheMoney, orderbook.TryGetBid("client7").State);
            Assert.AreEqual(OrderbookBidState.OutOfTheMoney, orderbook.TryGetBid("client8").State);

            Assert.AreEqual(0.5, orderbook.TryGetBid("client1").LkkPriceChf, Delta);
            Assert.AreEqual(0.5, orderbook.TryGetBid("client2").LkkPriceChf, Delta);
            Assert.AreEqual(0.5, orderbook.TryGetBid("client3").LkkPriceChf, Delta);
            Assert.AreEqual(0.5, orderbook.TryGetBid("client4").LkkPriceChf, Delta);
            Assert.AreEqual(0.5, orderbook.TryGetBid("client5").LkkPriceChf, Delta);
            Assert.AreEqual(0.5, orderbook.TryGetBid("client6").LkkPriceChf, Delta);
            Assert.AreEqual(0.5, orderbook.TryGetBid("client7").LkkPriceChf, Delta);
            Assert.AreEqual(0.5, orderbook.TryGetBid("client8").LkkPriceChf, Delta);

            Assert.AreEqual(2, orderbook.TryGetBid("client1").InMoneyAssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client2").InMoneyAssetVolumesLkk.Count);
            Assert.AreEqual(2, orderbook.TryGetBid("client3").InMoneyAssetVolumesLkk.Count);
            Assert.AreEqual(2, orderbook.TryGetBid("client4").InMoneyAssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client5").InMoneyAssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client6").InMoneyAssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client7").InMoneyAssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client8").InMoneyAssetVolumesLkk.Count);

            Assert.AreEqual(250 / 0.5, orderbook.TryGetBid("client1").InMoneyAssetVolumesLkk.Single(v => v.Key == "USD").Value, Delta);
            Assert.AreEqual(125 / 0.5, orderbook.TryGetBid("client1").InMoneyAssetVolumesLkk.Single(v => v.Key == "CHF").Value, Delta);
            Assert.AreEqual(125 / 0.5, orderbook.TryGetBid("client2").InMoneyAssetVolumesLkk.Single().Value, Delta);
            Assert.AreEqual(300 / 0.5, orderbook.TryGetBid("client3").InMoneyAssetVolumesLkk.Single(v => v.Key == "USD").Value, Delta);
            Assert.AreEqual(150 / 0.5, orderbook.TryGetBid("client3").InMoneyAssetVolumesLkk.Single(v => v.Key == "EUR").Value, Delta);
            Assert.AreEqual(660 / 0.5, orderbook.TryGetBid("client4").InMoneyAssetVolumesLkk.Single(v => v.Key == "USD").Value, Delta);
            Assert.AreEqual(880 / 0.5, orderbook.TryGetBid("client4").InMoneyAssetVolumesLkk.Single(v => v.Key == "EUR").Value, Delta);
            Assert.AreEqual(5 / 0.5, orderbook.TryGetBid("client5").InMoneyAssetVolumesLkk.Single().Value, Delta);
            Assert.AreEqual(5 / 0.5, orderbook.TryGetBid("client6").InMoneyAssetVolumesLkk.Single().Value, Delta);
            Assert.AreEqual(0, orderbook.TryGetBid("client7").InMoneyAssetVolumesLkk.Single().Value, Delta);
            Assert.AreEqual(0, orderbook.TryGetBid("client8").InMoneyAssetVolumesLkk.Single().Value, Delta);

            Assert.AreEqual(2, orderbook.TryGetBid("client1").AssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client2").AssetVolumesLkk.Count);
            Assert.AreEqual(2, orderbook.TryGetBid("client3").AssetVolumesLkk.Count);
            Assert.AreEqual(2, orderbook.TryGetBid("client4").AssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client5").AssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client6").AssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client7").AssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client8").AssetVolumesLkk.Count);

            Assert.AreEqual(250 / 0.5, orderbook.TryGetBid("client1").AssetVolumesLkk.Single(v => v.Key == "USD").Value, Delta);
            Assert.AreEqual(125 / 0.5, orderbook.TryGetBid("client1").AssetVolumesLkk.Single(v => v.Key == "CHF").Value, Delta);
            Assert.AreEqual(125 / 0.5, orderbook.TryGetBid("client2").AssetVolumesLkk.Single().Value, Delta);
            Assert.AreEqual(300 / 0.5, orderbook.TryGetBid("client3").AssetVolumesLkk.Single(v => v.Key == "USD").Value, Delta);
            Assert.AreEqual(150 / 0.5, orderbook.TryGetBid("client3").AssetVolumesLkk.Single(v => v.Key == "EUR").Value, Delta);
            Assert.AreEqual(660 / 0.5, orderbook.TryGetBid("client4").AssetVolumesLkk.Single(v => v.Key == "USD").Value, Delta);
            Assert.AreEqual(880 / 0.5, orderbook.TryGetBid("client4").AssetVolumesLkk.Single(v => v.Key == "EUR").Value, Delta);
            Assert.AreEqual(5 / 0.5, orderbook.TryGetBid("client5").AssetVolumesLkk.Single().Value, Delta);
            Assert.AreEqual(5 / 0.5, orderbook.TryGetBid("client6").AssetVolumesLkk.Single().Value, Delta);
            Assert.AreEqual(2000 / 0.5, orderbook.TryGetBid("client7").AssetVolumesLkk.Single().Value, Delta);
            Assert.AreEqual(100 / 0.5, orderbook.TryGetBid("client8").AssetVolumesLkk.Single().Value, Delta);
        }

        [TestMethod]
        public void Is_orderbook_correct_when_all_lkk_sold_to_high_price_orders_with_lower_lkk_price()
        {
            // Asset
            var clientBids = new IClientBid[]
            {
                new ClientBid("client1", 2.2, "USD", 1500),
                new ClientBid("client2", 2.0, "USD", 1000).SetVolume("CHF", 500),
                new ClientBid("client3", 2.0, "USD", 800).SetVolume("EUR", 600),
                new ClientBid("client4", 1.0, "USD", 100),
                // Cut off here with price 0.9 (= 4500 / 5000)
                new ClientBid("client5", 0.5, "USD", 500),
                new ClientBid("client6", 0.5, "EUR", 250),
                new ClientBid("client7", 0.4, "USD", 100)
            };

            // Act
            var orderbook = _orderbookService.Render(clientBids.ToImmutableArray());

            // Assert
            Assert.AreEqual(0.9, orderbook.LkkPriceChf, Delta);
            Assert.AreEqual(4500 / 0.9, orderbook.InMoneyVolumeLkk, Delta);
            Assert.AreEqual(850 / 0.9, orderbook.OutOfTheMoneyVolumeLkk, Delta);

            Assert.AreEqual(3, orderbook.InMoneyOrders.Count);

            Assert.AreEqual(2.2, orderbook.InMoneyOrders[0].Price, Delta);
            Assert.AreEqual(1, orderbook.InMoneyOrders[0].Investors);
            Assert.AreEqual(1500 / 0.9, orderbook.InMoneyOrders[0].Volume, Delta);

            Assert.AreEqual(2.0, orderbook.InMoneyOrders[1].Price, Delta);
            Assert.AreEqual(2, orderbook.InMoneyOrders[1].Investors);
            Assert.AreEqual(2900 / 0.9, orderbook.InMoneyOrders[1].Volume, Delta);

            Assert.AreEqual(1.0, orderbook.InMoneyOrders[2].Price, Delta);
            Assert.AreEqual(1, orderbook.InMoneyOrders[2].Investors);
            Assert.AreEqual(100 / 0.9, orderbook.InMoneyOrders[2].Volume, Delta);

            Assert.AreEqual(2, orderbook.OutOfMoneyOrders.Count);

            Assert.AreEqual(0.5, orderbook.OutOfMoneyOrders[0].Price, Delta);
            Assert.AreEqual(2, orderbook.OutOfMoneyOrders[0].Investors);
            Assert.AreEqual(750 / 0.9, orderbook.OutOfMoneyOrders[0].Volume, Delta);

            Assert.AreEqual(0.4, orderbook.OutOfMoneyOrders[1].Price, Delta);
            Assert.AreEqual(1, orderbook.OutOfMoneyOrders[1].Investors);
            Assert.AreEqual(100 / 0.9, orderbook.OutOfMoneyOrders[1].Volume, Delta);

            Assert.AreEqual(OrderbookBidState.InMoney, orderbook.TryGetBid("client1").State);
            Assert.AreEqual(OrderbookBidState.InMoney, orderbook.TryGetBid("client2").State);
            Assert.AreEqual(OrderbookBidState.InMoney, orderbook.TryGetBid("client3").State);
            Assert.AreEqual(OrderbookBidState.InMoney, orderbook.TryGetBid("client4").State);
            Assert.AreEqual(OrderbookBidState.OutOfTheMoney, orderbook.TryGetBid("client5").State);
            Assert.AreEqual(OrderbookBidState.OutOfTheMoney, orderbook.TryGetBid("client6").State);
            Assert.AreEqual(OrderbookBidState.OutOfTheMoney, orderbook.TryGetBid("client7").State);

            Assert.AreEqual(0.9, orderbook.TryGetBid("client1").LkkPriceChf, Delta);
            Assert.AreEqual(0.9, orderbook.TryGetBid("client2").LkkPriceChf, Delta);
            Assert.AreEqual(0.9, orderbook.TryGetBid("client3").LkkPriceChf, Delta);
            Assert.AreEqual(0.9, orderbook.TryGetBid("client4").LkkPriceChf, Delta);
            Assert.AreEqual(0.9, orderbook.TryGetBid("client5").LkkPriceChf, Delta);
            Assert.AreEqual(0.9, orderbook.TryGetBid("client6").LkkPriceChf, Delta);
            Assert.AreEqual(0.9, orderbook.TryGetBid("client7").LkkPriceChf, Delta);

            Assert.AreEqual(1, orderbook.TryGetBid("client1").InMoneyAssetVolumesLkk.Count);
            Assert.AreEqual(2, orderbook.TryGetBid("client2").InMoneyAssetVolumesLkk.Count);
            Assert.AreEqual(2, orderbook.TryGetBid("client3").InMoneyAssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client4").InMoneyAssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client5").InMoneyAssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client6").InMoneyAssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client7").InMoneyAssetVolumesLkk.Count);

            Assert.AreEqual(1500 / 0.9, orderbook.TryGetBid("client1").InMoneyAssetVolumesLkk.Single().Value, Delta);
            Assert.AreEqual(1000 / 0.9, orderbook.TryGetBid("client2").InMoneyAssetVolumesLkk.Single(v => v.Key == "USD").Value, Delta);
            Assert.AreEqual(500 / 0.9, orderbook.TryGetBid("client2").InMoneyAssetVolumesLkk.Single(v => v.Key == "CHF").Value, Delta);
            Assert.AreEqual(800 / 0.9, orderbook.TryGetBid("client3").InMoneyAssetVolumesLkk.Single(v => v.Key == "USD").Value, Delta);
            Assert.AreEqual(600 / 0.9, orderbook.TryGetBid("client3").InMoneyAssetVolumesLkk.Single(v => v.Key == "EUR").Value, Delta);
            Assert.AreEqual(100 / 0.9, orderbook.TryGetBid("client4").InMoneyAssetVolumesLkk.Single().Value, Delta);
            Assert.AreEqual(0, orderbook.TryGetBid("client5").InMoneyAssetVolumesLkk.Single().Value, Delta);
            Assert.AreEqual(0, orderbook.TryGetBid("client6").InMoneyAssetVolumesLkk.Single().Value, Delta);
            Assert.AreEqual(0, orderbook.TryGetBid("client7").InMoneyAssetVolumesLkk.Single().Value, Delta);

            Assert.AreEqual(1, orderbook.TryGetBid("client1").AssetVolumesLkk.Count);
            Assert.AreEqual(2, orderbook.TryGetBid("client2").AssetVolumesLkk.Count);
            Assert.AreEqual(2, orderbook.TryGetBid("client3").AssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client4").AssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client5").AssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client6").AssetVolumesLkk.Count);
            Assert.AreEqual(1, orderbook.TryGetBid("client7").AssetVolumesLkk.Count);

            Assert.AreEqual(1500 / 0.9, orderbook.TryGetBid("client1").AssetVolumesLkk.Single().Value, Delta);
            Assert.AreEqual(1000 / 0.9, orderbook.TryGetBid("client2").AssetVolumesLkk.Single(v => v.Key == "USD").Value, Delta);
            Assert.AreEqual(500 / 0.9, orderbook.TryGetBid("client2").AssetVolumesLkk.Single(v => v.Key == "CHF").Value, Delta);
            Assert.AreEqual(800 / 0.9, orderbook.TryGetBid("client3").AssetVolumesLkk.Single(v => v.Key == "USD").Value, Delta);
            Assert.AreEqual(600 / 0.9, orderbook.TryGetBid("client3").AssetVolumesLkk.Single(v => v.Key == "EUR").Value, Delta);
            Assert.AreEqual(100 / 0.9, orderbook.TryGetBid("client4").AssetVolumesLkk.Single().Value, Delta);
            Assert.AreEqual(500 / 0.9, orderbook.TryGetBid("client5").AssetVolumesLkk.Single().Value, Delta);
            Assert.AreEqual(250 / 0.9, orderbook.TryGetBid("client6").AssetVolumesLkk.Single().Value, Delta);
            Assert.AreEqual(100 / 0.9, orderbook.TryGetBid("client7").AssetVolumesLkk.Single().Value, Delta);
        }
    }
}