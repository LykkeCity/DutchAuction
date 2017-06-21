using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureStorage.Tables;
using Common.Log;
using DutchAuction.Api.DependencyInjection;
using DutchAuction.Api.Models;
using DutchAuction.Core;
using Lykke.AzureQueueIntegration;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.Logs;
using Lykke.SettingsReader;
using Lykke.SlackNotification.AzureQueue;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Converters;

namespace DutchAuction.Api
{
    public class Startup
    {
        public IHostingEnvironment HostingEnvironment { get; }
        public IContainer ApplicationContainer { get; set; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            HostingEnvironment = env;
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            ILog log = new LogToConsole();

            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                });

            services.AddSwaggerGen(options =>
            {
                options.DefaultLykkeConfiguration("v1", "Dutch Auction API");
            });

            var settings = HttpSettingsLoader.Load<ApplicationSettings>();
            var appSettings = settings.DutchAuction;

            var slackService = services.UseSlackNotificationsSenderViaAzureQueue(new AzureQueueSettings
            {
                ConnectionString = settings.SlackNotifications.AzureQueue.ConnectionString,
                QueueName = settings.SlackNotifications.AzureQueue.QueueName
            }, log);

            if (!string.IsNullOrEmpty(appSettings.Db.LogsConnectionString) &&
                !(appSettings.Db.LogsConnectionString.StartsWith("${") && appSettings.Db.LogsConnectionString.EndsWith("}")))
            {
                log = new LykkeLogToAzureStorage("Lykke.DutchAuction", new AzureTableStorage<LogEntity>(
                    appSettings.Db.LogsConnectionString, "DutchAuctionLogs", log), slackService);
            }

            var builder = new ContainerBuilder();
            
            builder.RegisterModule(new ApiModule(appSettings, log));
            builder.Populate(services);

            ApplicationContainer = builder.Build();

            return new AutofacServiceProvider(ApplicationContainer);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseLykkeMiddleware(Constants.ComponentName, () => ErrorResponse.Create("Technical problems"));

            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUi();
        }
    }
}
