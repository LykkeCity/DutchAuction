using Autofac;
using AzureStorage.Tables;
using DutchAuction.Core;
using DutchAuction.Repositories;
using DutchAuction.Services;

namespace DutchAuction.Api.Modules
{
    public class ApiModule : Module
    {
        private readonly ApplicationSettings _settings;

        public ApiModule(ApplicationSettings settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_settings)
                .SingleInstance();

            builder.Register<IAuctionLotRepository>(ctx =>
                new AuctionLotRepository(new NoSqlTableInMemory<AuctionLotEntity>())
            ).SingleInstance();

            builder.RegisterType<AuctionLotManager>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<AuctionLotCacheService>()
                .As<IAuctionLotCacheService>()
                .SingleInstance();
        }
    }
}
