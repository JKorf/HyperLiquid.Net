using CryptoExchange.Net.Converters.SystemTextJson;
using HyperLiquid.Net.Converters;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Cancel result
    /// </summary>
    [SerializationModel]
    internal record HyperLiquidCancelResult
    {
        [JsonPropertyName("statuses")]
        [JsonConverter(typeof(CancelResultConverter))]
        public string[] Statuses { get; set; } = [];
    }
}
