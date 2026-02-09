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
        IBookTickerSocketClient
    {
    }
}
