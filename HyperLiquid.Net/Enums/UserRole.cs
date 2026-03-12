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
        /// ["<c>missing</c>"] Missing
        /// </summary>
        [Map("missing")]
        Missing,
        /// <summary>
        /// ["<c>user</c>"] User
        /// </summary>
        [Map("user")]
        User,
        /// <summary>
        /// ["<c>agent</c>"] Agent
        /// </summary>
        [Map("agent")]
        Agent,
        /// <summary>
        /// ["<c>vault</c>"] Vault
        /// </summary>
        [Map("vault")]
        Vault,
        /// <summary>
        /// ["<c>subAccount</c>"] Sub account
        /// </summary>
        [Map("subAccount")]
        SubAccount
    }
}
