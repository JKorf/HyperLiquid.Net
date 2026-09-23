using CryptoExchange.Net.SharedApis;

namespace HyperLiquid.Net.Interfaces.Clients.SpotApi
{
    /// <summary>
    /// Shared interface for  socket API usage
    /// </summary>
    public interface IHyperLiquidSocketClientSpotApiShared :
        ITickerSocketClient,
        ITradeSocketClient,
        IKlineSocketClient,
        IOrderBookSocketClient,
        ISpotOrderSocketClient,
        IUserTradeSocketClient,
        IBalanceSocketClient,
        IBookTickerSocketClient,
        ISpotOrderManagementSocketClient
    {
    }

    /// <summary>
    /// Shared API interface. Shared APIs provide a common,
    /// exchange-independent contract for accessing functionality across different
    /// exchange client libraries.
    /// </summary>
    public interface IHyperLiquidSocketClientSpotSharedApi :
        ISubscribeTickerSocket,
        ISubscribeTradesSocket,
        ISubscribeKlinesSocket,
        ISubscribeOrderBookSocket,
        ISubscribeSpotOrdersSocket,
        ISubscribeBalancesSocket,
        ISubscribeBookTickerSocket,
        ISubscribeUserTradesSocket,
        IPlaceSpotOrderSocket,
        ICancelSpotOrderSocket
    {
    }
}
