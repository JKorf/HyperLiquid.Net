using System;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Agent info
    /// </summary>
    public record HyperLiquidUserAgent
    {
        /// <summary>
        /// ["<c>name</c>"] Name
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>address</c>"] Address
        /// </summary>
        [JsonPropertyName("address")]
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>validUntil</c>"] Name
        /// </summary>
        [JsonPropertyName("validUntil")]
        public DateTime? ValidUntil { get; set; }
    }
}
