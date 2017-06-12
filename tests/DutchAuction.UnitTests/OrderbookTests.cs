using System.Linq;
using AzureStorage.Tables;
using Common.Log;
using DutchAuction.Core.Services.Assets;
using DutchAuction.Repositories.Lots;
using DutchAuction.Services.Lots;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DutchAuction.UnitTests
{

    [TestClass]
    public class OrderbookTests
    {
        private AuctionLotManager _auctionLotManager;

        [TestInitialize]
        public void InitializeTest()
        {
            var assetExchangeServiceMock = new Mock<IAssetExchangeService>();

            assetExchangeServiceMock
                .Setup(s => s.Exchange(
                    It.IsAny<double>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns<double, string, string>((amount, baseAssetId, targetAssetId) => amount);

            var logMock = new Mock<ILog>();

            _auctionLotManager = new AuctionLotManager(
                new AuctionLotRepository(new NoSqlTableInMemory<AuctionLotEntity>()),
                new AuctionLotCacheService(),
                assetExchangeServiceMock.Object,
                logMock.Object);
        }

        [TestMethod]
        public void Is_Orderbook_Correct()
        {
            // Asset
            _auctionLotManager.Add("client1", "USD", 100, 50);
            _auctionLotManager.Add("client1", "USD", 50, 100);
            _auctionLotManager.Add("client2", "USD", 1000, 5);
            _auctionLotManager.Add("client2", "USD", 100, 10);

            // Act
            var orderbook = _auctionLotManager.GetOrderbook();

            // Assert
            Assert.IsTrue(orderbook.Count(item => item.Price == 100 && item.Volume == 60) == 1);
        }
    }
}
