using System;
using Autofac;
using DutchAuction.Core.Domain.Auction;

namespace DutchAuction.Core.Services.Auction
{
    /// <summary>
    /// Doing overall auction management
    /// </summary>
    public interface IAuctionManager : IStartable
    {
        Orderbook GetOrderbook();

        IBid TryGetBid(string clientId);

        /// <summary>
        /// Starts client`s bidding
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="assetId"></param>
        /// <param name="price">Price in CHF</param>
        /// <param name="volume">Volume of given <paramref name="assetId"/></param>
        /// <param name="date"></param>
        AuctionOperationResult StartBidding(string clientId, string assetId, double price, double volume, DateTime date);

        /// <summary>
        /// Accepts new client`s price bid
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="price">Price in CHF</param>
        /// <param name="date"></param>
        AuctionOperationResult AcceptPriceBid(string clientId, double price, DateTime date);

        /// <summary>
        /// Accepts new client`s volume bid
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="assetId"></param>
        /// <param name="volume">Volume change of given <paramref name="assetId"/></param>
        /// <param name="date"></param>
        AuctionOperationResult AcceptVolumeBid(string clientId, string assetId, double volume, DateTime date);
    }
}