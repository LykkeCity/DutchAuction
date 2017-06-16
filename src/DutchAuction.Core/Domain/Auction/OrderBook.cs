using System.Collections.Immutable;

namespace DutchAuction.Core.Domain.Auction
{
    public class Orderbook : IOrderbook
    {
        public double LkkPriceChf { get; }
        public double InMoneyVolumeLkk { get; }
        public double OutOfTheMoneyVolumeLkk { get; }
        public IImmutableList<Order> InMoneyOrders { get; }
        public IImmutableList<Order> OutOfMoneyOrders { get; }
        
        private readonly IImmutableDictionary<string, OrderbookBid> _bids;

        public Orderbook(
            double lkkPriceChf,
            double inMoneyVolumeLkk,
            double outOfTheMoneyVolumeLkk,
            IImmutableList<Order> inMoneyOrders,
            IImmutableList<Order> outOfMoneyOrders,
            IImmutableDictionary<string, OrderbookBid> bids)
        {
            _bids = bids;

            InMoneyOrders = inMoneyOrders;
            OutOfMoneyOrders = outOfMoneyOrders;
            LkkPriceChf = lkkPriceChf;
            InMoneyVolumeLkk = inMoneyVolumeLkk;
            OutOfTheMoneyVolumeLkk = outOfTheMoneyVolumeLkk;
        }

        public IOrderbookBid TryGetBid(string clientId)
        {
            _bids.TryGetValue(clientId, out OrderbookBid bid);

            return bid;
        }
    }
}