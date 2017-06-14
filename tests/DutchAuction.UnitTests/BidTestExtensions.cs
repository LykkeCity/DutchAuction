using DutchAuction.Core.Domain.Auction;

namespace DutchAuction.UnitTests
{
    internal static class BidTestExtensions
    {
        public static Bid SetVolumeFluently(this Bid bid, string assetId, double volume)
        {
            bid.SetVolume(assetId, volume);

            return bid;
        }
    }
}