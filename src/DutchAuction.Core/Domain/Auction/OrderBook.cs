namespace DutchAuction.Core.Domain.Auction
{
    public class Orderbook
    {
        public Order[] InMoneyOrders { get; set; }
        public Order[] OutOfMoneyOrders { get; set; }
        public double CurrentPrice { get; set; }
        public double CurrentInMoneyVolume { get; set; }
        public double CurrentOutOfTheMoneyVolume { get; set; }
    }
}