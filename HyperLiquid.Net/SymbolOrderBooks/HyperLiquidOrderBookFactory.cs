using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.OrderBook;
using CryptoExchange.Net.SharedApis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using HyperLiquid.Net.Interfaces;
using HyperLiquid.Net.Interfaces.Clients;
using HyperLiquid.Net.Objects.Options;

namespace HyperLiquid.Net.SymbolOrderBooks
{
    /// <summary>
    /// HyperLiquid order book factory
    /// </summary>
    public class HyperLiquidOrderBookFactory : IHyperLiquidOrderBookFactory
    {
        private readonly IServiceProvider _serviceProvider;

        /// <inheritdoc />
        public string ExchangeName => HyperLiquidExchange.ExchangeName;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="serviceProvider">Service provider for resolving logging and clients</param>
        public HyperLiquidOrderBookFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            Api = new OrderBookFactory<HyperLiquidOrderBookOptions>(Create, Create);
        }

        
         /// <inheritdoc />
        public IOrderBookFactory<HyperLiquidOrderBookOptions> Api { get; }


        /// <inheritdoc />
        public ISymbolOrderBook Create(SharedSymbol symbol, Action<HyperLiquidOrderBookOptions>? options = null)
        {
            var symbolName = symbol.GetSymbol(HyperLiquidExchange.FormatSymbol);
            return Create(symbolName, options);
        }

        
         /// <inheritdoc />
        public ISymbolOrderBook Create(string symbol, Action<HyperLiquidOrderBookOptions>? options = null)
            => new HyperLiquidSymbolOrderBook(symbol, options, 
                                                          _serviceProvider.GetRequiredService<ILoggerFactory>(),
                                                          _serviceProvider.GetRequiredService<IHyperLiquidSocketClient>());


    }
}
