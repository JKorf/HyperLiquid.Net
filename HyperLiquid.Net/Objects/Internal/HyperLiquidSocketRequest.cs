using CryptoExchange.Net.Objects;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Internal
{
    internal class HyperLiquidSocketRequest
    {
        [JsonPropertyName("method")]
        public string Method { get; set; } = string.Empty;
    }

    internal class HyperLiquidRequest : HyperLiquidSocketRequest
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }
        [JsonPropertyName("request")]
        public HyperLiquidRequestWrapper Request { get; set; } = default!;
    }

    internal class HyperLiquidRequestWrapper
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
        [JsonPropertyName("payload")]
        public ParameterCollection Payload { get; set; } = default!;
    }

    internal class HyperLiquidSubscribeRequest: HyperLiquidSocketRequest
    {
        public HyperLiquidSubscribeRequest()
        {
            Method = "subscribe";
        }

        [JsonPropertyName("subscription")]
        public Dictionary<string, object> Subscription { get; set; } = default!;
    }

    internal class HyperLiquidUnsubscribeRequest : HyperLiquidSocketRequest
    {
        public HyperLiquidUnsubscribeRequest()
        {
            Method = "unsubscribe";
        }

        [JsonPropertyName("subscription")]
        public Dictionary<string, object> Subscription { get; set; } = default!;
    }
}
