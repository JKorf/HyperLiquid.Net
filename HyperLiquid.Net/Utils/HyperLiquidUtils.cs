using CryptoExchange.Net.Objects;
using HyperLiquid.Net.Clients;
using HyperLiquid.Net.Objects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HyperLiquid.Net.Utils
{
    /// <summary>
    /// Util methods for the HyperLiquid API
    /// </summary>
    public static class HyperLiquidUtils
    {
        private static HyperLiquidAsset[]? _spotAssetInfo;
        private static HyperLiquidSymbol[]? _spotSymbolInfo;
        private static HyperLiquidFuturesSymbol[]? _futuresSymbolInfo;

        private static DateTime _lastSpotUpdateTime;
        private static DateTime _lastFuturesUpdateTime;

        private static readonly SemaphoreSlim _semaphoreSpot = new SemaphoreSlim(1, 1);
        private static readonly SemaphoreSlim _semaphoreFutures = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Update the internal futures symbol info
        /// </summary>
        public static async Task<CallResult> UpdateFuturesSymbolInfoAsync()
        {
            await _semaphoreFutures.WaitAsync().ConfigureAwait(false);

            try
            {
                if (DateTime.UtcNow - _lastFuturesUpdateTime < TimeSpan.FromHours(1))
                    return CallResult.SuccessResult;

                var symbolInfo = await new HyperLiquidRestClient().FuturesApi.ExchangeData.GetExchangeInfoAsync().ConfigureAwait(false);
                if (!symbolInfo)
                    return symbolInfo.AsDataless();

                _futuresSymbolInfo = symbolInfo.Data;
                _lastFuturesUpdateTime = DateTime.UtcNow;
                return CallResult.SuccessResult;
            }
            finally
            {
                _semaphoreFutures.Release();
            }
        }

        /// <summary>
        /// Update the internal spot symbol info
        /// </summary>
        public static async Task<CallResult> UpdateSpotSymbolInfoAsync()
        {
            await _semaphoreSpot.WaitAsync().ConfigureAwait(false);
            try
            {
                if (DateTime.UtcNow - _lastSpotUpdateTime < TimeSpan.FromHours(1))
                    return CallResult.SuccessResult;

                var symbolInfo = await new HyperLiquidRestClient().SpotApi.ExchangeData.GetExchangeInfoAsync().ConfigureAwait(false);
                if (!symbolInfo)
                    return symbolInfo.AsDataless();

                _spotSymbolInfo = symbolInfo.Data.Symbols;
                _spotAssetInfo = symbolInfo.Data.Assets;
                _lastSpotUpdateTime = DateTime.UtcNow;
                return CallResult.SuccessResult;
            }
            finally
            {
                _semaphoreSpot.Release();
            }
        }

        /// <summary>
        /// Get symbol id from a symbol name
        /// </summary>
        /// <param name="symbolName">Symbol name</param>
        /// <returns></returns>
        public static async Task<CallResult<int>> GetSymbolIdFromNameAsync(string symbolName)
        {
            if (SymbolIsExchangeSpotSymbol(symbolName))
            {
                var update = await UpdateSpotSymbolInfoAsync().ConfigureAwait(false);
                if (!update)
                    return new CallResult<int>(update.Error!);

                var symbol = _spotSymbolInfo!.SingleOrDefault(x => x.Name == symbolName);
                if (symbol == null)
                    return new CallResult<int>(new ServerError("Symbol not found"));

                return new CallResult<int>(symbol.Index + 10000);
            }
            else
            {
                var update = await UpdateFuturesSymbolInfoAsync().ConfigureAwait(false);
                if (!update)
                    return new CallResult<int>(update.Error!);

                var symbol = _futuresSymbolInfo!.SingleOrDefault(x => x.Name == symbolName);
                if (symbol == null)
                    return new CallResult<int>(new ServerError("Symbol not found"));

                return new CallResult<int>(symbol.Index);
            }
        }

        /// <summary>
        /// Get a symbol name from an exchange symbol name
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static CallResult<string> GetSymbolNameFromExchangeName(string id)
        {
            var symbol = _spotSymbolInfo?.SingleOrDefault(x => x.ExchangeName == id);
            if (symbol == null)
                return new CallResult<string>(new ServerError("Symbol not found"));

            return new CallResult<string>(symbol.Name);
        }

        /// <summary>
        /// Get a symbol name from an exchange symbol name
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<CallResult<string>> GetSymbolNameFromExchangeNameAsync(string id)
        {
            var update = await UpdateSpotSymbolInfoAsync().ConfigureAwait(false);
            if (!update)
                return new CallResult<string>(update.Error!);

            var symbol = _spotSymbolInfo!.SingleOrDefault(x => x.ExchangeName == id);
            if (symbol == null)
                return new CallResult<string>(new ServerError("Symbol not found"));

            return new CallResult<string>(symbol.Name);
        }

        /// <summary>
        /// Get an exchange symbol name from a symbol name
        /// </summary>
        /// <returns></returns>
        public static async Task<CallResult<string>> GetExchangeNameFromSymbolNameAsync(string name)
        {
            var update = await UpdateSpotSymbolInfoAsync().ConfigureAwait(false);
            if (!update)
                return new CallResult<string>(update.Error!);

            var symbol = _spotSymbolInfo!.SingleOrDefault(x => x.Name == name);
            if (symbol == null)
                return new CallResult<string>(new ServerError("Symbol not found"));

            return new CallResult<string>(symbol.ExchangeName);
        }

        /// <summary>
        /// Get a symbol name from an exchange symbol name
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetSymbolNameFromExchangeName(IEnumerable<string> ids)
        {
            var result = new Dictionary<string, string>();
            foreach (var id in ids)
            {
                var symbol = _spotSymbolInfo?.SingleOrDefault(x => x.ExchangeName == id);
                if (symbol == null)
                    continue;

                result[id] = symbol.Name;
            }

            return result;
        }

        /// <summary>
        /// Get an asset name and id from an exchange asset name
        /// </summary>
        /// <param name="asset">Exchange asset name</param>
        /// <returns></returns>
        public static async Task<CallResult<string>> GetAssetNameAndIdAsync(string asset)
        {
            var update = await UpdateSpotSymbolInfoAsync().ConfigureAwait(false);
            if (!update)
                return new CallResult<string>(update.Error!);

            var assetInfo = _spotAssetInfo!.SingleOrDefault(x => x.Name == asset);
            if (assetInfo == null)
                return new CallResult<string>(new ServerError("Asset not found"));

            return new CallResult<string>(assetInfo.Name + ":" + assetInfo.AssetId);
        }

        internal static bool ExchangeSymbolIsSpotSymbol(string symbol)
        {
            return symbol.StartsWith("@") || symbol.EndsWith("/USDC");
        }

        internal static bool SymbolIsExchangeSpotSymbol(string symbol)
        {
            return symbol.EndsWith("/USDC");
        }
    }
}
