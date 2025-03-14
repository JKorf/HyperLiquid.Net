using CryptoExchange.Net.SharedApis;

namespace HyperLiquid.Net.Interfaces.Clients.FuturesApi
{
    /// <summary>
    /// Shared interface for futures rest API usage
    /// </summary>
    public interface IHyperLiquidRestClientFuturesApiShared :
        IBalanceRestClient,
        IKlineRestClient,
        IOrderBookRestClient,
        IFuturesTickerRestClient,
        IFuturesSymbolRestClient,
        IFeeRestClient,
        IFundingRateRestClient,
        ILeverageRestClient,
        IOpenInterestRestClient,
        IFuturesOrderRestClient,
        IFuturesOrderClientIdClient
    {
    }
}
