using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Non user order cancellation
    /// </summary>
    public record HyperLiquidNonUserCancelation
    {
        /// <summary>
        /// ["<c>coin</c>"] Symbol
        /// </summary>
        [JsonPropertyName("coin")]
        public string Symbol { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>oid</c>"] Order id
        /// </summary>
        [JsonPropertyName("oid")]
        public long OrderId { get; set; }
    }
}
