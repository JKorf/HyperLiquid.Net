using System.Text.Json.Serialization;
using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Attributes;

namespace HyperLiquid.Net.Enums
{
    /// <summary>
    /// Interval
    /// </summary>
    [JsonConverter(typeof(EnumConverter<KlineInterval>))]
    public enum KlineInterval
    {
        /// <summary>
        /// ["<c>1m</c>"] 1 min
        /// </summary>
        [Map("1m")]
        OneMinute = 60,
        /// <summary>
        /// ["<c>3m</c>"] 3 min
        /// </summary>
        [Map("3m")]
        ThreeMinutes = 60 * 3,
        /// <summary>
        /// ["<c>5m</c>"] 5 min
        /// </summary>
        [Map("5m")]
        FiveMinutes = 60 * 5,
        /// <summary>
        /// ["<c>15m</c>"] 15 min
        /// </summary>
        [Map("15m")]
        FifteenMinutes = 60 * 15,
        /// <summary>
        /// ["<c>30m</c>"] 30 min
        /// </summary>
        [Map("30m")]
        ThirtyMinutes = 60 * 30,
        /// <summary>
        /// ["<c>1h</c>"] 1 hour
        /// </summary>
        [Map("1h")]
        OneHour = 60 * 60,
        /// <summary>
        /// ["<c>2h</c>"] 2 hour
        /// </summary>
        [Map("2h")]
        TwoHours = 60 * 60 * 2,
        /// <summary>
        /// ["<c>4h</c>"] 4 hour
        /// </summary>
        [Map("4h")]
        FourHours = 60 * 60 * 4,
        /// <summary>
        /// ["<c>8h</c>"] 8 hour
        /// </summary>
        [Map("8h")]
        EightHours = 60 * 60 * 8,
        /// <summary>
        /// ["<c>12h</c>"] 12 hour
        /// </summary>
        [Map("12h")]
        TwelveHours = 60 * 60 * 12,
        /// <summary>
        /// ["<c>1d</c>"] 1 day
        /// </summary>
        [Map("1d")]
        OneDay = 60 * 60 * 24,
        /// <summary>
        /// ["<c>3d</c>"] 3 day
        /// </summary>
        [Map("3d")]
        ThreeDays = 60 * 60 * 24 * 3,
        /// <summary>
        /// ["<c>1w</c>"] 1 week
        /// </summary>
        [Map("1w")]
        OneWeek = 60 * 60 * 24 * 7,
        /// <summary>
        /// ["<c>1M</c>"] 1 month
        /// </summary>
        [Map("1M")]
        OneMonth = 60 * 60 * 24 * 30,
    }
}
