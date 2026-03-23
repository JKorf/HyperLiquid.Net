using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Errors;
using HyperLiquid.Net.Clients;
using HyperLiquid.Net.Clients.BaseApi;
using HyperLiquid.Net.Interfaces.Clients;
using HyperLiquid.Net.Objects.Models;
using HyperLiquid.Net.Objects.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HyperLiquid.Net.Utils
{
    /// <summary>
    /// Util methods for the HyperLiquid API
    /// </summary>
    public static class HyperLiquidUtils
    {
        private static Dictionary<string, HyperLiquidAsset[]> _spotAssetInfo = new Dictionary<string, HyperLiquidAsset[]>();
        private static Dictionary<string, HyperLiquidSymbol[]> _spotSymbolInfo = new Dictionary<string, HyperLiquidSymbol[]>();
        private static Dictionary<string, HyperLiquidFuturesDexInfo[]> _futuresSymbolInfo = new Dictionary<string, HyperLiquidFuturesDexInfo[]>();

        private static Dictionary<string, DateTime> _lastSpotUpdateTime = new Dictionary<string, DateTime>();
        private static Dictionary<string, DateTime> _lastFuturesUpdateTime = new Dictionary<string, DateTime>();

        private static readonly SemaphoreSlim _semaphoreSpot = new SemaphoreSlim(1, 1);
        private static readonly SemaphoreSlim _semaphoreFutures = new SemaphoreSlim(1, 1);
        private static readonly SemaphoreSlim _semaphoreBuilderFee = new SemaphoreSlim(1, 1);
        private static bool _checkedBuilderFee = false;

        internal static async Task<CallResult> CheckBuilderFeeAsync(HyperLiquidSocketClient client)
        {
            if (!client.SpotApi.Authenticated)
                // No credentials provided, no need to check builder fee
                return CallResult.SuccessResult;

            var envName = client.ClientOptions.Environment.Name;
            if (envName.Equals("UnitTest", StringComparison.Ordinal))
                return CallResult.SuccessResult;

            var options = client.ClientOptions;
            return await CheckBuilderFeeAsync(
                options.BuilderFeePercentage, 
                () => client.SpotApi.Account.GetApprovedBuilderFeeAsync(),
                () => client.SpotApi.Account.ApproveBuilderFeeAsync()).ConfigureAwait(false);
        }

        internal static async Task<CallResult> CheckBuilderFeeAsync(HyperLiquidRestClient client)
        {
            if (!client.SpotApi.Authenticated)
                // No credentials provided, no need to check builder fee
                return CallResult.SuccessResult;

            var envName = client.ClientOptions.Environment.Name;
            if (envName.Equals("UnitTest", StringComparison.Ordinal))
                return CallResult.SuccessResult;

            var options = client.ClientOptions;
            return await CheckBuilderFeeAsync(
                options.BuilderFeePercentage,
                async () => await client.SpotApi.Account.GetApprovedBuilderFeeAsync().ConfigureAwait(false),
                async () => await client.SpotApi.Account.ApproveBuilderFeeAsync().ConfigureAwait(false)).ConfigureAwait(false);
        }

        internal static async Task<CallResult> CheckBuilderFeeAsync(
            decimal? builderFeePercentage,
            Func<Task<CallResult<int>>> getApprovedFee,
            Func<Task<CallResult>> approveFee)
        {
            if (_checkedBuilderFee)
                return CallResult.SuccessResult;

            if (builderFeePercentage == null
                || builderFeePercentage == 0)
            {
                // No builder fee, no need to check
                return CallResult.SuccessResult;
            }

            await _semaphoreBuilderFee.WaitAsync().ConfigureAwait(false);
            try
            {
                // Set to true even if the check fails to avoid continuously trying to check and approve the builder fee if there's an issue
                _checkedBuilderFee = true;

                var approvedResult = await getApprovedFee().ConfigureAwait(false);
                if (!approvedResult)
                    return approvedResult;

                var targetBps = (int)(builderFeePercentage.Value * 1000);
                if (approvedResult.Data >= targetBps)
                    // Builder fee is approved, we're good
                    return CallResult.SuccessResult;

                var approveResult = await approveFee().ConfigureAwait(false);
                return approvedResult;
            }
            finally
            {
                _semaphoreBuilderFee.Release();
            }
        }

        /// <summary>
        /// Update the internal spot symbol info
        /// </summary>
        public static async Task<CallResult> UpdateFuturesSymbolInfoAsync(HyperLiquidRestClient client)
        {
            var envName = client.ClientOptions.Environment.Name;
            if (envName.Equals("UnitTest", StringComparison.Ordinal))
                return CallResult.SuccessResult;

            return await UpdateFuturesSymbolInfoAsync(envName, async () => await client.FuturesApi.ExchangeData.GetExchangeInfoAllDexesAsync().ConfigureAwait(false)).ConfigureAwait(false);
        }

        /// <summary>
        /// Update the internal spot symbol info
        /// </summary>
        public static async Task<CallResult> UpdateFuturesSymbolInfoAsync(HyperLiquidSocketClient client)
        {
            var envName = client.ClientOptions.Environment.Name;
            if (envName.Equals("UnitTest", StringComparison.Ordinal))
                return CallResult.SuccessResult;

            return await UpdateFuturesSymbolInfoAsync(envName, async () => await client.FuturesApi.ExchangeData.GetExchangeInfoAllDexesAsync().ConfigureAwait(false)).ConfigureAwait(false);
        }

        /// <summary>
        /// Update the internal futures symbol info
        /// </summary>
        private static async Task<CallResult> UpdateFuturesSymbolInfoAsync(string envName, Func<Task<CallResult<HyperLiquidFuturesDexInfo[]>>> exchangeInfoCall)
        {
            await _semaphoreFutures.WaitAsync().ConfigureAwait(false);

            try
            {
                _lastFuturesUpdateTime.TryGetValue(envName, out var lastUpdateTime);
                if (DateTime.UtcNow - lastUpdateTime < TimeSpan.FromHours(1))
                    return CallResult.SuccessResult;

                var symbolInfo = await exchangeInfoCall().ConfigureAwait(false);
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
        public static async Task<CallResult> UpdateSpotSymbolInfoAsync(HyperLiquidRestClient client)
        {
            var envName = client.ClientOptions.Environment.Name;
            if (envName.Equals("UnitTest", StringComparison.Ordinal))
                return CallResult.SuccessResult;

            return await UpdateSpotSymbolInfoAsync(envName, async () => await client.SpotApi.ExchangeData.GetExchangeInfoAsync().ConfigureAwait(false)).ConfigureAwait(false);
        }

        /// <summary>
        /// Update the internal spot symbol info
        /// </summary>
        public static async Task<CallResult> UpdateSpotSymbolInfoAsync(HyperLiquidSocketClient client)
        {
            var envName = client.ClientOptions.Environment.Name;
            if (envName.Equals("UnitTest", StringComparison.Ordinal))
                return CallResult.SuccessResult;

            return await UpdateSpotSymbolInfoAsync(envName, async () => await client.SpotApi.ExchangeData.GetExchangeInfoAsync().ConfigureAwait(false)).ConfigureAwait(false);
        }

        private static async Task<CallResult> UpdateSpotSymbolInfoAsync(string envName, Func<Task<CallResult<HyperLiquidSpotExchangeInfo>>> exchangeInfoCall)
        {
            await _semaphoreSpot.WaitAsync().ConfigureAwait(false);
            try
            {
                _lastSpotUpdateTime.TryGetValue(envName, out var lastUpdateTime);
                if (DateTime.UtcNow - lastUpdateTime < TimeSpan.FromHours(1))
                    return CallResult.SuccessResult;

                var symbolInfo = await exchangeInfoCall().ConfigureAwait(false);
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
        public static async Task<CallResult<int>> GetSymbolIdFromNameAsync(HyperLiquidRestClient client, string symbolName)
        {
            var envName = client.ClientOptions.Environment.Name;
            if (envName.Equals("UnitTest", StringComparison.Ordinal))
                return new CallResult<int>(1);

            return await GetSymbolIdFromNameAsync(envName, symbolName, () => UpdateSpotSymbolInfoAsync(client), () => UpdateFuturesSymbolInfoAsync(client)).ConfigureAwait(false);
        }

        /// <summary>
        /// Get symbol id from a symbol name
        /// </summary>
        public static async Task<CallResult<int>> GetSymbolIdFromNameAsync(HyperLiquidSocketClient client, string symbolName)
        {
            var envName = client.ClientOptions.Environment.Name;
            if (envName.Equals("UnitTest", StringComparison.Ordinal))
                return new CallResult<int>(1);

            return await GetSymbolIdFromNameAsync(envName, symbolName, () => UpdateSpotSymbolInfoAsync(client), () => UpdateFuturesSymbolInfoAsync(client)).ConfigureAwait(false);
        }

        private static async Task<CallResult<int>> GetSymbolIdFromNameAsync(string envName, string symbolName, Func<Task<CallResult>> spotUpdate, Func<Task<CallResult>> futuresUpdate)
        {
            if (SymbolIsExchangeSpotSymbol(symbolName))
            {
                var update = await spotUpdate().ConfigureAwait(false);
                if (!update)
                    return new CallResult<int>(update.Error!);

                var symbol = _spotSymbolInfo[envName].SingleOrDefault(x => x.Name == symbolName);
                if (symbol == null)
                    return new CallResult<int>(new ServerError(new ErrorInfo(ErrorType.UnknownSymbol, "Symbol not found")));

                return new CallResult<int>(symbol.Index + 10000);
            }
            else
            {
                var update = await futuresUpdate().ConfigureAwait(false);
                if (!update)
                    return new CallResult<int>(update.Error!);

                var symbolSplit = symbolName.Split(':');
                if (symbolSplit.Length == 2)
                {
                    // DEX
                    var dexName = symbolSplit[0];
                    var dex = _futuresSymbolInfo[envName].SingleOrDefault(x => x.Name.Equals(dexName, StringComparison.InvariantCultureIgnoreCase));
                    if (dex == null)
                        return new CallResult<int>(new ServerError(new ErrorInfo(ErrorType.UnknownSymbol, "DEX not found")));

                    var symbol = dex.Symbols.SingleOrDefault(x => x.Name == symbolName);
                    if (symbol == null)
                        return new CallResult<int>(new ServerError(new ErrorInfo(ErrorType.UnknownSymbol, "Symbol not found")));

                    return new CallResult<int>(100000 + dex.Index * 10000 + symbol.Index);
                }
                else
                {
                    var symbol = _futuresSymbolInfo[envName][0].Symbols.SingleOrDefault(x => x.Name == symbolName);
                    if (symbol == null)
                        return new CallResult<int>(new ServerError(new ErrorInfo(ErrorType.UnknownSymbol, "Symbol not found")));

                    return new CallResult<int>(symbol.Index);
                }
            }
        }

        /// <summary>
        /// Get the quantity decimal places for a symbol
        /// </summary>
        public static async Task<CallResult<int>> GetQuantityDecimalPlacesForSymbolAsync(HyperLiquidRestClient client, string symbolName)
        {
            var envName = client.ClientOptions.Environment.Name;
            if (envName.Equals("UnitTest", StringComparison.Ordinal))
                return new CallResult<int>(1);

            return await GetQuantityDecimalPlacesForSymbolAsync(envName, symbolName, () => UpdateSpotSymbolInfoAsync(client), () => UpdateFuturesSymbolInfoAsync(client)).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the quantity decimal places for a symbol
        /// </summary>
        public static async Task<CallResult<int>> GetQuantityDecimalPlacesForSymbolAsync(HyperLiquidSocketClient client, string symbolName)
        {
            var envName = client.ClientOptions.Environment.Name;
            if (envName.Equals("UnitTest", StringComparison.Ordinal))
                return new CallResult<int>(1);

            return await GetQuantityDecimalPlacesForSymbolAsync(envName, symbolName, () => UpdateSpotSymbolInfoAsync(client), () => UpdateFuturesSymbolInfoAsync(client)).ConfigureAwait(false);
        }

        private static async Task<CallResult<int>> GetQuantityDecimalPlacesForSymbolAsync(string envName, string symbolName, Func<Task<CallResult>> spotUpdate, Func<Task<CallResult>> futuresUpdate)
        {
            if (SymbolIsExchangeSpotSymbol(symbolName))
            {
                var update = await spotUpdate().ConfigureAwait(false);
                if (!update)
                    return new CallResult<int>(update.Error!);

                var symbol = _spotSymbolInfo[envName].SingleOrDefault(x => x.Name == symbolName);
                if (symbol == null)
                    return new CallResult<int>(new ServerError(new ErrorInfo(ErrorType.UnknownSymbol, "Symbol not found")));

                return new CallResult<int>(symbol.BaseAsset.QuantityDecimals);
            }
            else
            {
                var update = await futuresUpdate().ConfigureAwait(false);
                if (!update)
                    return new CallResult<int>(update.Error!);

                var symbolSplit = symbolName.Split(':');
                if (symbolSplit.Length == 2)
                {
                    // DEX
                    var dexName = symbolSplit[0];
                    var dex = _futuresSymbolInfo[envName].SingleOrDefault(x => x.Name.Equals(dexName, StringComparison.InvariantCultureIgnoreCase));
                    if (dex == null)
                        return new CallResult<int>(new ServerError(new ErrorInfo(ErrorType.UnknownSymbol, "DEX not found")));

                    var symbol = dex.Symbols.SingleOrDefault(x => x.Name == symbolName);
                    if (symbol == null)
                        return new CallResult<int>(new ServerError(new ErrorInfo(ErrorType.UnknownSymbol, "Symbol not found")));

                    return new CallResult<int>(symbol.QuantityDecimals);
                }
                else
                {

                    var symbol = _futuresSymbolInfo[envName][0].Symbols.SingleOrDefault(x => x.Name == symbolName);
                    if (symbol == null)
                        return new CallResult<int>(new ServerError(new ErrorInfo(ErrorType.UnknownSymbol, "Symbol not found")));

                    return new CallResult<int>(symbol.QuantityDecimals);
                }
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
        public static async Task<CallResult<string>> GetSymbolNameFromExchangeNameAsync(HyperLiquidRestClient client, string id)
        {
            var envName = client.ClientOptions.Environment.Name;
            if (envName.Equals("UnitTest", StringComparison.Ordinal))
                return new CallResult<string>("HYPE");

            return await GetSymbolNameFromExchangeNameAsync(envName, id, () => UpdateSpotSymbolInfoAsync(client)).ConfigureAwait(false);
        }

        /// <summary>
        /// Get a symbol name from an exchange symbol name
        /// </summary>
        public static async Task<CallResult<string>> GetSymbolNameFromExchangeNameAsync(HyperLiquidSocketClient client, string id)
        {
            var envName = client.ClientOptions.Environment.Name;
            if (envName.Equals("UnitTest", StringComparison.Ordinal))
                return new CallResult<string>("HYPE");

            return await GetSymbolNameFromExchangeNameAsync(envName, id, () => UpdateSpotSymbolInfoAsync(client)).ConfigureAwait(false);
        }

        private static async Task<CallResult<string>> GetSymbolNameFromExchangeNameAsync(string envName, string id, Func<Task<CallResult>> spotUpdate)
        {
            var update = await spotUpdate().ConfigureAwait(false);
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
        public static async Task<CallResult<string>> GetExchangeNameFromSymbolNameAsync(HyperLiquidRestClient client, string name)
        {
            var envName = client.ClientOptions.Environment.Name;
            if (envName.Equals("UnitTest", StringComparison.Ordinal))
                return new CallResult<string>("HYPE");

            return await GetExchangeNameFromSymbolNameAsync(envName, name, () => UpdateSpotSymbolInfoAsync(client)).ConfigureAwait(false);
        }

        /// <summary>
        /// Get an exchange symbol name from a symbol name
        /// </summary>
        public static async Task<CallResult<string>> GetExchangeNameFromSymbolNameAsync(HyperLiquidSocketClient client, string name)
        {
            var envName = client.ClientOptions.Environment.Name;
            if (envName.Equals("UnitTest", StringComparison.Ordinal))
                return new CallResult<string>("HYPE");

            return await GetExchangeNameFromSymbolNameAsync(envName, name, () => UpdateSpotSymbolInfoAsync(client)).ConfigureAwait(false);
        }

        private static async Task<CallResult<string>> GetExchangeNameFromSymbolNameAsync(string envName, string name, Func<Task<CallResult>> spotUpdate)
        {
            var update = await spotUpdate().ConfigureAwait(false);
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
        /// Get asset name and id for an asset
        /// </summary>
        public static async Task<CallResult<string>> GetAssetNameAndIdAsync(HyperLiquidRestClient client, string asset)
        {
            var envName = client.ClientOptions.Environment.Name;
            if (envName.Equals("UnitTest", StringComparison.Ordinal))
                return new CallResult<string>("HYPE");

            return await GetAssetNameAndIdAsync(envName, asset, () => UpdateSpotSymbolInfoAsync(client)).ConfigureAwait(false);
        }

        /// <summary>
        /// Get asset name and id for an asset
        /// </summary>
        public static async Task<CallResult<string>> GetAssetNameAndIdAsync(HyperLiquidSocketClient client, string asset)
        {
            var envName = client.ClientOptions.Environment.Name;
            if (envName.Equals("UnitTest", StringComparison.Ordinal))
                return new CallResult<string>("HYPE");

            return await GetAssetNameAndIdAsync(envName, asset, () => UpdateSpotSymbolInfoAsync(client)).ConfigureAwait(false);
        }

        private static async Task<CallResult<string>> GetAssetNameAndIdAsync(string envName, string asset, Func<Task<CallResult>> spotUpdate)
        {
            var update = await spotUpdate().ConfigureAwait(false);
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
            return symbol.EndsWith("/USDC") || symbol.EndsWith("/USDH") || symbol.EndsWith("/USDT0") || symbol.EndsWith("/USDE");
        }
    }
}
