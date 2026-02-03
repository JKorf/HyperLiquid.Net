using CryptoExchange.Net.Converters;
using CryptoExchange.Net.Converters.SystemTextJson;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Referral information
    /// </summary>
    [SerializationModel]
    public record HyperliquidReferralInfo
    {
        /// <summary>
        /// Referred by information
        /// </summary>
        [JsonPropertyName("referredBy")]
        public HyperliquidReferredBy? ReferredBy { get; set; }

        /// <summary>
        /// Cumulative volume (USDC only)
        /// </summary>
        [JsonPropertyName("cumVlm")]
        public decimal CumulativeVolume { get; set; }

        /// <summary>
        /// Unclaimed rewards (USDC only)
        /// </summary>
        [JsonPropertyName("unclaimedRewards")]
        public decimal UnclaimedRewards { get; set; }

        /// <summary>
        /// Claimed rewards (USDC only)
        /// </summary>
        [JsonPropertyName("claimedRewards")]
        public decimal ClaimedRewards { get; set; }

        /// <summary>
        /// Builder rewards (USDC only)
        /// </summary>
        [JsonPropertyName("builderRewards")]
        public decimal BuilderRewards { get; set; }

        /// <summary>
        /// Token to state mapping
        /// </summary>
        [JsonPropertyName("tokenToState")]
        public HyperliquidTokenState? TokenToState { get; set; }

        /// <summary>
        /// Referrer state information
        /// </summary>
        [JsonPropertyName("referrerState")]
        public HyperliquidReferrerState? ReferrerState { get; set; }

        /// <summary>
        /// Reward history
        /// </summary>
        [JsonPropertyName("rewardHistory")]
        public object[] RewardHistory { get; set; } = [];
    }

    /// <summary>
    /// Referred by information
    /// </summary>
    [SerializationModel]
    public record HyperliquidReferredBy
    {
        /// <summary>
        /// Referrer address
        /// </summary>
        [JsonPropertyName("referrer")]
        public string Referrer { get; set; } = string.Empty;

        /// <summary>
        /// Referral code
        /// </summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;
    }

    /// <summary>
    /// Token state mapping (token ID and state data)
    /// </summary>
    [JsonConverter(typeof(ArrayConverter<HyperliquidTokenState>))]
    public record HyperliquidTokenState
    {
        /// <summary>
        /// Token ID
        /// </summary>
        [ArrayProperty(0)]
        public int TokenId { get; set; }

        /// <summary>
        /// State data
        /// </summary>
        [ArrayProperty(1)]
        public HyperliquidTokenStateData StateData { get; set; } = default!;
    }

    /// <summary>
    /// Token state data
    /// </summary>
    [SerializationModel]
    public record HyperliquidTokenStateData
    {
        /// <summary>
        /// Cumulative volume
        /// </summary>
        [JsonPropertyName("cumVlm")]
        public decimal CumulativeVolume { get; set; }

        /// <summary>
        /// Unclaimed rewards
        /// </summary>
        [JsonPropertyName("unclaimedRewards")]
        public decimal UnclaimedRewards { get; set; }

        /// <summary>
        /// Claimed rewards
        /// </summary>
        [JsonPropertyName("claimedRewards")]
        public decimal ClaimedRewards { get; set; }

        /// <summary>
        /// Builder rewards
        /// </summary>
        [JsonPropertyName("builderRewards")]
        public decimal BuilderRewards { get; set; }
    }

    /// <summary>
    /// Referrer state information
    /// </summary>
    [SerializationModel]
    public record HyperliquidReferrerState
    {
        /// <summary>
        /// Stage
        /// </summary>
        [JsonPropertyName("stage")]
        public string Stage { get; set; } = string.Empty;

        /// <summary>
        /// Data
        /// </summary>
        [JsonPropertyName("data")]
        public HyperliquidReferrerStateData? Data { get; set; }
    }

    /// <summary>
    /// Referrer state data
    /// </summary>
    [SerializationModel]
    public record HyperliquidReferrerStateData
    {
        /// <summary>
        /// Referral code
        /// </summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Referral states
        /// </summary>
        [JsonPropertyName("referralStates")]
        public HyperliquidReferralState[] ReferralStates { get; set; } = [];
    }

    /// <summary>
    /// Individual referral state
    /// </summary>
    [SerializationModel]
    public record HyperliquidReferralState
    {
        /// <summary>
        /// Cumulative volume
        /// </summary>
        [JsonPropertyName("cumVlm")]
        public decimal CumulativeVolume { get; set; }

        /// <summary>
        /// Cumulative rewarded fees since referred
        /// </summary>
        [JsonPropertyName("cumRewardedFeesSinceReferred")]
        public decimal CumulativeRewardedFeesSinceReferred { get; set; }

        /// <summary>
        /// Cumulative fees rewarded to referrer
        /// </summary>
        [JsonPropertyName("cumFeesRewardedToReferrer")]
        public decimal CumulativeFeesRewardedToReferrer { get; set; }

        /// <summary>
        /// Time joined (milliseconds timestamp)
        /// </summary>
        [JsonPropertyName("timeJoined")]
        public long TimeJoined { get; set; }

        /// <summary>
        /// User address
        /// </summary>
        [JsonPropertyName("user")]
        public string User { get; set; } = string.Empty;
    }
}
