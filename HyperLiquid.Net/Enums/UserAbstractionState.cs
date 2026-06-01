using System.Text.Json.Serialization;
using CryptoExchange.Net.Attributes;
using CryptoExchange.Net.Converters.SystemTextJson;
namespace HyperLiquid.Net.Enums
{
    /// <summary>
    /// User abstraction state
    /// </summary>
    [JsonConverter(typeof(EnumConverter<UserAbstractionState>))]
    public enum UserAbstractionState
    {
        /// <summary>
        /// ["<c>unifiedAccount</c>"] Unified account
        /// </summary>
        [Map("unifiedAccount")]
        UnifiedAccount,
        /// <summary>
        /// ["<c>portfolioMargin</c>"] Portfolio margin
        /// </summary>
        [Map("portfolioMargin")]
        PortfolioMargin,
        /// <summary>
        /// ["<c>disabled</c>"] Disabled
        /// </summary>
        [Map("disabled")]
        Disabled,
        /// <summary>
        /// ["<c>default</c>"] Default
        /// </summary>
        [Map("default")]
        Default,
        /// <summary>
        /// ["<c>dexAbstraction</c>"] DEX abstraction
        /// </summary>
        [Map("dexAbstraction")]
        DexAbstraction,
    }
}
