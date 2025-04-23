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
        IBookTickerRestClient
    {
    }
}
