using DutchAuction.Core.Services.Auction;
using DutchAuction.Services.Auction;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DutchAuction.UnitTests
{

    [TestClass]
    public class BidsServiceTests
    {
        private BidsService _bidsService;

        [TestInitialize]
        public void InitializeTest()
        {
            _bidsService = new BidsService();
        }

        [TestMethod]
        public void Is_client_can_start_bidding()
        {
            // Act
            var result = _bidsService.StartBidding("client1", "USD", 100, 100);

            // Assert
            Assert.AreEqual(AuctionOperationResult.Ok, result);
        }

        [TestMethod]
        public void Is_client_cant_start_bidding_twice()
        {
            // Arrange
            _bidsService.StartBidding("client1", "EUR", 200, 20);

            // Act
            var result = _bidsService.StartBidding("client1", "USD", 100, 10);

            // Assert
            Assert.AreEqual(AuctionOperationResult.ClientHasAlreadyDoneBid, result);
        }

        [TestMethod]
        public void Is_client_can_increase_price()
        {
            // Arrange
            _bidsService.StartBidding("client1", "EUR", 200, 20);

            // Act
            var result = _bidsService.SetPrice("client1", 201);

            // Assert
            Assert.AreEqual(AuctionOperationResult.Ok, result);
        }

        [TestMethod]
        public void Is_client_can_increase_volume()
        {
            // Arrange
            _bidsService.StartBidding("client1", "EUR", 200, 20);

            // Act
            var result = _bidsService.SetAssetVolume("client1", "EUR", 21);

            // Assert
            Assert.AreEqual(AuctionOperationResult.Ok, result);
        }

        [TestMethod]
        public void Is_client_cant_decrease_price()
        {
            // Arrange
            _bidsService.StartBidding("client1", "EUR", 200, 20);

            // Act
            var result = _bidsService.SetPrice("client1", 199);

            // Assert
            Assert.AreEqual(AuctionOperationResult.PriceIsLessThanCurrentBidPrice, result);
        }

        [TestMethod]
        public void Is_client_cant_decrease_volume()
        {
            // Arrange
            _bidsService.StartBidding("client1", "EUR", 200, 20);

            // Act
            var result = _bidsService.SetAssetVolume("client1", "EUR", 19);

            // Assert
            Assert.AreEqual(AuctionOperationResult.VolumeIsLessThanCurrentBidAssetVolume, result);
        }

        [TestMethod]
        public void Is_client_can_add_new_asset()
        {
            // Arrange
            _bidsService.StartBidding("client1", "EUR", 200, 20);

            // Act
            var result = _bidsService.SetAssetVolume("client1", "USD", 5);

            // Assert
            Assert.AreEqual(AuctionOperationResult.Ok, result);
        }

        [TestMethod]
        public void Is_client_cant_decrease_new_asset_volume()
        {
            // Arrange
            _bidsService.StartBidding("client1", "EUR", 200, 20);
            _bidsService.SetAssetVolume("client1", "USD", 5);

            // Act
            var result = _bidsService.SetAssetVolume("client1", "USD", 4);

            // Assert
            Assert.AreEqual(AuctionOperationResult.VolumeIsLessThanCurrentBidAssetVolume, result);
        }
    }
}
