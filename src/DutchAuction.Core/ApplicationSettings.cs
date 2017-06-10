using System;

namespace DutchAuction.Core
{
    public class ApplicationSettings
    {
        public DutchAuctionSettings DutchAuction { get; set; }

        public class DutchAuctionSettings
        {
            public DatabaseSettings Db { get; set; }
            public string[] Assets { get; set; }
            public MarketProfileSettings MarketProfile { get; set; }
            public DictionariesSettings Dictionaries { get; set; }
        }

        public class DatabaseSettings
        {
            public string DataConnectionString { get; set; }
            public string DictionariesConnectionString { get; set; }
        }

        public class MarketProfileSettings
        {
            public Uri ServiceUri { get; set; }
            public TimeSpan CacheUpdatePeriod { get; set; }
        }

        public class DictionariesSettings
        {
            public TimeSpan CacheUpdatePeriod { get; set; }
        }
    }
}