using System;
using Autofac;
using AzureStorage.Tables;
using Common.Log;
using DutchAuction.Core;
using DutchAuction.Core.Domain.Asset;
using DutchAuction.Core.Domain.Auction;
using DutchAuction.Core.Services.Assets;
using DutchAuction.Core.Services.Auction;
using DutchAuction.Core.Services.MarketProfile;
using DutchAuction.Repositories.Assets;
using DutchAuction.Repositories.Lots;
using DutchAuction.Services.Assets;
using DutchAuction.Services.Auction;
using DutchAuction.Services.MarketProfile;
using Lykke.MarketProfileService.Client;

namespace DutchAuction.Api.DependencyInjection
{
    public class ApiModule : Module
    {
        private readonly ApplicationSettings.DutchAuctionSettings _settings;
        private readonly ILog _log;

        public ApiModule(ApplicationSettings.DutchAuctionSettings settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_log).SingleInstance();

            builder.RegisterInstance(_settings).SingleInstance();

            builder
                .Register<IBidsRepository>(
                    ctx => new BidsRepository(
                        new AzureTableStorage<BidEntity>(_settings.Db.DataConnectionString, "Bids", null)))
                .SingleInstance();

            builder
                .Register<IAssetPairsRepository>(
                    ctx => new AssetPairsRepository(
                        new AzureTableStorage<AssetPairEntity>(_settings.Db.DictionariesConnectionString,
                            "Dictionaries", null)))
                .SingleInstance();

            builder.RegisterType<BidsManager>()
                .As<IBidsManager>()
                .As<IStartable>()
                .SingleInstance();

            builder.Register(x => new MarketProfileManager(
                    new LykkeMarketProfileServiceAPI(new Uri(_settings.MarketProfile.ServiceUri)),
                    new MarketProfileCacheService(),
                    _settings.MarketProfile.CacheUpdatePeriod, _log))
                .As<IMarketProfileManager>()
                .As<IStartable>()
                .SingleInstance();

            builder.Register(x => new AssetPairsManager(
                    x.Resolve<IAssetPairsRepository>(),
                    new AssetPairsCacheService(),
                    _settings.Dictionaries.CacheUpdatePeriod, _log))
                .As<IAssetPairsManager>()
                .As<IStartable>()
                .SingleInstance();

            builder.RegisterType<AuctionManager>()
                .As<IAuctionManager>()
                .As<IStartable>()
                .SingleInstance();

            builder.RegisterType<AssetExchangeService>().As<IAssetExchangeService>();

            builder
                .RegisterType<ClientAccountsService>()
                .As<IClientAccountsService>()
                .SingleInstance();

            builder
                .RegisterType<OrderbookService>()
                .As<IOrderbookService>()
                .SingleInstance();
        }
    }
}
