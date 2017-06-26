using System;
using System.Threading.Tasks;
using DutchAuction.Core.Domain.MarketProfile;
using DutchAuction.Core.Services.Assets;
using DutchAuction.Core.Services.MarketProfile;
using DutchAuction.Services.Assets;
using Lykke.Service.Assets.Client.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DutchAuction.UnitTests
{
    [TestClass]
    public class AssetExchangeTests
    {
        private AssetExchangeService _exchangeService;
        private Mock<IMarketProfileManager> _marketProfileManagerMock;
        private Mock<IAssetPairsManager> _assetPairsManagerMock;

        [TestInitialize]
        public void InitializeTest()
        {
            _marketProfileManagerMock = new Mock<IMarketProfileManager>();
            _assetPairsManagerMock = new Mock<IAssetPairsManager>();

            _exchangeService = new AssetExchangeService(_marketProfileManagerMock.Object, _assetPairsManagerMock.Object);
        }

        [TestMethod]
        public async Task Is_Forward_Exchange_Correct()
        {
            // Arrange
            _marketProfileManagerMock
                .Setup(m => m.TryGetPair(
                    It.Is<string>(s => s == "EUR"),
                    It.Is<string>(s => s == "USD")))
                .Returns(new MarketProfileAssetPair
                {
                    AssetPair = "EURUSD",
                    AskPrice = 1.11985,
                    BidPrice = 1.11933
                });

            _assetPairsManagerMock
                .Setup(m => m.GetEnabledPairAsync(It.Is<string>(s => s == "EURUSD")))
                .ReturnsAsync(new AssetPairResponseModel
                {
                    Accuracy = 3,
                    InvertedAccuracy = 5,
                    IsDisabled = false
                });

            // Act
            var result = await _exchangeService.ExchangeAsync(10, "EUR", "USD");

            // Assert
            Assert.AreEqual(11.193, result); // 10 * 1.11933 rounded to 3 decimal places
        }

        [TestMethod]
        public async Task Is_Inverted_Exchange_Correct()
        {
            // Arrange
            _marketProfileManagerMock
                .Setup(m => m.TryGetPair(
                    It.Is<string>(s => s == "EUR"),
                    It.Is<string>(s => s == "USD")))
                .Returns(new MarketProfileAssetPair
                {
                    AssetPair = "EURUSD",
                    AskPrice = 1.11985,
                    BidPrice = 1.11933
                });

            _assetPairsManagerMock
                .Setup(m => m.GetEnabledPairAsync(It.Is<string>(s => s == "EURUSD")))
                .ReturnsAsync(new AssetPairResponseModel
                {
                    Accuracy = 3,
                    InvertedAccuracy = 5,
                    IsDisabled = false
                });
            
            // Act
            var result = await _exchangeService.ExchangeAsync(10, "USD", "EUR");

            // Assert
            Assert.AreEqual(8.92977, result); // 10 / 1.11985 rounded to 5 decimal places
        }

        [TestMethod]
        public async Task Is_Exchange_To_The_Same_Asset_Correct()
        {
            // Act
            var result = await _exchangeService.ExchangeAsync(10, "CHF", "CHF");

            // Assert
            Assert.AreEqual(10, result);
        }

        [TestMethod]
        public async Task Is_Disabled_Pair_Usage_Throws()
        {
            // Arrange
            _assetPairsManagerMock
                .Setup(m => m.GetEnabledPairAsync(It.Is<string>(s => s == "USDCHF")))
                .ReturnsAsync(new AssetPairResponseModel
                {
                    IsDisabled = true
                });

            // Act/Assert
            await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(async () => await _exchangeService.ExchangeAsync(10, "USD", "CHF"));
        }

        [TestMethod]
        public async Task Is_Not_Existing_Pair_Usage_Throws()
        {
            // Arrange
            _marketProfileManagerMock
                .Setup(m => m.TryGetPair(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns<string, string>((baseAssetId, targetAssetId) => null);

            // Act/Assert
            await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(async () => await _exchangeService.ExchangeAsync(10, "USD", "CHF"));
        }
    }
}
