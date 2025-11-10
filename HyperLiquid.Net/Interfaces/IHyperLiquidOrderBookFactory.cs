using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.SharedApis;
using System;
using HyperLiquid.Net.Objects.Options;

namespace HyperLiquid.Net.Interfaces
{
    /// <summary>
    /// HyperLiquid local order book factory
    /// </summary>
    public interface IHyperLiquidOrderBookFactory : IExchangeService
    {
        /// <summary>
        /// Order book factory methods
        /// </summary>
        IOrderBookFactory<HyperLiquidOrderBookOptions> Api { get; }

        /// <summary>
        /// Create a SymbolOrderBook for the symbol
        /// </summary>
        /// <param name="symbol">The symbol</param>
        /// <param name="options">Book options</param>
        /// <returns></returns>
        ISymbolOrderBook Create(SharedSymbol symbol, Action<HyperLiquidOrderBookOptions>? options = null);
        
        /// <summary>
        /// Create a new local order book instance
        /// </summary>
        ISymbolOrderBook Create(string symbol, Action<HyperLiquidOrderBookOptions>? options = null);
    }
}