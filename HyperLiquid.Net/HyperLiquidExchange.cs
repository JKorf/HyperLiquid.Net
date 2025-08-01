using CryptoExchange.Net.Objects;
using CryptoExchange.Net.RateLimiting.Filters;
using CryptoExchange.Net.RateLimiting.Guards;
using CryptoExchange.Net.RateLimiting.Interfaces;
using CryptoExchange.Net.RateLimiting;
using System;
using CryptoExchange.Net.SharedApis;
using System.Text.Json.Serialization;
using HyperLiquid.Net.Converters;
using CryptoExchange.Net.Converters;
using System.Collections.Generic;
using System.Text.Json;
using CryptoExchange.Net.Converters.SystemTextJson;

namespace HyperLiquid.Net
{
    /// <summary>
    /// HyperLiquid exchange information and configuration
    /// </summary>
    public static class HyperLiquidExchange
    {
        /// <summary>
        /// Exchange name
        /// </summary>
        public static string ExchangeName => "HyperLiquid";

        /// <summary>
        /// Exchange name
        /// </summary>
        public static string DisplayName => "HyperLiquid";

        /// <summary>
        /// Url to exchange image
        /// </summary>
        public static string ImageUrl { get; } = "https://raw.githubusercontent.com/JKorf/HyperLiquid.Net/master/HyperLiquid.Net/Icon/icon.png";

        /// <summary>
        /// Url to the main website
        /// </summary>
        public static string Url { get; } = "https://app.hyperliquid.xyz/";

        /// <summary>
        /// Urls to the API documentation
        /// </summary>
        public static string[] ApiDocsUrl { get; } = new[] {
            "https://hyperliquid.gitbook.io/hyperliquid-docs"
            };

        /// <summary>
        /// Type of exchange
        /// </summary>
        public static ExchangeType Type { get; } = ExchangeType.DEX;

        /// <summary>
        /// Address of the builder
        /// </summary>
        public static string BuilderAddress => "0x64134a9577A857BcC5dAfa42E1647E1439e5F8E7".ToLower();

        /// <summary>
        /// Aliases for HyperLiquid assets
        /// </summary>
        public static AssetAliasConfiguration AssetAliases { get; } = new AssetAliasConfiguration
        {
            Aliases = 
            [
                new AssetAlias("UBTC", "BTC"),
                new AssetAlias("UETH", "ETH")
            ]
        };

        /// <summary>
        /// Delegate for signing a request. Should only be overridden in situations where default signing doesn't work, for example on OSX where the default signing method isn't supported.<br/>
        /// First parameter is the request string to sign, second parameter is the secret to sign it with.
        /// </summary>
        public static Func<string, string, Dictionary<string, object>>? SignRequestDelegate { get; set; }

        internal static JsonSerializerOptions _serializerContext = SerializerOptions.WithConverters(JsonSerializerContextCache.GetOrCreate<HyperLiquidSourceGenerationContext>());

        /// <summary>
        /// Format a base and quote asset to an HyperLiquid recognized symbol 
        /// </summary>
        /// <param name="baseAsset">Base asset</param>
        /// <param name="quoteAsset">Quote asset</param>
        /// <param name="tradingMode">Trading mode</param>
        /// <param name="deliverTime">Delivery time for delivery futures</param>
        /// <returns></returns>
        public static string FormatSymbol(string baseAsset, string quoteAsset, TradingMode tradingMode, DateTime? deliverTime = null)
        {
            if (tradingMode == TradingMode.Spot)
            {
                baseAsset = AssetAliases.CommonToExchangeName(baseAsset);
                quoteAsset = AssetAliases.CommonToExchangeName(quoteAsset);
            }

            if (tradingMode == TradingMode.Spot)
                return baseAsset + "/" + quoteAsset;

            return baseAsset;
        }

        /// <summary>
        /// Rate limiter configuration for the HyperLiquid API
        /// </summary>
        public static HyperLiquidRateLimiters RateLimiter { get; } = new HyperLiquidRateLimiters();
    }

    /// <summary>
    /// Rate limiter configuration for the HyperLiquid API
    /// </summary>
    public class HyperLiquidRateLimiters
    {
        /// <summary>
        /// Event for when a rate limit is triggered
        /// </summary>
        public event Action<RateLimitEvent> RateLimitTriggered;

        /// <summary>
        /// Event when the rate limit is updated. Note that it's only updated when a request is send, so there are no specific updates when the current usage is decaying.
        /// </summary>
        public event Action<RateLimitUpdateEvent> RateLimitUpdated;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal HyperLiquidRateLimiters()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            Initialize();
        }

        private void Initialize()
        {
            HyperLiquidRest = new RateLimitGate("HyperLiquid REST")
                .AddGuard(new RateLimitGuard(RateLimitGuard.PerHost, [], 1200, TimeSpan.FromSeconds(60), RateLimitWindowType.Sliding)); // Limit of 1200 weight per minute
            HyperLiquidSocket = new RateLimitGate("HyperLiquid WebSocket")
                .AddGuard(new RateLimitGuard(RateLimitGuard.PerHost, new LimitItemTypeFilter(RateLimitItemType.Request), 2000, TimeSpan.FromSeconds(60), RateLimitWindowType.Sliding)); // Limit of 2000 weight per minute
            HyperLiquidRest.RateLimitTriggered += (x) => RateLimitTriggered?.Invoke(x);
            HyperLiquidRest.RateLimitUpdated += (x) => RateLimitUpdated?.Invoke(x);
            HyperLiquidSocket.RateLimitTriggered += (x) => RateLimitTriggered?.Invoke(x);
            HyperLiquidSocket.RateLimitUpdated += (x) => RateLimitUpdated?.Invoke(x);
        }


        internal IRateLimitGate HyperLiquidRest { get; private set; }
        internal IRateLimitGate HyperLiquidSocket { get; private set; }

    }
}
