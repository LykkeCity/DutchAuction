using Autofac;
using DutchAuction.UnitTests.Modules;

namespace DutchAuction.UnitTests
{
    public class BaseTests
    {
        protected IContainer Container { get; set; }

        protected void RegisterDependencies()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new TestModule());
        }
    }
}
