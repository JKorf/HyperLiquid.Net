using CryptoExchange.Net.Attributes;
using CryptoExchange.Net.Converters.SystemTextJson;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Enums
{
    /// <summary>
    /// User role
    /// </summary>
    [JsonConverter(typeof(EnumConverter<UserRole>))]
    public enum UserRole
    {
        /// <summary>
        /// Missing
        /// </summary>
        [Map("missing")]
        Missing,
        /// <summary>
        /// User
        /// </summary>
        [Map("user")]
        User,
        /// <summary>
        /// Agent
        /// </summary>
        [Map("agent")]
        Agent,
        /// <summary>
        /// Vault
        /// </summary>
        [Map("vault")]
        Vault,
        /// <summary>
        /// Sub account
        /// </summary>
        [Map("subAccount")]
        SubAccount
    }
}
