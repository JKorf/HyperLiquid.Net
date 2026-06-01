using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HyperLiquid.Net.Objects.Models
{
    internal record HyperLiquidOutcomeUpdate
    {
        [JsonPropertyName("outcomeSettled")]
        public long? OutcomeSettled { get; set; }
        [JsonPropertyName("outcomeCreated")]
        public HyperLiquidOutcomeInfo? OutcomeCreated { get; set; }
        [JsonPropertyName("questionSettled")]
        public long? QuestionSettled { get; set; }
        [JsonPropertyName("questionUpdated")]
        public HyperLiquidQuestion? QuestionUpdated { get; set; }
    }
}
