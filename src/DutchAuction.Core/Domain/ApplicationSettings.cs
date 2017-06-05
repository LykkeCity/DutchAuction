namespace DutchAuction.Core.Domain
{
    public class ApplicationSettings
    {
        public DutchAuctionSettings DutchAuction { get; set; }

        public class DutchAuctionSettings
        {
            public DatabaseSettings Db { get; set; }
            public string[] Assets { get; set; }
        }

        public class DatabaseSettings
        {
            public string DataConnectionString { get; set; }
        }
    }
}