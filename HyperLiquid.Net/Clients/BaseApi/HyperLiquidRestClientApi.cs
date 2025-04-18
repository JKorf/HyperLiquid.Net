using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HyperLiquid.Net.Objects.Options;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Converters.MessageParsing;
using HyperLiquid.Net.Objects.Models;
using CryptoExchange.Net.Objects.Options;
using HyperLiquid.Net.Interfaces.Clients;

namespace HyperLiquid.Net.Clients.BaseApi
{
    internal abstract partial class HyperLiquidRestClientApi : RestApiClient
    {
        /// <inheritdoc />
        public string ExchangeName => "HyperLiquid";

        public new HyperLiquidRestOptions ClientOptions => (HyperLiquidRestOptions)base.ClientOptions;
        internal IHyperLiquidRestClient BaseClient { get; }

        #region constructor/destructor
        internal HyperLiquidRestClientApi(ILogger logger, IHyperLiquidRestClient baseClient, HttpClient? httpClient, HyperLiquidRestOptions options, RestApiOptions apiOptions)
            : base(logger, httpClient, options.Environment.RestClientAddress, options, apiOptions)
        {
            BaseClient = baseClient;
        }
        #endregion

        /// <inheritdoc />
        protected override IStreamMessageAccessor CreateAccessor() => new SystemTextJsonStreamMessageAccessor(SerializerOptions.WithConverters(HyperLiquidExchange._serializerContext));
        /// <inheritdoc />
        protected override IMessageSerializer CreateSerializer() => new SystemTextJsonMessageSerializer(SerializerOptions.WithConverters(HyperLiquidExchange._serializerContext));

        /// <inheritdoc />
        protected override AuthenticationProvider CreateAuthenticationProvider(ApiCredentials credentials)
            => new HyperLiquidAuthenticationProvider(credentials);

        internal Task<WebCallResult> SendAsync(RequestDefinition definition, ParameterCollection? parameters, CancellationToken cancellationToken, int? weight = null)
            => SendToAddressAsync(BaseAddress, definition, parameters, cancellationToken, weight);

        internal async Task<WebCallResult> SendToAddressAsync(string baseAddress, RequestDefinition definition, ParameterCollection? parameters, CancellationToken cancellationToken, int? weight = null)
        {
            return await base.SendAsync(baseAddress, definition, parameters, cancellationToken, null, weight).ConfigureAwait(false);
        }

        internal Task<WebCallResult<T>> SendAsync<T>(RequestDefinition definition, ParameterCollection? parameters, CancellationToken cancellationToken, int? weight = null)
            => SendToAddressAsync<T>(BaseAddress, definition, parameters, cancellationToken, weight);

        internal async Task<WebCallResult<T>> SendToAddressAsync<T>(string baseAddress, RequestDefinition definition, ParameterCollection? parameters, CancellationToken cancellationToken, int? weight = null)
        {
            return await base.SendAsync<T>(baseAddress, definition, parameters, cancellationToken, null, weight).ConfigureAwait(false);
        }

        internal async Task<WebCallResult<T>> SendAuthAsync<T>(RequestDefinition definition, ParameterCollection? parameters, CancellationToken cancellationToken, int? weight = null)
        {
            var result = await SendToAddressAsync<HyperLiquidResponse<T>>(BaseAddress, definition, parameters, cancellationToken, weight).ConfigureAwait(false);
            if (!result)
                return result.As<T>(default);

            if (!result.Data.Status.Equals("ok"))
                return result.AsError<T>(new ServerError(result.Data.Status));

            return result.As(result.Data.Data!.Data);
        }

        protected override Error? TryParseError(KeyValuePair<string, string[]>[] responseHeaders, IMessageAccessor accessor)
        {
            var status = accessor.GetValue<string?>(MessagePath.Get().Property("status"));
            if (status == "err")
                return new ServerError(accessor.GetValue<string>(MessagePath.Get().Property("response"))!);

            return null;
        }

        /// <inheritdoc />
        protected override Task<WebCallResult<DateTime>> GetServerTimestampAsync() => throw new NotImplementedException();

        /// <inheritdoc />
        public override TimeSyncInfo? GetTimeSyncInfo() => null;

        /// <inheritdoc />
        public override string FormatSymbol(string baseAsset, string quoteAsset, TradingMode tradingMode, DateTime? deliverDate = null)
            => HyperLiquidExchange.FormatSymbol(baseAsset, quoteAsset, tradingMode, deliverDate);

    }
}
