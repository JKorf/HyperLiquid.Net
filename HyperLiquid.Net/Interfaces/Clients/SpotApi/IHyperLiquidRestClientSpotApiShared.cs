using CryptoExchange.Net.SharedApis;

namespace HyperLiquid.Net.Interfaces.Clients.SpotApi
{
    /// <summary>
    /// Shared interface for spot rest API usage
    /// </summary>
    public interface IHyperLiquidRestClientSpotApiShared :
        IBalanceRestClient,
        IKlineRestClient,
        IOrderBookRestClient,
        ISpotTickerRestClient,
        ISpotSymbolRestClient,
        ISpotOrderRestClient,
        IAssetsRestClient,
        IFeeRestClient,
        IWithdrawRestClient,
        ISpotOrderClientIdRestClient,
        IBookTickerRestClient,
        ITransferRestClient
    {
    }

    /// <summary>
    /// Shared API interface. Shared APIs provide a common,
    /// exchange-independent contract for accessing functionality across different
    /// exchange client libraries.
    /// </summary>
    public interface IHyperLiquidRestClientSpotSharedApi :
        IGetBalancesRest,
        IGetKlinesRest,
        IGetOrderBookRest,
        IGetTickerRest,
        IGetAllTickersRest,
        IGetSpotSymbolsRest,
        IPlaceSpotOrderRest,
        IGetSpotOrderRest,
        IGetOpenSpotOrdersRest,
        IGetClosedSpotOrdersRest,
        IGetSpotOrderTradesRest,
        IGetSpotUserTradeHistoryRest,
        ICancelSpotOrderRest,
        IGetAssetRest,
        IGetAllAssetsRest,
        IGetFeesRest,
        IWithdrawRest,
        IGetSpotOrderByClientOrderIdRest,
        ICancelSpotOrderByClientOrderIdRest,
        IGetBookTickerRest,
        ITransferRest
    {
    }
}
