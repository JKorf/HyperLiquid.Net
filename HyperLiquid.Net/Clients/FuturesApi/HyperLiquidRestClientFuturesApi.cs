using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using HyperLiquid.Net.Objects.Options;
using HyperLiquid.Net.Clients.BaseApi;
using HyperLiquid.Net.Interfaces.Clients.FuturesApi;
using HyperLiquid.Net.Interfaces.Clients;

namespace HyperLiquid.Net.Clients.FuturesApi
{
    /// <inheritdoc cref="IHyperLiquidRestClientFuturesApi" />
    internal partial class HyperLiquidRestClientFuturesApi : HyperLiquidRestClientApi, IHyperLiquidRestClientFuturesApi
    {
        #region fields 
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
        internal HyperLiquidRestClientFuturesApi(ILogger logger, IHyperLiquidRestClient baseClient, HttpClient? httpClient, HyperLiquidRestOptions options)
            : base(logger, baseClient, httpClient, options, options.FuturesOptions)
        {
            Account = new HyperLiquidRestClientFuturesApiAccount(this);
            ExchangeData = new HyperLiquidRestClientFuturesApiExchangeData(logger, this);
            Trading = new HyperLiquidRestClientFuturesApiTrading(logger, this);
        }
        #endregion

        /// <inheritdoc />
        protected override Task<WebCallResult<DateTime>> GetServerTimestampAsync() => throw new NotImplementedException();

        /// <inheritdoc />
        public IHyperLiquidRestClientFuturesApiShared SharedClient => this;

    }
}
