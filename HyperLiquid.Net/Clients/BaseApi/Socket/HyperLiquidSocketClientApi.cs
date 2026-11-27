using CryptoExchange.Net;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Converters.MessageParsing.DynamicConverters;
using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Errors;
using CryptoExchange.Net.Objects.Options;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Sockets;
using CryptoExchange.Net.Sockets.Default;
using CryptoExchange.Net.Sockets.Interfaces;
using HyperLiquid.Net.Clients.MessageHandlers;
using HyperLiquid.Net.Interfaces.Clients;
using HyperLiquid.Net.Objects.Options;
using HyperLiquid.Net.Objects.Sockets;
using HyperLiquid.Net.Objects.Sockets.Subscriptions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace HyperLiquid.Net.Clients.BaseApi
{
    /// <summary>
    /// Client providing access to the HyperLiquid  websocket Api
    /// </summary>
    internal abstract class HyperLiquidSocketClientApi : SocketApiClient<HyperLiquidEnvironment, HyperLiquidAuthenticationProvider, HyperLiquidCredentials>
    {
        #region fields
        protected override ErrorMapping ErrorMapping => HyperLiquidErrors.Errors;
        internal new HyperLiquidSocketOptions ClientOptions => (HyperLiquidSocketOptions)base.ClientOptions;
        internal HyperLiquidSocketClient BaseClient { get; }

        #endregion

        #region constructor/destructor

        /// <summary>
        /// ctor
        /// </summary>
        internal HyperLiquidSocketClientApi(
            ILoggerFactory? loggerFactory,
            HyperLiquidSocketClient baseClient,
            HyperLiquidSocketOptions options,
            SocketApiOptions apiOptions) :
            base(loggerFactory, HyperLiquidExchange.Metadata.Id, options.Environment.SocketClientAddress!, options, apiOptions)
        {
            BaseClient = baseClient;

            RateLimiter = HyperLiquidExchange.RateLimiter.HyperLiquidSocket;
            // To prevent multiple connections being created when first retrieving some public data for an authenticated request
            UsePublicConnectionForAuth = true;

            RegisterPeriodicQuery(
                "Ping",
                TimeSpan.FromSeconds(30),
                x => new HyperLiquidPingQuery(),
                (connection, result) =>
                {
                    if (result.Error?.ErrorType == ErrorType.Timeout)
                    {
                        // Ping timeout, reconnect
                        _logger.LogWarning("[Sckt {SocketId}] Ping response timeout, reconnecting", connection.SocketId);
                        _ = connection.TriggerReconnectAsync();
                    }
                });
        }
        #endregion

        /// <inheritdoc />
        protected override IMessageSerializer CreateSerializer() => new SystemTextJsonMessageSerializer(HyperLiquidExchange._serializerContext);
        /// <inheritdoc />
        public override ISocketMessageHandler CreateMessageConverter(WebSocketMessageType messageType) => new HyperLiquidSocketMessageHandler();

        /// <inheritdoc />
        protected override HyperLiquidAuthenticationProvider CreateAuthenticationProvider(HyperLiquidCredentials credentials)
            => new HyperLiquidAuthenticationProvider(credentials);

        internal Task<WebSocketResult<UpdateSubscription>> SubscribeInternalAsync(Subscription subscription, CancellationToken ct)
            => base.SubscribeAsync(BaseAddress.AppendPath("ws"), subscription, ct);

        internal Task<QueryResult<T>> QueryInternalAsync<T>(Query<T> query, CancellationToken ct)
            => base.QueryAsync<T>(BaseAddress.AppendPath("ws"), query, ct);

        internal void AddExpiresAfter(Parameters parameters, DateTime? requestExpiresAfter)
        {
            if (requestExpiresAfter != null)
                parameters.Add("expiresAfter", DateTimeConverter.ConvertToMilliseconds(requestExpiresAfter));
            else if (ClientOptions.ExpiresAfter != null)
                parameters.Add("expiresAfter", DateTimeConverter.ConvertToMilliseconds(DateTime.UtcNow + ClientOptions.ExpiresAfter));
        }

        internal void ValidateAddress(string? address)
        {
            if (address != null && (!address.StartsWith("0x") || address.Length != 42))
                throw new ArgumentException("Address should be in 42-character hexadecimal format; e.g. 0x0000000000000000000000000000000000000000");
        }

        /// <inheritdoc />
        public override string FormatSymbol(string baseAsset, string quoteAsset, TradingMode tradingMode, DateTime? deliverDate = null)
            => HyperLiquidExchange.FormatSymbol(baseAsset, quoteAsset, tradingMode, deliverDate);

        /// <inheritdoc />
        protected override bool ConnectionCanBeUsedFor(SocketConnection connection, string address, bool authenticated, IMessageProcessor processor, string? topic = null)
        {
            // Override to allow different API sub clients to use the same connection
            if (!connection.ConnectionUriString.Equals(address.TrimEnd('/'), StringComparison.Ordinal))
                return false;

            // Check if this subscription is for a different user address than the other subscriptions on this connection, if so don't use the existing connection
            if (processor is IHyperLiquidSubscription subscription && subscription.UserAddress != null)
            {
                var thisSubAddress = subscription.UserAddress;
                if (connection.Subscriptions.Any(x =>
                    x is IHyperLiquidSubscription sub
                    && !string.Equals(sub.UserAddress, thisSubAddress, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
