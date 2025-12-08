using HyperLiquid.Net.Enums;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// User role info
    /// </summary>
    public record HyperLiquidUserRole
    {
        /// <summary>
        /// Role of the user
        /// </summary>
        [JsonPropertyName("role")]
        public UserRole Role { get; set; }
    }
}
