using CryptoExchange.Net.Objects;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using HyperLiquid.Net.Objects.Options;
using HyperLiquid.Net.Clients.BaseApi;
using HyperLiquid.Net.Interfaces.Clients.SpotApi;
using HyperLiquid.Net.Interfaces.Clients;

namespace HyperLiquid.Net.Clients.SpotApi
{
    /// <inheritdoc cref="IHyperLiquidRestClientSpotApi" />
    internal partial class HyperLiquidRestClientSpotApi : HyperLiquidRestClientApi, IHyperLiquidRestClientSpotApi
    {
        #region fields 
        internal new HyperLiquidRestOptions ClientOptions => (HyperLiquidRestOptions)base.ClientOptions;
        #endregion

        #region Api clients
        /// <inheritdoc />
        public IHyperLiquidRestClientSpotApiAccount Account { get; }
        /// <inheritdoc />
        public IHyperLiquidRestClientSpotApiExchangeData ExchangeData { get; }
        /// <inheritdoc />
        public IHyperLiquidRestClientSpotApiTrading Trading { get; }
        #endregion

        #region constructor/destructor
        internal HyperLiquidRestClientSpotApi(ILogger logger, IHyperLiquidRestClient baseClient, HttpClient? httpClient, HyperLiquidRestOptions options)
            : base(logger, baseClient, httpClient, options, options.SpotOptions)
        {
            Account = new HyperLiquidRestClientSpotApiAccount(this);
            ExchangeData = new HyperLiquidRestClientSpotApiExchangeData(logger, this);
            Trading = new HyperLiquidRestClientSpotApiTrading(logger, this);
        }
        #endregion

        /// <inheritdoc />
        protected override Task<WebCallResult<DateTime>> GetServerTimestampAsync() => throw new NotImplementedException();

        /// <inheritdoc />
        public IHyperLiquidRestClientSpotApiShared SharedClient => this;

    }
}
