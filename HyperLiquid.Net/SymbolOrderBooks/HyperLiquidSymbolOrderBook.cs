using System;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.OrderBook;
using Microsoft.Extensions.Logging;
using HyperLiquid.Net.Clients;
using HyperLiquid.Net.Interfaces.Clients;
using HyperLiquid.Net.Objects.Options;
using HyperLiquid.Net.Objects.Models;
using Microsoft.Extensions.Options;

namespace HyperLiquid.Net.SymbolOrderBooks
{
    /// <summary>
    /// Implementation for a synchronized order book. After calling Start the order book will sync itself and keep up to date with new data. It will automatically try to reconnect and resync in case of a lost/interrupted connection.
    /// Make sure to check the State property to see if the order book is synced.
    /// </summary>
    public class HyperLiquidSymbolOrderBook : SymbolOrderBook
    {
        private readonly bool _clientOwner;
        private readonly IHyperLiquidSocketClient _socketClient;
        private readonly TimeSpan _initialDataTimeout;
        private readonly int? _nSigFigs;
        private readonly int? _mantissa;

        /// <summary>
        /// Create a new order book instance
        /// </summary>
        /// <param name="symbol">The symbol the order book is for</param>
        /// <param name="optionsDelegate">Option configuration delegate</param>
        public HyperLiquidSymbolOrderBook(string symbol, Action<HyperLiquidOrderBookOptions>? optionsDelegate = null)
            : this(symbol, optionsDelegate, null, null)
        {
            _clientOwner = true;
        }

        /// <summary>
        /// Create a new order book instance
        /// </summary>
        /// <param name="symbol">The symbol the order book is for</param>
        /// <param name="optionsDelegate">Option configuration delegate</param>
        /// <param name="logger">Logger</param>
        /// <param name="socketClient">Socket client instance</param>
        public HyperLiquidSymbolOrderBook(
            string symbol,
            Action<HyperLiquidOrderBookOptions>? optionsDelegate,
            ILoggerFactory? logger,
            IHyperLiquidSocketClient? socketClient) : base(logger, "HyperLiquid", "Api", symbol)
        {
            var options = HyperLiquidOrderBookOptions.Default.Copy();
            if (optionsDelegate != null)
                optionsDelegate(options);
            Initialize(options);

            _strictLevels = false;
            _sequencesAreConsecutive = options?.Limit == null;

            Levels = options?.Limit;
            _nSigFigs = options?.NSigFigs;
            _mantissa = options?.Mantissa;
            _initialDataTimeout = options?.InitialDataTimeout ?? TimeSpan.FromSeconds(30);
            _clientOwner = socketClient == null;
            _socketClient = socketClient ?? new HyperLiquidSocketClient();
        }

        /// <inheritdoc />
        protected override async Task<CallResult<UpdateSubscription>> DoStartAsync(CancellationToken ct)
        {
            // Uses SpotApi but can also be used for futures
            var sub = await _socketClient.SpotApi.SubscribeToOrderBookUpdatesAsync(Symbol, HandleUpdate, nSigFigs: _nSigFigs, mantissa: _mantissa,  ct: ct).ConfigureAwait(false);
            if (!sub)
                return sub;

            Status = OrderBookStatus.Syncing;

            var set = await WaitForSetOrderBookAsync(_initialDataTimeout, ct).ConfigureAwait(false);
            if (!set)
            {
                _ = sub.Data.CloseAsync();
                return new CallResult<UpdateSubscription>(set.Error!);
            }

            Status = OrderBookStatus.Synced;
            return sub;
        }

        private void HandleUpdate(DataEvent<HyperLiquidOrderBook> @event)
        {
            SetInitialOrderBook(@event.Data.Timestamp.Ticks, @event.Data.Levels.Bids, @event.Data.Levels.Asks);
        }

        /// <inheritdoc />
        protected override void DoReset()
        {
        }

        /// <inheritdoc />
        protected override async Task<CallResult<bool>> DoResyncAsync(CancellationToken ct)
        {
            return await WaitForSetOrderBookAsync(_initialDataTimeout, ct).ConfigureAwait(false);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (_clientOwner)
                _socketClient?.Dispose();

            base.Dispose(disposing);
        }
    }
}
