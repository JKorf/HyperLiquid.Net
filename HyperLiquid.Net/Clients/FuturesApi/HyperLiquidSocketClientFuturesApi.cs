using System;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using Microsoft.Extensions.Logging;
using HyperLiquid.Net.Objects.Options;
using HyperLiquid.Net.Clients.BaseApi;
using HyperLiquid.Net.Interfaces.Clients.FuturesApi;

namespace HyperLiquid.Net.Clients.FuturesApi
{
    /// <summary>
    /// Client providing access to the HyperLiquid  websocket Api
    /// </summary>
    internal partial class HyperLiquidSocketClientFuturesApi : HyperLiquidSocketClientApi, IHyperLiquidSocketClientFuturesApi
    {
        public IHyperLiquidSocketClientFuturesApiAccount Account { get; }
        public IHyperLiquidSocketClientFuturesApiExchangeData ExchangeData { get; }
        public IHyperLiquidSocketClientFuturesApiTrading Trading { get; }

        #region constructor/destructor

        /// <summary>
        /// ctor
        /// </summary>
        internal HyperLiquidSocketClientFuturesApi(ILogger logger, HyperLiquidSocketClient baseClient, HyperLiquidSocketOptions options) :
            base(logger, baseClient, options, options.FuturesOptions)
        {

            Account = new HyperLiquidSocketClientFuturesApiAccount(logger, this);
            ExchangeData = new HyperLiquidSocketClientFuturesApiExchangeData(logger, this);
            Trading = new HyperLiquidSocketClientFuturesApiTrading(logger, this);
        }
        #endregion

        /// <inheritdoc />
        public IHyperLiquidSocketClientFuturesApiShared SharedClient => this;
    }
}
