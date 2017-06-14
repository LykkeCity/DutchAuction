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
        private Mock<IAssetExchangeService> _assetExchangeServiceMock;
        private OrderbookService _orderbookService;
        private Mock<IBidsService> _bidsServiceMock;

        [TestInitialize]
        public void InitializeTest()
        {
            _assetExchangeServiceMock = new Mock<IAssetExchangeService>();
            _bidsServiceMock = new Mock<IBidsService>();

            _orderbookService = new OrderbookService(_assetExchangeServiceMock.Object, _bidsServiceMock.Object, 
                totalAuctionVolume: 50000, 
                minClosingBidCutoffVolume: 100);
        }

        [TestMethod]
        public void Is_two_bids_with_one_price_perofrms_single_order()
        {
            // Arrange
            _assetExchangeServiceMock
                .Setup(s => s.Exchange(It.IsAny<double>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns<double, string, string>((amount, baseAssetId, targetAssetId) => amount);

            _bidsServiceMock.Setup(s => s.GetAll())
                .Returns(() => new IBid[]
                {
                    new Bid("client1", 100, "USD", 10),
                    new Bid("client1", 100, "USD", 20),
                });

            // Act
            var orderbook = _orderbookService.Render();

            // Assert
            Assert.AreEqual(1, orderbook.Orders.Length);
            Assert.AreEqual(100, orderbook.Orders[0].Price);
        }
    }
}