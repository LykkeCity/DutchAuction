using System.Linq;
using System.Threading.Tasks;
using Autofac;
using DutchAuction.Core;
using DutchAuction.Services;
using NUnit.Framework;

namespace DutchAuction.UnitTests
{
    [TestFixture]
    public class OrderbookTests : BaseTests
    {
        private IAuctionLotCacheService _auctionLotCacheService;
        private AuctionLotManager _auctionLotManager;

        [OneTimeSetUp]
        public void SetUp()
        {
            RegisterDependencies();
            _auctionLotCacheService = Container.Resolve<IAuctionLotCacheService>();
            _auctionLotManager = Container.Resolve<AuctionLotManager>();
        }

        [Test]
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
