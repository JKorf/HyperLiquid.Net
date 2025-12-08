using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Errors;
using HyperLiquid.Net.Interfaces.Clients;
using HyperLiquid.Net.Objects.Models;
using HyperLiquid.Net.Objects.Options;
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
        private static Dictionary<string, HyperLiquidAsset[]> _spotAssetInfo = new Dictionary<string, HyperLiquidAsset[]>();
        private static Dictionary<string, HyperLiquidSymbol[]> _spotSymbolInfo = new Dictionary<string, HyperLiquidSymbol[]>();
        private static Dictionary<string, HyperLiquidFuturesSymbol[]> _futuresSymbolInfo = new Dictionary<string, HyperLiquidFuturesSymbol[]>();

        private static Dictionary<string, DateTime> _lastSpotUpdateTime = new Dictionary<string, DateTime>();
        private static Dictionary<string, DateTime> _lastFuturesUpdateTime = new Dictionary<string, DateTime>();

        private static readonly SemaphoreSlim _semaphoreSpot = new SemaphoreSlim(1, 1);
        private static readonly SemaphoreSlim _semaphoreFutures = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Update the internal futures symbol info
        /// </summary>
        public static async Task<CallResult> UpdateFuturesSymbolInfoAsync(IHyperLiquidRestClient client)
        {
            await _semaphoreFutures.WaitAsync().ConfigureAwait(false);

            try
            {
                var envName = ((HyperLiquidRestOptions)client.ClientOptions).Environment.Name;
                if (envName.Equals("UnitTest", StringComparison.Ordinal))
                    return CallResult.SuccessResult;

                _lastFuturesUpdateTime.TryGetValue(envName, out var lastUpdateTime);
                if (DateTime.UtcNow - lastUpdateTime < TimeSpan.FromHours(1))
                    return CallResult.SuccessResult;

                var symbolInfo = await client.FuturesApi.ExchangeData.GetExchangeInfoAsync().ConfigureAwait(false);
                if (!symbolInfo)
                    return symbolInfo.AsDataless();

                _futuresSymbolInfo[envName] = symbolInfo.Data;
                _lastFuturesUpdateTime[envName] = DateTime.UtcNow;
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
        public static async Task<CallResult> UpdateSpotSymbolInfoAsync(IHyperLiquidRestClient client)
        {
            await _semaphoreSpot.WaitAsync().ConfigureAwait(false);
            try
            {
                var envName = ((HyperLiquidRestOptions)client.ClientOptions).Environment.Name;
                if (envName.Equals("UnitTest", StringComparison.Ordinal))
                    return CallResult.SuccessResult;

                _lastSpotUpdateTime.TryGetValue(envName, out var lastUpdateTime);
                if (DateTime.UtcNow - lastUpdateTime < TimeSpan.FromHours(1))
                    return CallResult.SuccessResult;

                var symbolInfo = await client.SpotApi.ExchangeData.GetExchangeInfoAsync().ConfigureAwait(false);
                if (!symbolInfo)
                    return symbolInfo.AsDataless();

                _spotSymbolInfo[envName] = symbolInfo.Data.Symbols;
                _spotAssetInfo[envName] = symbolInfo.Data.Assets;
                _lastSpotUpdateTime[envName] = DateTime.UtcNow;
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
        /// <param name="client">Client to make a request to retrieve exchange info if necessary</param>
        /// <param name="symbolName">Symbol name</param>
        /// <returns></returns>
        public static async Task<CallResult<int>> GetSymbolIdFromNameAsync(IHyperLiquidRestClient client, string symbolName)
        {
            if (symbolName == "UnitTest")
                return new CallResult<int>(1);

            if (SymbolIsExchangeSpotSymbol(symbolName))
            {
                var update = await UpdateSpotSymbolInfoAsync(client).ConfigureAwait(false);
                if (!update)
                    return new CallResult<int>(update.Error!);

                var envName = ((HyperLiquidRestOptions)client.ClientOptions).Environment.Name;
                var symbol = _spotSymbolInfo[envName].SingleOrDefault(x => x.Name == symbolName);
                if (symbol == null)
                    return new CallResult<int>(new ServerError(new ErrorInfo(ErrorType.UnknownSymbol, "Symbol not found")));

                return new CallResult<int>(symbol.Index + 10000);
            }
            else
            {
                var update = await UpdateFuturesSymbolInfoAsync(client).ConfigureAwait(false);
                if (!update)
                    return new CallResult<int>(update.Error!);

                var envName = ((HyperLiquidRestOptions)client.ClientOptions).Environment.Name;
                var symbol = _futuresSymbolInfo[envName].SingleOrDefault(x => x.Name == symbolName);
                if (symbol == null)
                    return new CallResult<int>(new ServerError(new ErrorInfo(ErrorType.UnknownSymbol, "Symbol not found")));

                return new CallResult<int>(symbol.Index);
            }
        }

        /// <summary>
        /// Get the quantity decimal places for a symbol
        /// </summary>
        /// <param name="client"></param>
        /// <param name="symbolName"></param>
        /// <returns></returns>
        public static async Task<CallResult<int>> GetQuantityDecimalPlacesForSymbolAsync(IHyperLiquidRestClient client, string symbolName)
        {
            if (symbolName == "UnitTest")
                return new CallResult<int>(1);

            if (SymbolIsExchangeSpotSymbol(symbolName))
            {
                var update = await UpdateSpotSymbolInfoAsync(client).ConfigureAwait(false);
                if (!update)
                    return new CallResult<int>(update.Error!);

                var envName = ((HyperLiquidRestOptions)client.ClientOptions).Environment.Name;
                var symbol = _spotSymbolInfo[envName].SingleOrDefault(x => x.Name == symbolName);
                if (symbol == null)
                    return new CallResult<int>(new ServerError(new ErrorInfo(ErrorType.UnknownSymbol, "Symbol not found")));

                return new CallResult<int>(symbol.BaseAsset.QuantityDecimals);
            }
            else
            {
                var update = await UpdateFuturesSymbolInfoAsync(client).ConfigureAwait(false);
                if (!update)
                    return new CallResult<int>(update.Error!);

                var envName = ((HyperLiquidRestOptions)client.ClientOptions).Environment.Name;
                var symbol = _futuresSymbolInfo[envName].SingleOrDefault(x => x.Name == symbolName);
                if (symbol == null)
                    return new CallResult<int>(new ServerError(new ErrorInfo(ErrorType.UnknownSymbol, "Symbol not found")));

                return new CallResult<int>(symbol.QuantityDecimals);
            }
        }

        /// <summary>
        /// Get a symbol name from an exchange symbol name
        /// </summary>
        public static CallResult<string> GetSymbolNameFromExchangeName(string envName, string id)
        {
            if (envName.Equals("UnitTest", StringComparison.Ordinal))
                return new CallResult<string>("HYPE");

            var symbol = _spotSymbolInfo[envName].SingleOrDefault(x => x.ExchangeName == id);
            if (symbol == null)
                return new CallResult<string>(new ServerError(new ErrorInfo(ErrorType.UnknownSymbol, "Symbol not found")));

            return new CallResult<string>(symbol.Name);
        }

        /// <summary>
        /// Get a symbol name from an exchange symbol name
        /// </summary>
        /// <param name="client">Client to make a request to retrieve exchange info if necessary</param>
        /// <param name="id">Id</param>
        /// <returns></returns>
        public static async Task<CallResult<string>> GetSymbolNameFromExchangeNameAsync(IHyperLiquidRestClient client, string id)
        {
            var envName = ((HyperLiquidRestOptions)client.ClientOptions).Environment.Name;
            if (envName.Equals("UnitTest", StringComparison.Ordinal))
                return new CallResult<string>("HYPE");

            var update = await UpdateSpotSymbolInfoAsync(client).ConfigureAwait(false);
            if (!update)
                return new CallResult<string>(update.Error!);

            var symbol = _spotSymbolInfo[envName].SingleOrDefault(x => x.ExchangeName == id);
            if (symbol == null)
                return new CallResult<string>(new ServerError(new ErrorInfo(ErrorType.UnknownSymbol, "Symbol not found")));

            return new CallResult<string>(symbol.Name);
        }

        /// <summary>
        /// Get an exchange symbol name from a symbol name
        /// </summary>
        /// <returns></returns>
        public static async Task<CallResult<string>> GetExchangeNameFromSymbolNameAsync(IHyperLiquidRestClient client, string name)
        {
            var envName = ((HyperLiquidRestOptions)client.ClientOptions).Environment.Name;
            if (envName.Equals("UnitTest", StringComparison.Ordinal))
                return new CallResult<string>("HYPE");

            var update = await UpdateSpotSymbolInfoAsync(client).ConfigureAwait(false);
            if (!update)
                return new CallResult<string>(update.Error!);

            var symbol = _spotSymbolInfo[envName].SingleOrDefault(x => x.Name == name);
            if (symbol == null)
                return new CallResult<string>(new ServerError(new ErrorInfo(ErrorType.UnknownSymbol, "Symbol not found")));

            return new CallResult<string>(symbol.ExchangeName);
        }

        /// <summary>
        /// Get a symbol name from an exchange symbol name
        /// </summary>
        public static Dictionary<string, string> GetSymbolNameFromExchangeName(string envName, IEnumerable<string> ids)
        {
            var result = new Dictionary<string, string>();
            if (envName.Equals("UnitTest", StringComparison.Ordinal))
                return result;

            foreach (var id in ids)
            {
                var symbol = _spotSymbolInfo[envName].SingleOrDefault(x => x.ExchangeName == id);
                if (symbol == null)
                    continue;

                result[id] = symbol.Name;
            }

            return result;
        }

        /// <summary>
        /// Get an asset name and id from an exchange asset name
        /// </summary>
        /// <param name="client">Client to make a request to retrieve exchange info if necessary</param>
        /// <param name="asset">Exchange asset name</param>
        /// <returns></returns>
        public static async Task<CallResult<string>> GetAssetNameAndIdAsync(IHyperLiquidRestClient client, string asset)
        {
            var envName = ((HyperLiquidRestOptions)client.ClientOptions).Environment.Name;
            if (envName.Equals("UnitTest", StringComparison.Ordinal))
                return new CallResult<string>("HYPE");

            var update = await UpdateSpotSymbolInfoAsync(client).ConfigureAwait(false);
            if (!update)
                return new CallResult<string>(update.Error!);

            var assetInfo = _spotAssetInfo[envName].SingleOrDefault(x => x.Name == asset);
            if (assetInfo == null)
                return new CallResult<string>(new ServerError(new ErrorInfo(ErrorType.UnknownAsset, "Asset not found")));

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
