using System.Linq;
using DutchAuction.Core.Domain.Auction;
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
            _bidsService = new BidsService(null);
        }

        [TestMethod]
        public void Is_client_can_start_bidding()
        {
            // Act
            var result = _bidsService.StartBidding("client1", "USD", 100, 10);

            // Assert
            var bid = _bidsService.TryGetBid("client1");

            Assert.AreEqual(AuctionOperationResult.Ok, result);
            Assert.IsNotNull(bid);
            Assert.AreEqual("client1", bid.ClientId);
            Assert.AreEqual(100, bid.LimitPriceChf);
            Assert.AreEqual(1, bid.AssetVolumes.Count);
            Assert.AreEqual(1, bid.AssetVolumes.Count(i => i.Key == "USD"));
            Assert.AreEqual(10, bid.AssetVolumes.Single(i => i.Key == "USD").Value);
            Assert.AreEqual(BidState.NotCalculatedYet, bid.State);
            Assert.AreEqual(0, bid.InMoneyAssetVolumesLkk.Count);
        }

        [TestMethod]
        public void Is_two_clients_can_bidding()
        {
            // Act
            var result1 = _bidsService.StartBidding("client1", "USD", 100, 10);
            var result2 = _bidsService.StartBidding("client2", "EUR", 200, 20);

            // Assert
            var bid1 = _bidsService.TryGetBid("client1");
            var bid2 = _bidsService.TryGetBid("client2");

            Assert.AreEqual(AuctionOperationResult.Ok, result1);
            Assert.AreEqual(AuctionOperationResult.Ok, result2);
            Assert.IsNotNull(bid1);
            Assert.IsNotNull(bid2);
            Assert.AreEqual("client1", bid1.ClientId);
            Assert.AreEqual("client2", bid2.ClientId);
            Assert.AreEqual(100, bid1.LimitPriceChf);
            Assert.AreEqual(200, bid2.LimitPriceChf);
            Assert.AreEqual(1, bid1.AssetVolumes.Count);
            Assert.AreEqual(1, bid2.AssetVolumes.Count);
            Assert.AreEqual(1, bid1.AssetVolumes.Count(i => i.Key == "USD"));
            Assert.AreEqual(1, bid2.AssetVolumes.Count(i => i.Key == "EUR"));
            Assert.AreEqual(10, bid1.AssetVolumes.Single(i => i.Key == "USD").Value);
            Assert.AreEqual(20, bid2.AssetVolumes.Single(i => i.Key == "EUR").Value);
            Assert.AreEqual(BidState.NotCalculatedYet, bid1.State);
            Assert.AreEqual(BidState.NotCalculatedYet, bid2.State);
            Assert.AreEqual(0, bid1.InMoneyAssetVolumesLkk.Count);
            Assert.AreEqual(0, bid2.InMoneyAssetVolumesLkk.Count);
        }

        [TestMethod]
        public void Is_client_cant_start_bidding_twice()
        {
            // Arrange
            _bidsService.StartBidding("client1", "USD", 100, 10);

            // Act
            var result = _bidsService.StartBidding("client1", "EUR", 200, 20);

            // Assert
            var bid = _bidsService.TryGetBid("client1");

            Assert.AreEqual(AuctionOperationResult.ClientHasAlreadyDoneBid, result);
            Assert.IsNotNull(bid);
            Assert.AreEqual("client1", bid.ClientId);
            Assert.AreEqual(100, bid.LimitPriceChf);
            Assert.AreEqual(1, bid.AssetVolumes.Count);
            Assert.AreEqual(1, bid.AssetVolumes.Count(i => i.Key == "USD"));
            Assert.AreEqual(10, bid.AssetVolumes.Single(i => i.Key == "USD").Value);
            Assert.AreEqual(BidState.NotCalculatedYet, bid.State);
            Assert.AreEqual(0, bid.InMoneyAssetVolumesLkk.Count);
        }

        [TestMethod]
        public void Is_client_can_increase_price()
        {
            // Arrange
            _bidsService.StartBidding("client1", "USD", 100, 10);

            // Act
            var result = _bidsService.SetPrice("client1", 101);

            // Assert
            var bid = _bidsService.TryGetBid("client1");

            Assert.AreEqual(AuctionOperationResult.Ok, result);
            Assert.IsNotNull(bid);
            Assert.AreEqual("client1", bid.ClientId);
            Assert.AreEqual(101, bid.LimitPriceChf);
            Assert.AreEqual(1, bid.AssetVolumes.Count);
            Assert.AreEqual(1, bid.AssetVolumes.Count(i => i.Key == "USD"));
            Assert.AreEqual(10, bid.AssetVolumes.Single(i => i.Key == "USD").Value);
            Assert.AreEqual(BidState.NotCalculatedYet, bid.State);
            Assert.AreEqual(0, bid.InMoneyAssetVolumesLkk.Count);
        }

        [TestMethod]
        public void Is_client_can_increase_volume()
        {
            // Arrange
            _bidsService.StartBidding("client1", "USD", 100, 10);

            // Act
            var result = _bidsService.SetAssetVolume("client1", "USD", 11);

            // Assert
            var bid = _bidsService.TryGetBid("client1");

            Assert.AreEqual(AuctionOperationResult.Ok, result);
            Assert.IsNotNull(bid);
            Assert.AreEqual("client1", bid.ClientId);
            Assert.AreEqual(100, bid.LimitPriceChf);
            Assert.AreEqual(1, bid.AssetVolumes.Count);
            Assert.AreEqual(1, bid.AssetVolumes.Count(i => i.Key == "USD"));
            Assert.AreEqual(11, bid.AssetVolumes.Single(i => i.Key == "USD").Value);
            Assert.AreEqual(BidState.NotCalculatedYet, bid.State);
            Assert.AreEqual(0, bid.InMoneyAssetVolumesLkk.Count);
        }

        [TestMethod]
        public void Is_client_cant_decrease_price()
        {
            // Arrange
            _bidsService.StartBidding("client1", "USD", 100, 10);

            // Act
            var result = _bidsService.SetPrice("client1", 99);

            // Assert
            var bid = _bidsService.TryGetBid("client1");

            Assert.AreEqual(AuctionOperationResult.PriceIsLessThanCurrentBidPrice, result);
            Assert.IsNotNull(bid);
            Assert.AreEqual("client1", bid.ClientId);
            Assert.AreEqual(100, bid.LimitPriceChf);
            Assert.AreEqual(1, bid.AssetVolumes.Count);
            Assert.AreEqual(1, bid.AssetVolumes.Count(i => i.Key == "USD"));
            Assert.AreEqual(10, bid.AssetVolumes.Single(i => i.Key == "USD").Value);
            Assert.AreEqual(BidState.NotCalculatedYet, bid.State);
            Assert.AreEqual(0, bid.InMoneyAssetVolumesLkk.Count);
        }

        [TestMethod]
        public void Is_client_cant_decrease_volume()
        {
            // Arrange
            _bidsService.StartBidding("client1", "USD", 100, 10);

            // Act
            var result = _bidsService.SetAssetVolume("client1", "USD", 9);

            // Assert
            var bid = _bidsService.TryGetBid("client1");

            Assert.AreEqual(AuctionOperationResult.VolumeIsLessThanCurrentBidAssetVolume, result);
            Assert.IsNotNull(bid);
            Assert.AreEqual("client1", bid.ClientId);
            Assert.AreEqual(100, bid.LimitPriceChf);
            Assert.AreEqual(1, bid.AssetVolumes.Count);
            Assert.AreEqual(1, bid.AssetVolumes.Count(i => i.Key == "USD"));
            Assert.AreEqual(10, bid.AssetVolumes.Single(i => i.Key == "USD").Value);
            Assert.AreEqual(BidState.NotCalculatedYet, bid.State);
            Assert.AreEqual(0, bid.InMoneyAssetVolumesLkk.Count);
        }

        [TestMethod]
        public void Is_client_can_add_new_asset()
        {
            // Arrange
            _bidsService.StartBidding("client1", "USD", 100, 10);

            // Act
            var result = _bidsService.SetAssetVolume("client1", "EUR", 20);

            // Assert
            var bid = _bidsService.TryGetBid("client1");

            Assert.AreEqual(AuctionOperationResult.Ok, result);
            Assert.IsNotNull(bid);
            Assert.AreEqual("client1", bid.ClientId);
            Assert.AreEqual(100, bid.LimitPriceChf);
            Assert.AreEqual(2, bid.AssetVolumes.Count);
            Assert.AreEqual(1, bid.AssetVolumes.Count(i => i.Key == "USD"));
            Assert.AreEqual(1, bid.AssetVolumes.Count(i => i.Key == "EUR"));          
            Assert.AreEqual(10, bid.AssetVolumes.Single(i => i.Key == "USD").Value);
            Assert.AreEqual(20, bid.AssetVolumes.Single(i => i.Key == "EUR").Value);
            Assert.AreEqual(BidState.NotCalculatedYet, bid.State);
            Assert.AreEqual(0, bid.InMoneyAssetVolumesLkk.Count);
        }

        [TestMethod]
        public void Is_client_cant_decrease_new_asset_volume()
        {
            // Arrange
            _bidsService.StartBidding("client1", "USD", 100, 10);
            _bidsService.SetAssetVolume("client1", "EUR", 20);

            // Act
            var result = _bidsService.SetAssetVolume("client1", "EUR", 19);

            // Assert
            var bid = _bidsService.TryGetBid("client1");

            Assert.AreEqual(AuctionOperationResult.VolumeIsLessThanCurrentBidAssetVolume, result);
            Assert.IsNotNull(bid);
            Assert.AreEqual("client1", bid.ClientId);
            Assert.AreEqual(100, bid.LimitPriceChf);
            Assert.AreEqual(2, bid.AssetVolumes.Count);
            Assert.AreEqual(1, bid.AssetVolumes.Count(i => i.Key == "USD"));
            Assert.AreEqual(1, bid.AssetVolumes.Count(i => i.Key == "EUR"));
            Assert.AreEqual(10, bid.AssetVolumes.Single(i => i.Key == "USD").Value);
            Assert.AreEqual(20, bid.AssetVolumes.Single(i => i.Key == "EUR").Value);
            Assert.AreEqual(BidState.NotCalculatedYet, bid.State);
            Assert.AreEqual(0, bid.InMoneyAssetVolumesLkk.Count);
        }
    }
}
