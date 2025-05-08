using System.Text.Json.Serialization;
using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Attributes;
namespace HyperLiquid.Net.Enums
{
    /// <summary>
    /// Transfer direction
    /// </summary>
    [JsonConverter(typeof(EnumConverter<TransferDirection>))]
    public enum TransferDirection
    {
        /// <summary>
        /// Spot to futures
        /// </summary>
        SpotToFutures,
        /// <summary>
        /// Futures to spot
        /// </summary>
        FuturesToSpot
    }
}
