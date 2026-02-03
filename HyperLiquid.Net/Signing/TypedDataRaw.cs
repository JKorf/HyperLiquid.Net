using CryptoExchange.Net.Converters.SystemTextJson;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using HyperLiquid.Net.Converters;

namespace HyperLiquid.Net.Signing
{
    [SerializationModel]
    [JsonConverter(typeof(TypedDataRawConverter))]
    internal class TypedDataRaw 
    {
        [JsonPropertyName("types")]
        public IDictionary<string, MemberDescription[]> Types { get; set; } = new Dictionary<string, MemberDescription[]>();
        [JsonPropertyName("primaryType")]
        public string PrimaryType { get; set; } = string.Empty;
        [JsonPropertyName("domain")]
        public MemberValue[] DomainRawValues { get; set; } = [];
        [JsonPropertyName("message")]
        public MemberValue[] Message { get; set; } = [];
    }
}