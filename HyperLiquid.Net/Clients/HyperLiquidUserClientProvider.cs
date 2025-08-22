using HyperLiquid.Net.Interfaces.Clients;
using HyperLiquid.Net.Objects.Options;
using CryptoExchange.Net.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Collections.Generic;

namespace HyperLiquid.Net.Clients
{
    /// <inheritdoc />
    public class HyperLiquidUserClientProvider : IHyperLiquidUserClientProvider
    {
        private static ConcurrentDictionary<string, IHyperLiquidRestClient> _restClients = new ConcurrentDictionary<string, IHyperLiquidRestClient>();
        private static ConcurrentDictionary<string, IHyperLiquidSocketClient> _socketClients = new ConcurrentDictionary<string, IHyperLiquidSocketClient>();

        private readonly IOptions<HyperLiquidRestOptions> _restOptions;
        private readonly IOptions<HyperLiquidSocketOptions> _socketOptions;
        private readonly HttpClient _httpClient;
        private readonly ILoggerFactory? _loggerFactory;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="optionsDelegate">Options to use for created clients</param>
        public HyperLiquidUserClientProvider(Action<HyperLiquidOptions>? optionsDelegate = null)
            : this(null, null, Options.Create(ApplyOptionsDelegate(optionsDelegate).Rest), Options.Create(ApplyOptionsDelegate(optionsDelegate).Socket))
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        public HyperLiquidUserClientProvider(
            HttpClient? httpClient,
            ILoggerFactory? loggerFactory,
            IOptions<HyperLiquidRestOptions> restOptions,
            IOptions<HyperLiquidSocketOptions> socketOptions)
        {
            _httpClient = httpClient ?? new HttpClient();
            _loggerFactory = loggerFactory;
            _restOptions = restOptions;
            _socketOptions = socketOptions;
        }

        /// <inheritdoc />
        public void InitializeUserClient(string userIdentifier, ApiCredentials credentials, HyperLiquidEnvironment? environment = null)
        {
            CreateRestClient(userIdentifier, credentials, environment);
            CreateSocketClient(userIdentifier, credentials, environment);
        }

        /// <inheritdoc />
        public void ClearUserClients(string userIdentifier)
        {
            _restClients.TryRemove(userIdentifier, out _);
            _socketClients.TryRemove(userIdentifier, out _);
        }

        /// <inheritdoc />
        public IHyperLiquidRestClient GetRestClient(string userIdentifier, ApiCredentials? credentials = null, HyperLiquidEnvironment? environment = null)
        {
            if (!_restClients.TryGetValue(userIdentifier, out var client))
                client = CreateRestClient(userIdentifier, credentials, environment);

            return client;
        }

        /// <inheritdoc />
        public IHyperLiquidSocketClient GetSocketClient(string userIdentifier, ApiCredentials? credentials = null, HyperLiquidEnvironment? environment = null)
        {
            if (!_socketClients.TryGetValue(userIdentifier, out var client))
                client = CreateSocketClient(userIdentifier, credentials, environment);

            return client;
        }

        private IHyperLiquidRestClient CreateRestClient(string userIdentifier, ApiCredentials? credentials, HyperLiquidEnvironment? environment)
        {
            var clientRestOptions = SetRestEnvironment(environment);
            var client = new HyperLiquidRestClient(_httpClient, _loggerFactory, clientRestOptions);
            if (credentials != null)
            {
                client.SetApiCredentials(credentials);
                _restClients.TryAdd(userIdentifier, client);
            }
            return client;
        }

        private IHyperLiquidSocketClient CreateSocketClient(string userIdentifier, ApiCredentials? credentials, HyperLiquidEnvironment? environment)
        {
            var clientSocketOptions = SetSocketEnvironment(environment);
            var client = new HyperLiquidSocketClient(clientSocketOptions!, _loggerFactory);
            if (credentials != null)
            {
                client.SetApiCredentials(credentials);
                _socketClients.TryAdd(userIdentifier, client);
            }
            return client;
        }

        private IOptions<HyperLiquidRestOptions> SetRestEnvironment(HyperLiquidEnvironment? environment)
        {
            if (environment == null)
                return _restOptions;

            var newRestClientOptions = new HyperLiquidRestOptions();
            var restOptions = _restOptions.Value.Set(newRestClientOptions);
            newRestClientOptions.Environment = environment;
            return Options.Create(newRestClientOptions);
        }

        private IOptions<HyperLiquidSocketOptions> SetSocketEnvironment(HyperLiquidEnvironment? environment)
        {
            if (environment == null)
                return _socketOptions;

            var newSocketClientOptions = new HyperLiquidSocketOptions();
            var restOptions = _socketOptions.Value.Set(newSocketClientOptions);
            newSocketClientOptions.Environment = environment;
            return Options.Create(newSocketClientOptions);
        }

        private static T ApplyOptionsDelegate<T>(Action<T>? del) where T : new()
        {
            var opts = new T();
            del?.Invoke(opts);
            return opts;
        }
    }
}
