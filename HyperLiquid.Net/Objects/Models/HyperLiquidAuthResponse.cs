using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    internal record HyperLiquidResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }

    internal record HyperLiquidResponse<T> : HyperLiquidResponse
    {
        [JsonPropertyName("response")]
        public HyperLiquidAuthResponse<T>? Data { get; set; }
    }

    internal record HyperLiquidAuthResponse<T>
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
        [JsonPropertyName("data")]
        public T Data { get; set; } = default!;
    }

    internal record HyperLiquidDefault
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
    }
}
