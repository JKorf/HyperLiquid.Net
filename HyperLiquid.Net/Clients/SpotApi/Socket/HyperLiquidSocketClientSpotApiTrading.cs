using System;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using Microsoft.Extensions.Logging;
using HyperLiquid.Net.Clients.BaseApi;
using HyperLiquid.Net.Interfaces.Clients.SpotApi;

namespace HyperLiquid.Net.Clients.SpotApi
{
    /// <summary>
    /// Client providing access to the HyperLiquid  websocket Api
    /// </summary>
    internal partial class HyperLiquidSocketClientSpotApiTrading : HyperLiquidSocketClientApiTrading, IHyperLiquidSocketClientSpotApiTrading
    {
        #region constructor/destructor

        /// <summary>
        /// ctor
        /// </summary>
        internal HyperLiquidSocketClientSpotApiTrading(ILogger logger, HyperLiquidSocketClientApi baseClient) :
            base(logger, baseClient)
        {
        }
        #endregion

    }
}
