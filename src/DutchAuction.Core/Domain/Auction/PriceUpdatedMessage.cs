using System;

namespace DutchAuction.Core.Domain.Auction
{
    public class PriceUpdatedMessage
    {
        public double Price { get; set; }
        public double InMoneyVolume { get; set; }
        public double OutOfTheMoneyVolume { get; set; }
        public DateTime Timestamp { get; set; }
    }
}