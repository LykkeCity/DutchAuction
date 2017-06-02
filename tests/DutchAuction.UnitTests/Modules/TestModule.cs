using Autofac;
using AzureStorage.Tables;
using DutchAuction.Core;
using DutchAuction.Repositories;
using DutchAuction.Repositories.Entities;
using DutchAuction.Services;

namespace DutchAuction.UnitTests.Modules
{
    public class TestModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
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
