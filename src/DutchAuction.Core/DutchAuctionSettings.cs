namespace DutchAuction.Core
{
    public class DutchAuctionSettings
    {
        public ApplicationSettings DutchAuction { get; set; }
    }

    public class ApplicationSettings
    {
        public string[] Assets { get; set; }
    }
}
