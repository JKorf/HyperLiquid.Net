using Microsoft.Extensions.Logging;
using HyperLiquid.Net.Objects.Options;
using HyperLiquid.Net.Clients.BaseApi;
using HyperLiquid.Net.Interfaces.Clients.SpotApi;

namespace HyperLiquid.Net.Clients.SpotApi
{
    /// <summary>
    /// Client providing access to the HyperLiquid  websocket Api
    /// </summary>
    internal partial class HyperLiquidSocketClientSpotApi : HyperLiquidSocketClientApi, IHyperLiquidSocketClientSpotApi
    {
        public IHyperLiquidSocketClientSpotApiAccount Account { get; }
        public IHyperLiquidSocketClientSpotApiExchangeData ExchangeData { get; }
        public IHyperLiquidSocketClientSpotApiTrading Trading { get; }

        #region constructor/destructor

        /// <summary>
        /// ctor
        /// </summary>
        internal HyperLiquidSocketClientSpotApi(ILoggerFactory? loggerFactory, HyperLiquidSocketClient baseClient, HyperLiquidSocketOptions options) :
            base(loggerFactory, baseClient, options, options.SpotOptions)
        {
            Account = new HyperLiquidSocketClientSpotApiAccount(_logger, this);
            ExchangeData = new HyperLiquidSocketClientSpotApiExchangeData(_logger, this);
            Trading = new HyperLiquidSocketClientSpotApiTrading(_logger, this);
        }
        #endregion

        /// <inheritdoc />
        public IHyperLiquidSocketClientSpotApiShared SharedClient => this;
    }
}
