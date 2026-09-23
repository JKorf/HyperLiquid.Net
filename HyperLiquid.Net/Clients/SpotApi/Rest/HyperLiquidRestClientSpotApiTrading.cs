using Microsoft.Extensions.Logging;
using HyperLiquid.Net.Clients.BaseApi;
using HyperLiquid.Net.Interfaces.Clients.SpotApi;

namespace HyperLiquid.Net.Clients.SpotApi
{
    /// <inheritdoc />
    internal class HyperLiquidRestClientSpotApiTrading : HyperLiquidRestClientApiTrading, IHyperLiquidRestClientSpotApiTrading
    {
        internal HyperLiquidRestClientSpotApiTrading(ILogger logger, HyperLiquidRestClientSpotApi baseClient) : base(logger, baseClient)
        {
        }
    }
}
