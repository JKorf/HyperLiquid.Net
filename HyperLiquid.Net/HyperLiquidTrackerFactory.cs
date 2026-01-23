using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Trackers.Klines;
using CryptoExchange.Net.Trackers.Trades;
using CryptoExchange.Net.Trackers.UserData;
using HyperLiquid.Net.Clients;
using HyperLiquid.Net.Interfaces;
using HyperLiquid.Net.Interfaces.Clients;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Net;

namespace HyperLiquid.Net
{
    /// <inheritdoc />
    public class HyperLiquidTrackerFactory : IHyperLiquidTrackerFactory
    {
        private readonly IServiceProvider? _serviceProvider;

        /// <summary>
        /// ctor
        /// </summary>
        public HyperLiquidTrackerFactory()
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="serviceProvider">Service provider for resolving logging and clients</param>
        public HyperLiquidTrackerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public bool CanCreateKlineTracker(SharedSymbol symbol, SharedKlineInterval interval)
        {
            var client = (_serviceProvider?.GetRequiredService<IHyperLiquidSocketClient>() ?? new HyperLiquidSocketClient());
            SubscribeKlineOptions klineOptions = symbol.TradingMode == TradingMode.Spot ? client.SpotApi.SharedClient.SubscribeKlineOptions : client.FuturesApi.SharedClient.SubscribeKlineOptions;
            return klineOptions.IsSupported(interval);
        }

        /// <inheritdoc />
        public bool CanCreateTradeTracker(SharedSymbol symbol) => true;

        /// <inheritdoc />
        public IKlineTracker CreateKlineTracker(SharedSymbol symbol, SharedKlineInterval interval, int? limit = null, TimeSpan? period = null)
        {
            var restClient = _serviceProvider?.GetRequiredService<IHyperLiquidRestClient>() ?? new HyperLiquidRestClient();
            var socketClient = _serviceProvider?.GetRequiredService<IHyperLiquidSocketClient>() ?? new HyperLiquidSocketClient();

            IKlineRestClient sharedRestClient;
            IKlineSocketClient sharedSocketClient;
            if (symbol.TradingMode == TradingMode.Spot)
            {
                sharedRestClient = restClient.SpotApi.SharedClient;
                sharedSocketClient = socketClient.SpotApi.SharedClient;
            }
            else
            {
                sharedRestClient = restClient.FuturesApi.SharedClient;
                sharedSocketClient = socketClient.FuturesApi.SharedClient;
            }

            return new KlineTracker(
                _serviceProvider?.GetRequiredService<ILoggerFactory>().CreateLogger(restClient.Exchange),
                sharedRestClient,
                sharedSocketClient,
                symbol,
                interval,
                limit,
                period
                );
        }
        /// <inheritdoc />
        public ITradeTracker CreateTradeTracker(SharedSymbol symbol, int? limit = null, TimeSpan? period = null)
        {
            var restClient = _serviceProvider?.GetRequiredService<IHyperLiquidRestClient>() ?? new HyperLiquidRestClient();
            var socketClient = _serviceProvider?.GetRequiredService<IHyperLiquidSocketClient>() ?? new HyperLiquidSocketClient();

            ITradeSocketClient sharedSocketClient;
            if (symbol.TradingMode == TradingMode.Spot)            
                sharedSocketClient = socketClient.SpotApi.SharedClient;            
            else            
                sharedSocketClient = socketClient.FuturesApi.SharedClient;
            
            return new TradeTracker(
                _serviceProvider?.GetRequiredService<ILoggerFactory>().CreateLogger(restClient.Exchange),
                null,
                null,
                sharedSocketClient,
                symbol,
                limit,
                period
                );
        }

        public IUserDataTracker CreateUserDataTracker(UserDataTrackerConfig config)
        {
            var restClient = _serviceProvider?.GetRequiredService<IHyperLiquidRestClient>() ?? new HyperLiquidRestClient();
            var socketClient = _serviceProvider?.GetRequiredService<IHyperLiquidSocketClient>() ?? new HyperLiquidSocketClient();
            return new HyperLiquidUserDataTracker(
                _serviceProvider?.GetRequiredService<ILogger<HyperLiquidUserDataTracker>>() ?? new NullLogger<HyperLiquidUserDataTracker>(),
                restClient,
                socketClient,
                null,
                config
                );
        }

        /// <inheritdoc />
        public IUserDataTracker CreateUserDataTracker(string userIdentifier, UserDataTrackerConfig config, ApiCredentials credentials, HyperLiquidEnvironment? environment = null)
        {
            var clientProvider = _serviceProvider?.GetRequiredService<IHyperLiquidUserClientProvider>() ?? new HyperLiquidUserClientProvider();
            var restClient = clientProvider.GetRestClient(userIdentifier, credentials, environment);
            var socketClient = clientProvider.GetSocketClient(userIdentifier, credentials, environment);
            return new HyperLiquidUserDataTracker(
                _serviceProvider?.GetRequiredService<ILogger<HyperLiquidUserDataTracker>>() ?? new NullLogger<HyperLiquidUserDataTracker>(),
                restClient,
                socketClient,
                userIdentifier,
                config
                );
        }
    }
}
