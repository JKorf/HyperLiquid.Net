using CryptoExchange.Net;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Interfaces.Clients;
using CryptoExchange.Net.SharedApis;
using HyperLiquid.Net;
using HyperLiquid.Net.Clients;
using HyperLiquid.Net.Interfaces;
using HyperLiquid.Net.Interfaces.Clients;
using HyperLiquid.Net.Objects.Options;
using HyperLiquid.Net.SymbolOrderBooks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions for DI
    /// </summary>
    public static class ServiceCollectionExtensions
    {

        /// <summary>
        /// Add services such as the IHyperLiquidRestClient and IHyperLiquidSocketClient. Configures the services based on the provided configuration.<br />
        /// See <see href="https://github.com/JKorf/HyperLiquid.Net/blob/main/Examples/example-config.json" /> for an example of how to set up the configuration.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration(section) containing the options</param>
        /// <returns></returns>
        public static IServiceCollection AddHyperLiquid(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var options = HyperLiquidOptions.CreateFromConfiguration(configuration);
            services.AddSingleton(Options.Options.Create(options.Rest));
            services.AddSingleton(Options.Options.Create(options.Socket));
            services.AddSingleton(Options.Options.Create(options));

            return AddHyperLiquidCore(services, options.SocketClientLifeTime);
        }

        /// <summary>
        /// Add services such as the IHyperLiquidRestClient and IHyperLiquidSocketClient. Services will be configured based on the provided options.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="optionsDelegate">Set options for the HyperLiquid services</param>
        /// <returns></returns>
        public static IServiceCollection AddHyperLiquid(
            this IServiceCollection services,
            Action<HyperLiquidOptions>? optionsDelegate = null)
        {
            var options = HyperLiquidOptions.Create(optionsDelegate);
            services.AddSingleton(Options.Options.Create(options.Rest));
            services.AddSingleton(Options.Options.Create(options.Socket));
            services.AddSingleton(Options.Options.Create(options));

            return AddHyperLiquidCore(services, options.SocketClientLifeTime);
        }

        private static IServiceCollection AddHyperLiquidCore(
            this IServiceCollection services,
            ServiceLifetime? socketClientLifeTime = null)
        {
            services.AddHttpClient<IHyperLiquidRestClient, HyperLiquidRestClient>((client, serviceProvider) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<HyperLiquidRestOptions>>().Value;
                client.Timeout = options.RequestTimeout;
                return new HyperLiquidRestClient(client, serviceProvider.GetRequiredService<ILoggerFactory>(), serviceProvider.GetRequiredService<IOptions<HyperLiquidRestOptions>>());
            }).ConfigurePrimaryHttpMessageHandler((serviceProvider) => {
                var options = serviceProvider.GetRequiredService<IOptions<HyperLiquidRestOptions>>().Value;
                return LibraryHelpers.CreateHttpClientMessageHandler(options);
            }).SetHandlerLifetime(Timeout.InfiniteTimeSpan);
            services.Add(new ServiceDescriptor(typeof(IHyperLiquidSocketClient), x => { return new HyperLiquidSocketClient(x.GetRequiredService<IOptions<HyperLiquidSocketOptions>>(), x.GetRequiredService<ILoggerFactory>()); }, socketClientLifeTime ?? ServiceLifetime.Singleton));

            services.AddTransient<IHyperLiquidOrderBookFactory, HyperLiquidOrderBookFactory>();
            services.AddTransient<IHyperLiquidTrackerFactory, HyperLiquidTrackerFactory>();
            services.AddTransient<ITrackerFactory, HyperLiquidTrackerFactory>();
            services.AddSingleton<IHyperLiquidUserClientProvider, HyperLiquidUserClientProvider>(x =>
            new HyperLiquidUserClientProvider(
                x.GetRequiredService<IHttpClientFactory>().CreateClient(typeof(IHyperLiquidRestClient).Name),
                x.GetRequiredService<ILoggerFactory>(),
                x.GetRequiredService<IOptions<HyperLiquidRestOptions>>(),
                x.GetRequiredService<IOptions<HyperLiquidSocketOptions>>()));

            services.RegisterSharedRestInterfaces(x => x.GetRequiredService<IHyperLiquidRestClient>().SpotApi.SharedClient);
            services.RegisterSharedRestInterfaces(x => x.GetRequiredService<IHyperLiquidRestClient>().FuturesApi.SharedClient);
            services.RegisterSharedSocketInterfaces(x => x.GetRequiredService<IHyperLiquidSocketClient>().SpotApi.SharedClient);
            services.RegisterSharedSocketInterfaces(x => x.GetRequiredService<IHyperLiquidSocketClient>().FuturesApi.SharedClient);

            services.RegisterSharedApiClient<
                IHyperLiquidSharedApiClient,
                HyperLiquidSharedApiClient>(sharedApis => sharedApis
                    .Add(client => client.SpotRest)
                    .Add(client => client.SpotSocket)
                    .Add(client => client.FuturesRest)
                    .Add(client => client.FuturesSocket)
                    );

            return services;
        }
    }
}
