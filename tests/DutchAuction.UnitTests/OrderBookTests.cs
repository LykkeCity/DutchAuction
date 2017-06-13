using DutchAuction.Core.Services.Assets;
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

        [TestInitialize]
        public void InitializeTest()
        {
            _assetExchangeServiceMock = new Mock<IAssetExchangeService>();

            _orderbookService = new OrderbookService(_assetExchangeServiceMock.Object, 
                totalAuctionVolume: 50000, 
                minClosingBidCutoffVolume: 100);
        }
    }
}