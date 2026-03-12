using System.Text.Json.Serialization;
using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Attributes;

namespace HyperLiquid.Net.Enums
{
    /// <summary>
    /// Time in force
    /// </summary>
    [JsonConverter(typeof(EnumConverter<TimeInForce>))]
    public enum TimeInForce
    {
        /// <summary>
        /// ["<c>Alo</c>"] Post only
        /// </summary>
        [Map("Alo")]
        PostOnly,
        /// <summary>
        /// ["<c>Ioc</c>"] Immediate or cancel
        /// </summary>
        [Map("Ioc")]
        ImmediateOrCancel,
        /// <summary>
        /// ["<c>Gtc</c>"] Good till canceled
        /// </summary>
        [Map("Gtc")]
        GoodTillCanceled,
        /// <summary>
        /// ["<c>FrontendMarket</c>"] Frontend market
        /// </summary>
        [Map("FrontendMarket")]
        FrontendMarket,
        /// <summary>
        /// ["<c>LiquidationMarket</c>"] Liquidation market
        /// </summary>
        [Map("LiquidationMarket")]
        LiquidationMarket
    }
}
