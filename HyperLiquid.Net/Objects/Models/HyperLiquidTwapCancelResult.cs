using HyperLiquid.Net.Converters;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    internal record HyperLiquidTwapCancelResult
    {
        [JsonPropertyName("status")]
        [JsonConverter(typeof(TwapCancelResultConverter))]
        public string Status { get; set; } = string.Empty;
    }
}