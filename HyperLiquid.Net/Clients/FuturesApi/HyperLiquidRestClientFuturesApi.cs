using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HyperLiquid.Net.Objects.Options;
using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.SharedApis;
using CryptoExchange.Net.Converters.MessageParsing;
using HyperLiquid.Net.Objects.Models;
using HyperLiquid.Net.Clients.BaseApi;
using HyperLiquid.Net.Interfaces.Clients.FuturesApi;

namespace HyperLiquid.Net.Clients.FuturesApi
{
    /// <inheritdoc cref="IHyperLiquidRestClientFuturesApi" />
    internal partial class HyperLiquidRestClientFuturesApi : HyperLiquidRestClientApi, IHyperLiquidRestClientFuturesApi
    {
        #region fields 
        internal static TimeSyncState _timeSyncState = new TimeSyncState("Futures Api");

        internal new HyperLiquidRestOptions ClientOptions => (HyperLiquidRestOptions)base.ClientOptions;
        #endregion

        #region Api clients
        /// <inheritdoc />
        public IHyperLiquidRestClientFuturesApiAccount Account { get; }
        /// <inheritdoc />
        public IHyperLiquidRestClientFuturesApiExchangeData ExchangeData { get; }
        /// <inheritdoc />
        public IHyperLiquidRestClientFuturesApiTrading Trading { get; }
        #endregion

        #region constructor/destructor
        internal HyperLiquidRestClientFuturesApi(ILogger logger, HttpClient? httpClient, HyperLiquidRestOptions options)
            : base(logger, httpClient, options, options.FuturesOptions)
        {
            Account = new HyperLiquidRestClientFuturesApiAccount(this);
            ExchangeData = new HyperLiquidRestClientFuturesApiExchangeData(logger, this);
            Trading = new HyperLiquidRestClientFuturesApiTrading(logger, this);
        }
        #endregion


        /// <inheritdoc />
        protected override Task<WebCallResult<DateTime>> GetServerTimestampAsync() => throw new NotImplementedException();

        /// <inheritdoc />
        public override TimeSyncInfo? GetTimeSyncInfo() => null;

        /// <inheritdoc />
        public override TimeSpan? GetTimeOffset() => _timeSyncState.TimeOffset;

        /// <inheritdoc />
        public IHyperLiquidRestClientFuturesApiShared SharedClient => this;

    }
}
