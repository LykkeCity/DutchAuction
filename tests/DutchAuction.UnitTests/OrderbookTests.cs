using System.Linq;
using System.Threading.Tasks;
using Autofac;
using DutchAuction.Services.Lots;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DutchAuction.UnitTests
{

    [TestClass]
    public class OrderbookTests : BaseTests
    {
        private AuctionLotManager _auctionLotManager;

        [TestInitialize]
        public void InitializeTest()
        {
            _auctionLotManager = Container.Resolve<AuctionLotManager>();
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
