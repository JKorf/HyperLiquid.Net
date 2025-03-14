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
        /// 1 min
        /// </summary>
        [Map("1m")]
        OneMinute = 60,
        /// <summary>
        /// 3 min
        /// </summary>
        [Map("3m")]
        ThreeMinutes = 60 * 3,
        /// <summary>
        /// 5 min
        /// </summary>
        [Map("5m")]
        FiveMinutes = 60 * 5,
        /// <summary>
        /// 15 min
        /// </summary>
        [Map("15m")]
        FifteenMinutes = 60 * 15,
        /// <summary>
        /// 30 min
        /// </summary>
        [Map("30m")]
        ThirtyMinutes = 60 * 30,
        /// <summary>
        /// 1 hour
        /// </summary>
        [Map("1h")]
        OneHour = 60 * 60,
        /// <summary>
        /// 2 hour
        /// </summary>
        [Map("2h")]
        TwoHours = 60 * 60 * 2,
        /// <summary>
        /// 4 hour
        /// </summary>
        [Map("4h")]
        FourHours = 60 * 60 * 4,
        /// <summary>
        /// 8 hour
        /// </summary>
        [Map("8h")]
        EightHours = 60 * 60 * 8,
        /// <summary>
        /// 12 hour
        /// </summary>
        [Map("12h")]
        TwelveHours = 60 * 60 * 12,
        /// <summary>
        /// 1 day
        /// </summary>
        [Map("1d")]
        OneDay = 60 * 60 * 24,
        /// <summary>
        /// 3 day
        /// </summary>
        [Map("3d")]
        ThreeDays = 60 * 60 * 24 * 3,
        /// <summary>
        /// 1 week
        /// </summary>
        [Map("1w")]
        OneWeek = 60 * 60 * 24 * 7,
        /// <summary>
        /// 1 month
        /// </summary>
        [Map("1M")]
        OneMonth = 60 * 60 * 24 * 30,
    }
}
