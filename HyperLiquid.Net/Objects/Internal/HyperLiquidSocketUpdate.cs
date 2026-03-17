using HyperLiquid.Net.Objects.Models;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Internal
{
    internal class HyperLiquidSocketUpdate<T>
    {
        [JsonPropertyName("channel")]
        public string Channel { get; set; } = string.Empty;
        [JsonPropertyName("data")]
        public T Data { get; set; } = default!;
    }
    


    internal class HyperLiquidSocketResponse<T>
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }
        [JsonPropertyName("response")]
        public HyperLiquidSocketResponseWrapper<T> Response { get; set; } = default!;
    }

    internal class HyperLiquidSocketResponseAuth<T>
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }
        [JsonPropertyName("response")]
        public HyperLiquidSocketResponseWrapperAuth<T> Response { get; set; } = default!;
    }




    internal class HyperLiquidSocketResponseWrapper<T>
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
        [JsonPropertyName("payload")]
        public HyperLiquidSocketResponseWrapper2<T> Payload { get; set; } = default!;
    }

    internal class HyperLiquidSocketResponseWrapperAuth<T>
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
        [JsonPropertyName("payload")]
        public HyperLiquidResponse<T> Payload { get; set; } = default!;
    }




    internal class HyperLiquidSocketResponseWrapper2<T>
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
        [JsonPropertyName("data")]
        public T Data { get; set; } = default!;
    }
}
