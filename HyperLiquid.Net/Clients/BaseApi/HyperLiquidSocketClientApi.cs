using CryptoExchange.Net;
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
using HyperLiquid.Net.Clients.MessageHandlers;
using HyperLiquid.Net.Interfaces.Clients;
using HyperLiquid.Net.Objects.Options;
using HyperLiquid.Net.Objects.Sockets;
using Microsoft.Extensions.Logging;
using System;
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
            ILogger logger,
            HyperLiquidSocketClient baseClient,
            HyperLiquidSocketOptions options,
            SocketApiOptions<HyperLiquidCredentials> apiOptions) :
            base(logger, options.Environment.SocketClientAddress!, options, apiOptions)
        {
            BaseClient = baseClient;

            RateLimiter = HyperLiquidExchange.RateLimiter.HyperLiquidSocket;

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

        internal Task<CallResult<UpdateSubscription>> SubscribeInternalAsync(Subscription subscription, CancellationToken ct)
            => base.SubscribeAsync(BaseAddress.AppendPath("ws"), subscription, ct);

        internal Task<CallResult<T>> QueryInternalAsync<T>(Query<T> query, CancellationToken ct)
            => base.QueryAsync<T>(BaseAddress.AppendPath("ws"), query, ct);

        internal void AddExpiresAfter(ParameterCollection parameters, DateTime? requestExpiresAfter)
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
    }
}
