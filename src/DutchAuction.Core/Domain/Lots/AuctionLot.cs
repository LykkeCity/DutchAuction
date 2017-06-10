using System;

namespace DutchAuction.Core.Domain.Lots
{
    public class AuctionLot : IAuctionLot
    {
        public string ClientId { get; set; }
        public string AssetId { get; set; }
        public double Volume { get; set; }
        public double Price { get; set; }
        public DateTime Date { get; set; }

        public static AuctionLot Create(IAuctionLot src)
        {
            return new AuctionLot
            {
                AssetId = src.AssetId,
                ClientId = src.ClientId,
                Date = src.Date,
                Price = src.Price,
                Volume = src.Volume
            };
        }
    }
}