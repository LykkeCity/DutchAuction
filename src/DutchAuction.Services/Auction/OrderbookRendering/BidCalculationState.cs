namespace DutchAuction.Services.Auction.OrderbookRendering
{
    internal enum BidCalculationState
    {
        NotCalculatedYet,
        InMoney,
        OutOfTheMoney,
        PartiallyInMoney
    }
}