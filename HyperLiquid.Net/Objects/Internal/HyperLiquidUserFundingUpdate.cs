using HyperLiquid.Net.Objects.Models;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Internal
{
    internal class HyperLiquidUserFundingUpdate
    {
        [JsonPropertyName("fundings")]
        public HyperLiquidUserFunding[] Fundings { get; set; } = [];
        [JsonPropertyName("user")]
        public string User { get; set; } = string.Empty;
        [JsonPropertyName("isSnapshot")]
        public bool IsSnapshot { get; set; }
    }
}
