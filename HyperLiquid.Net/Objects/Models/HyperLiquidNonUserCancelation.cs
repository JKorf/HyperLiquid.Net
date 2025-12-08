using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Non user order cancellation
    /// </summary>
    public record HyperLiquidNonUserCancelation
    {
        /// <summary>
        /// Symbol
        /// </summary>
        [JsonPropertyName("coin")]
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// Order id
        /// </summary>
        [JsonPropertyName("oid")]
        public long OrderId { get; set; }
    }
}
