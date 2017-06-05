using System.Linq;
using System.Threading.Tasks;
using Autofac;
using DutchAuction.Core.Services;
using DutchAuction.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DutchAuction.UnitTests
{
    [TestClass]
    public class OrderbookTests : BaseTests
    {
        private IAuctionLotCacheService _auctionLotCacheService;
        private AuctionLotManager _auctionLotManager;

        [TestInitialize]
        public void InitializeTest()
        {
            _auctionLotCacheService = Container.Resolve<IAuctionLotCacheService>();
            _auctionLotManager = Container.Resolve<AuctionLotManager>();
        }

        [TestMethod]
        public async Task Is_Orderbook_Correct()
        {
            await _auctionLotManager.AddAsync("client1", "USD", 100, 50);
            await _auctionLotManager.AddAsync("client1", "USD", 50, 100);
            await _auctionLotManager.AddAsync("client2", "USD", 1000, 5);
            await _auctionLotManager.AddAsync("client2", "USD", 100, 10);

            var orderbook = _auctionLotCacheService.GetOrderbook();

            Assert.IsTrue(orderbook.Any(item => item.Price == 100 && item.Volume == 60));
        }
    }
}
