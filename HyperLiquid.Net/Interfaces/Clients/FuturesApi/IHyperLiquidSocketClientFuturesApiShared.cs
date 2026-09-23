using CryptoExchange.Net.SharedApis;

namespace HyperLiquid.Net.Interfaces.Clients.FuturesApi
{
    /// <summary>
    /// Shared interface for  socket API usage
    /// </summary>
    public interface IHyperLiquidSocketClientFuturesApiShared :
        ITickerSocketClient,
        ITradeSocketClient,
        IKlineSocketClient,
        IOrderBookSocketClient,
        IUserTradeSocketClient,
        IFuturesOrderSocketClient,
        IBalanceSocketClient,
        IPositionSocketClient,
        IBookTickerSocketClient,
        IFuturesOrderManagementSocketClient
    {
    }

    /// <summary>
    /// Shared API interface. Shared APIs provide a common,
    /// exchange-independent contract for accessing functionality across different
    /// exchange client libraries.
    /// </summary>
    public interface IHyperLiquidSocketClientFuturesSharedApi :
        ISubscribeTickerSocket,
        ISubscribeTradesSocket,
        ISubscribeKlinesSocket,
        ISubscribeOrderBookSocket,
        ISubscribeUserTradesSocket,
        ISubscribeFuturesOrdersSocket,
        ISubscribeBalancesSocket,
        ISubscribePositionsSocket,
        ISubscribeBookTickerSocket,
        IPlaceFuturesOrderSocket,
        ICancelFuturesOrderSocket
    {
    }
}
