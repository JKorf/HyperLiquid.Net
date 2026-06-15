using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Clients;
using HyperLiquid.Net.Interfaces.Clients;
using HyperLiquid.Net.Objects.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Net.Http;

namespace HyperLiquid.Net.Clients
{
    /// <inheritdoc />
    public class HyperLiquidUserClientProvider : UserClientProvider<
        IHyperLiquidRestClient,
        IHyperLiquidSocketClient,
        HyperLiquidRestOptions,
        HyperLiquidSocketOptions,
        HyperLiquidCredentials,
        HyperLiquidEnvironment
        >, IHyperLiquidUserClientProvider
    {
        /// <inheritdoc />
        public override string ExchangeName => HyperLiquidExchange.ExchangeName;

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
            : base(httpClient, loggerFactory, restOptions, socketOptions)
        {
        }

        /// <inheritdoc />
        protected override IHyperLiquidRestClient ConstructRestClient(HttpClient client, ILoggerFactory? loggerFactory, IOptions<HyperLiquidRestOptions> options) 
            => new HyperLiquidRestClient(client, loggerFactory, options);
        /// <inheritdoc />
        protected override IHyperLiquidSocketClient ConstructSocketClient(ILoggerFactory? loggerFactory, IOptions<HyperLiquidSocketOptions> options)
            => new HyperLiquidSocketClient(options, loggerFactory);
    }
}
