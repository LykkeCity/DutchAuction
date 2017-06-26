using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureStorage.Tables;
using Common.Log;
using DutchAuction.Core;
using DutchAuction.Core.Domain.Auction;
using DutchAuction.Core.Services.Assets;
using DutchAuction.Core.Services.Auction;
using DutchAuction.Core.Services.MarketProfile;
using DutchAuction.Repositories.AuctionEvents;
using DutchAuction.Services.Assets;
using DutchAuction.Services.Auction;
using DutchAuction.Services.MarketProfile;
using Lykke.MarketProfileService.Client;
using Lykke.Service.Assets.Client.Custom;
using Microsoft.Extensions.DependencyInjection;

namespace DutchAuction.Api.DependencyInjection
{
    public class ApiModule : Module
    {
        private readonly ApplicationSettings.DutchAuctionSettings _settings;
        private readonly ILog _log;
        private IServiceCollection _services;

        public ApiModule(ApplicationSettings.DutchAuctionSettings settings, ILog log)
        {
            _settings = settings;
            _log = log;
            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_log).SingleInstance();

            builder.RegisterInstance(_settings).SingleInstance();

            RegisterAuction(builder);

            RegisterAssets(builder);

            RegisterMarketProfile(builder);

            builder.Populate(_services);
        }

        private void RegisterAuction(ContainerBuilder builder)
        {
            builder
                .Register<IAuctionEventsRepository>(
                    ctx => new AuctionEventsRepository(
                        new AzureTableStorage<AuctionEventEntity>(_settings.Db.DataConnectionString, "AuctionEvents", null)))
                .SingleInstance();

            builder.RegisterType<AuctionEventsManager>()
                .As<IAuctionEventsManager>()
                .As<IStartable>()
                .SingleInstance();

            builder.Register(x => new AuctionManager(
                    x.Resolve<IAuctionEventsManager>(),
                    x.Resolve<IBidsService>(),
                    x.Resolve<IOrderbookService>(),
                    x.Resolve<IPriceHistoryService>(),
                    _log,
                    _settings.OrderbookUpdatePeriod))
                .As<IAuctionManager>()
                .As<IStartable>()
                .SingleInstance();

            builder
                .RegisterType<BidsService>()
                .As<IBidsService>()
                .SingleInstance();

            builder.Register(x => new OrderbookService(
                    x.Resolve<IAssetExchangeService>(),
                    _settings.TotalAuctionVolume,
                    _settings.MinClosingBidCutoffVolume))
                .As<IOrderbookService>()
                .SingleInstance();

            builder.Register(x => new PriceHistoryService(_log, _settings.AuctionHistoryRabbitSettings))
                .As<IPriceHistoryService>()
                .As<IStartable>()
                .SingleInstance();
        }

        private void RegisterAssets(ContainerBuilder builder)
        {
            _services.UseAssetsClient(AssetServiceSettings.Create(new Uri(_settings.Dictionaries.AssetsServiceUrl), _settings.Dictionaries.CacheExpirationPeriod));

            builder.RegisterType<AssetPairsManager>().As<IAssetPairsManager>().SingleInstance();

            builder.RegisterType<AssetExchangeService>().As<IAssetExchangeService>();
        }

        private void RegisterMarketProfile(ContainerBuilder builder)
        {
            builder.Register(x => new MarketProfileManager(
                    new LykkeMarketProfileServiceAPI(new Uri(_settings.MarketProfile.ServiceUri)),
                    new MarketProfileCacheService(),
                    _settings.MarketProfile.CacheUpdatePeriod, _log))
                .As<IMarketProfileManager>()
                .As<IStartable>()
                .SingleInstance();
        }
    }
}
