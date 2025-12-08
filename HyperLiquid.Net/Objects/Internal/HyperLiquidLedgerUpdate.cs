using HyperLiquid.Net.Objects.Models;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Internal
{
    internal class HyperLiquidLedgerUpdate
    {
        [JsonPropertyName("nonFundingLedgerUpdates")]
        public HyperLiquidAccountLedger Ledger { get; set; } = default!;
        [JsonPropertyName("user")]
        public string User { get; set; } = string.Empty;
        [JsonPropertyName("isSnapshot")]
        public bool IsSnapshot { get; set; }
    }
}
