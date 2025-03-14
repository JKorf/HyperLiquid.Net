using System.Text.Json.Serialization;
using CryptoExchange.Net.Converters.SystemTextJson;
using CryptoExchange.Net.Attributes;
namespace HyperLiquid.Net.Enums
{
    /// <summary>
    /// Direction
    /// </summary>
    [JsonConverter(typeof(EnumConverter<DepositWithdrawDirection>))]
    public enum DepositWithdrawDirection
    {
        /// <summary>
        /// Deposit
        /// </summary>
        Deposit,
        /// <summary>
        /// Withdraw
        /// </summary>
        Withdraw
    }
}
