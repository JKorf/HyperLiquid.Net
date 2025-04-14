using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    internal class HyperLiquidResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }

    internal class HyperLiquidResponse<T> : HyperLiquidResponse
    {
        [JsonPropertyName("response")]
        public HyperLiquidAuthResponse<T>? Data { get; set; }
    }

    internal class HyperLiquidAuthResponse<T>
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
        [JsonPropertyName("data")]
        public T Data { get; set; } = default!;
    }

    internal class HyperLiquidDefault
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
    }
}
