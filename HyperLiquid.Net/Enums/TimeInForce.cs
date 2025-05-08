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
        /// Post only
        /// </summary>
        [Map("Alo")]
        PostOnly,
        /// <summary>
        /// Immediate or cancel
        /// </summary>
        [Map("Ioc")]
        ImmediateOrCancel,
        /// <summary>
        /// Good till canceled
        /// </summary>
        [Map("Gtc")]
        GoodTillCanceled,
        /// <summary>
        /// Frontend market
        /// </summary>
        [Map("FrontendMarket")]
        FrontendMarket,
        /// <summary>
        /// Liquidation market
        /// </summary>
        [Map("LiquidationMarket")]
        LiquidationMarket
    }
}
