using HyperLiquid.Net.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
