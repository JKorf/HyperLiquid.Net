using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    internal record HyperLiquidUserEventUpdate
    {
        [JsonPropertyName("fills")]
        public HyperLiquidUserTrade[]? Trades { get; set; }

        [JsonPropertyName("funding")]
        public HyperLiquidUserFunding? Funding { get; set; }

        [JsonPropertyName("liquidation")]
        public HyperLiquidLiquidationUpdate? Liquidation { get; set; }

        [JsonPropertyName("nonUserCancel")]
        public HyperLiquidNonUserCancelation[]? NonUserCancelations { get; set; }
    }
}
