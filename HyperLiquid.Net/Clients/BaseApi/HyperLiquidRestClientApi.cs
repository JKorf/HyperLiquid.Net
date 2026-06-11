using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HyperLiquid.Net.Objects.Options;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.SharedApis;
using HyperLiquid.Net.Objects.Models;
using CryptoExchange.Net.Objects.Options;
using HyperLiquid.Net.Interfaces.Clients;
using CryptoExchange.Net.Objects.Errors;
using CryptoExchange.Net.Converters.MessageParsing.DynamicConverters;
using HyperLiquid.Net.Clients.MessageHandlers;
using CryptoExchange.Net.Authentication;

namespace HyperLiquid.Net.Clients.BaseApi
{
    internal abstract partial class HyperLiquidRestClientApi : RestApiClient<HyperLiquidEnvironment, HyperLiquidAuthenticationProvider, HyperLiquidCredentials>
    {
        public new HyperLiquidRestOptions ClientOptions => (HyperLiquidRestOptions)base.ClientOptions;
        internal HyperLiquidRestClient BaseClient { get; }

        protected override ErrorMapping ErrorMapping => HyperLiquidErrors.Errors;
        protected override IRestMessageHandler MessageHandler { get; } = new HyperLiquidRestMessageHandler();

        #region constructor/destructor
        internal HyperLiquidRestClientApi(
            ILogger logger, 
            HyperLiquidRestClient baseClient,
            HttpClient? httpClient,
            HyperLiquidRestOptions options,
            RestApiOptions apiOptions)
            : base(logger, HyperLiquidExchange.Metadata.Id, httpClient, options.Environment.RestClientAddress, options, apiOptions)
        {
            BaseClient = baseClient;
        }
        #endregion

        /// <inheritdoc />
        protected override IMessageSerializer CreateSerializer() => new SystemTextJsonMessageSerializer(HyperLiquidExchange._serializerContext);


        /// <inheritdoc />
        protected override HyperLiquidAuthenticationProvider CreateAuthenticationProvider(HyperLiquidCredentials credentials)
            => new HyperLiquidAuthenticationProvider(credentials);

        internal async Task<HttpResult> SendAsync(RequestDefinition definition, Parameters? parameters, CancellationToken cancellationToken, int? weight = null)
        {
            return await base.SendAsync<Unit>(definition, parameters, cancellationToken, null, weight).ConfigureAwait(false);
        }

        internal async Task<HttpResult<T>> SendAsync<T>(RequestDefinition definition, Parameters? parameters, CancellationToken cancellationToken, int? weight = null)
        {
            return await base.SendAsync<T>(definition, parameters, cancellationToken, null, weight).ConfigureAwait(false);
        }

        internal async Task<HttpResult<T>> SendAuthAsync<T>(RequestDefinition definition, Parameters? parameters, CancellationToken cancellationToken, int? weight = null)
        {
            var result = await SendAsync<HyperLiquidResponse<T>>(definition, parameters, cancellationToken, weight).ConfigureAwait(false);
            if (!result.Success)
                return HttpResult.Fail<T>(result);

            if (!result.Data.Status.Equals("ok"))
                return HttpResult.Fail<T>(result, new ServerError(ErrorInfo.Unknown with { Message = result.Data.Status }));

            return HttpResult.Ok(result, result.Data.Data!.Data);
        }

        internal void AddExpiresAfter(Parameters parameters, DateTime? requestExpiresAfter)
        {
            if (requestExpiresAfter != null)
                parameters.Add("expiresAfter", DateTimeConverter.ConvertToMilliseconds(requestExpiresAfter));
            else if (ClientOptions.ExpiresAfter != null)
                parameters.Add("expiresAfter", DateTimeConverter.ConvertToMilliseconds(DateTime.UtcNow + ClientOptions.ExpiresAfter));
        }

        /// <inheritdoc />
        protected override Task<HttpResult<DateTime>> GetServerTimestampAsync() => throw new NotImplementedException();

        /// <inheritdoc />
        public override string FormatSymbol(string baseAsset, string quoteAsset, TradingMode tradingMode, DateTime? deliverDate = null)
            => HyperLiquidExchange.FormatSymbol(baseAsset, quoteAsset, tradingMode, deliverDate);

    }
}
