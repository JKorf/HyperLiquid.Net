using System.Text.Json.Serialization;
using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Attributes;

namespace HyperLiquid.Net.Enums
{
    /// <summary>
    /// TakeProfit/StopLoss grouping
    /// </summary>
    [JsonConverter(typeof(EnumConverter<TpSlGrouping>))]
    public enum TpSlGrouping
    {
        /// <summary>
        /// ["<c>normalTpsl</c>"] Normal TakeProfit/StopLoss
        /// </summary>
        [Map("normalTpsl")]
        NormalTpSl,
        /// <summary>
        /// ["<c>positionTpsl</c>"] Position TakeProfit/StopLoss
        /// </summary>
        [Map("positionTpsl")]
        PositionTpSl
    }
}
