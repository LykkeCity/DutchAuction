using Autofac;
using AzureStorage.Tables;
using DutchAuction.Core;
using DutchAuction.Core.Domain;
using DutchAuction.Core.Services.Assets;
using DutchAuction.Core.Services.Lots;
using DutchAuction.Core.Services.MarketProfile;
using DutchAuction.Repositories;
using DutchAuction.Repositories.Entities;
using DutchAuction.Services.Assets;
using DutchAuction.Services.Lots;
using DutchAuction.Services.MarketProfile;
using Lykke.MarketProfileService.Client;

namespace DutchAuction.Api.Modules
{
    public class ApiModule : Module
    {
        private readonly ApplicationSettings.DutchAuctionSettings _settings;

        public ApiModule(ApplicationSettings.DutchAuctionSettings settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_settings)
                .SingleInstance();

            builder
                .Register<IAuctionLotRepository>(
                    ctx => new AuctionLotRepository(new NoSqlTableInMemory<AuctionLotEntity>()))
                .SingleInstance();

            builder.RegisterType<AuctionLotManager>()
                .As<IAuctionLotManager>()
                .As<IStartable>()
                .SingleInstance();

            builder.RegisterType<AuctionLotCacheService>()
                .As<IAuctionLotCacheService>()
                .SingleInstance();

            builder.Register(x => new MarketProfileManager(
                    new LykkeMarketProfileServiceAPI(_settings.MarketProfile.ServiceUri), 
                    new MarketProfileCacheService(),
                    _settings.MarketProfile.CacheUpdatePeriod))
                .As<IMarketProfileManager>()
                .As<IStartable>()
                .SingleInstance();

            builder.RegisterType<AssetExchangeService>().As<IAssetExchangeService>();
        }
    }
}
