





using Newtonsoft.Json;
using System.Collections.Generic;

namespace HyperLiquid.Net
{
    [JsonObject(MemberSerialization.OptIn)]
    internal class TypedDataRaw 
    {
        [JsonProperty(PropertyName = "types")]
        public IDictionary<string, MemberDescription[]> Types { get; set; }

        [JsonProperty(PropertyName = "primaryType")]
        public string PrimaryType { get; set; }

        public MemberValue[] Message { get; set; }

        public MemberValue[] DomainRawValues { get; set; }
    }
}