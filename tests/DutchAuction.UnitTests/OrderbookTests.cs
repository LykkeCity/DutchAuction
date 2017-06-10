using System.Linq;
using System.Threading.Tasks;
using AzureStorage.Tables;
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

            _auctionLotManager = new AuctionLotManager(
                new AuctionLotRepository(new NoSqlTableInMemory<AuctionLotEntity>()),
                new AuctionLotCacheService(),
                assetExchangeServiceMock.Object);
        }

        [TestMethod]
        public async Task Is_Orderbook_Correct()
        {
            // Asset
            await _auctionLotManager.AddAsync("client1", "USD", 100, 50);
            await _auctionLotManager.AddAsync("client1", "USD", 50, 100);
            await _auctionLotManager.AddAsync("client2", "USD", 1000, 5);
            await _auctionLotManager.AddAsync("client2", "USD", 100, 10);

            // Act
            var orderbook = _auctionLotManager.GetOrderbook();

            // Assert
            Assert.IsTrue(orderbook.Count(item => item.Price == 100 && item.Volume == 60) == 1);
        }
    }
}
