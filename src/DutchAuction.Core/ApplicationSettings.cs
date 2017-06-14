using System;

namespace DutchAuction.Core
{
    public class ApplicationSettings
    {
        public DutchAuctionSettings DutchAuction { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }

        public class DutchAuctionSettings
        {
            public DatabaseSettings Db { get; set; }
            public string[] Assets { get; set; }
            public MarketProfileSettings MarketProfile { get; set; }
            public DictionariesSettings Dictionaries { get; set; }
            public double TotalAuctionVolume { get; set; }
            public double MinClosingBidCutoffVolume { get; set; }
            public TimeSpan OrderbookUpdatePeriod { get; set; }
            public RabbitSettings AuctionHistoryRabbitSettings { get; set; }
        }

        public class DatabaseSettings
        {
            public string DataConnectionString { get; set; }
            public string DictionariesConnectionString { get; set; }
            public string LogsConnectionString { get; set; }
        }

        public class MarketProfileSettings
        {
            public string ServiceUri { get; set; }
            public TimeSpan CacheUpdatePeriod { get; set; }
        }

        public class DictionariesSettings
        {
            public TimeSpan CacheUpdatePeriod { get; set; }
        }

        public class SlackNotificationsSettings
        {
            public AzureQueueSettings AzureQueue { get; set; }

            public int ThrottlingLimitSeconds { get; set; }
        }

        public class RabbitSettings
        {
            public string ConnectionString { get; set; }
            public string ExchangeName { get; set; }
        }

        public class AzureQueueSettings
        {
            public string ConnectionString { get; set; }

            public string QueueName { get; set; }
        }
    }
}