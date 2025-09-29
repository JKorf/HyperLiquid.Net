using HyperLiquid.Net.Objects.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
