namespace DutchAuction.Api.Models
{
    public class OrderbookResponse
    {
        public double Price { get; set; }
        public double InMoneyVolume { get; set; }
        public double OutOfTheMoneyVolume { get; set; }
        public Order Orders { get; set; }

        public class Order
        {
            public double Price { get; set; }
            public double Volume { get; set; }
            public int Investors { get; set; }
        }
    }
}