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
        IBookTickerSocketClient
    {
    }
}
