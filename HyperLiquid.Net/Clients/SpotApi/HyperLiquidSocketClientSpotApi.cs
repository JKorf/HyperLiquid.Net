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
        internal HyperLiquidSocketClientSpotApi(ILogger logger, HyperLiquidSocketClient baseClient, HyperLiquidSocketOptions options) :
            base(logger, baseClient, options, options.SpotOptions)
        {
            Account = new HyperLiquidSocketClientSpotApiAccount(logger, this);
            ExchangeData = new HyperLiquidSocketClientSpotApiExchangeData(logger, this);
            Trading = new HyperLiquidSocketClientSpotApiTrading(logger, this);
        }
        #endregion

        /// <inheritdoc />
        public IHyperLiquidSocketClientSpotApiShared SharedClient => this;
    }
}
