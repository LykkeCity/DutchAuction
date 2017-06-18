using System;
using System.Collections.Immutable;

namespace DutchAuction.Core.Domain.Auction
{
    public class Orderbook : IOrderbook
    {
        public TimeSpan RenderDuration { get; }
        public int BidsCount => _bids.Count;
        public double LkkPriceChf { get; }
        public double InMoneyVolumeLkk { get; }
        public double OutOfTheMoneyVolumeLkk { get; }
        public IImmutableList<Order> InMoneyOrders { get; }
        public IImmutableList<Order> OutOfTheMoneyOrders { get; }
        
        private readonly IImmutableDictionary<string, OrderbookBid> _bids;

        public Orderbook(
            TimeSpan renderDuration,
            double lkkPriceChf,
            double inMoneyVolumeLkk,
            double outOfTheMoneyVolumeLkk,
            IImmutableList<Order> inMoneyOrders,
            IImmutableList<Order> outOfTheMoneyOrders,
            IImmutableDictionary<string, OrderbookBid> bids)
        {
            RenderDuration = renderDuration;
            InMoneyOrders = inMoneyOrders;
            OutOfTheMoneyOrders = outOfTheMoneyOrders;
            LkkPriceChf = lkkPriceChf;
            InMoneyVolumeLkk = inMoneyVolumeLkk;
            OutOfTheMoneyVolumeLkk = outOfTheMoneyVolumeLkk;
            _bids = bids;
        }

        public IOrderbookBid TryGetBid(string clientId)
        {
            _bids.TryGetValue(clientId, out OrderbookBid bid);

            return bid;
        }
    }
}