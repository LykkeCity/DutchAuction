using Autofac;
using DutchAuction.UnitTests.Modules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DutchAuction.UnitTests
{
    [TestClass]
    public class BaseTests
    {
        protected static IContainer Container { get; private set; }

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new TestModule());

            Container = builder.Build();
        }
    }
}
