using CryptoExchange.Net.Interfaces.Clients;
using HyperLiquid.Net.Clients;
using HyperLiquid.Net.Interfaces.Clients;

namespace CryptoExchange.Net.Interfaces
{
    /// <summary>
    /// Extensions for the ICryptoRestClient and ICryptoSocketClient interfaces
    /// </summary>
    public static class CryptoClientExtensions
    {
        /// <summary>
        /// Get the HyperLiquid REST Api client
        /// </summary>
        /// <param name="baseClient"></param>
        /// <returns></returns>
        public static IHyperLiquidRestClient HyperLiquid(this ICryptoRestClient baseClient) => baseClient.TryGet<IHyperLiquidRestClient>(() => new HyperLiquidRestClient());

        /// <summary>
        /// Get the HyperLiquid Websocket Api client
        /// </summary>
        /// <param name="baseClient"></param>
        /// <returns></returns>
        public static IHyperLiquidSocketClient HyperLiquid(this ICryptoSocketClient baseClient) => baseClient.TryGet<IHyperLiquidSocketClient>(() => new HyperLiquidSocketClient());
    }
}
