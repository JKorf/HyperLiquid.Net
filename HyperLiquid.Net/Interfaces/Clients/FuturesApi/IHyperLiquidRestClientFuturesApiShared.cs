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
        IFuturesOrderClientIdRestClient,
        IFuturesTpSlRestClient,
        IBookTickerRestClient
    {
    }

    /// <summary>
    /// Shared API interface. Shared APIs provide a common,
    /// exchange-independent contract for accessing functionality across different
    /// exchange client libraries.
    /// </summary>
    public interface IHyperLiquidRestClientFuturesSharedApi :
        IGetBalancesRest,
        IGetKlinesRest,
        IGetOrderBookRest,
        IGetTickerRest,
        IGetAllTickersRest,
        IGetFeesRest,
        IGetFundingRateHistoryRest,
        IGetLeverageRest,
        ISetLeverageRest,
        IGetOpenInterestRest,
        IGetFuturesSymbolsRest,
        IPlaceFuturesOrderRest,
        IGetFuturesOrderRest,
        IGetOpenFuturesOrdersRest,
        IGetClosedFuturesOrdersRest,
        IGetFuturesOrderTradesRest,
        IGetFuturesUserTradeHistoryRest,
        ICancelFuturesOrderRest,
        IGetPositionsRest,
        IGetFuturesOrderByClientOrderIdRest,
        ICancelFuturesOrderByClientOrderIdRest,
        ISetFuturesTpSlRest,
        ICancelFuturesTpSlRest,
        IGetBookTickerRest
    { 
    }
}
