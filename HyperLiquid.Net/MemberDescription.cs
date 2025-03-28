using Newtonsoft.Json;

namespace HyperLiquid.Net
{
    internal class MemberDescription
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
    }
}