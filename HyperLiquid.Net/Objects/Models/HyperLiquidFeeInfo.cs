using CryptoExchange.Net.Converters.SystemTextJson;
using System;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Fee info
    /// </summary>
    [SerializationModel]
    public record HyperLiquidFeeInfo
    {
        /// <summary>
        /// ["<c>dailyUserVlm</c>"] Daily user volume
        /// </summary>
        [JsonPropertyName("dailyUserVlm")]
        public HyperLiquidFeeInfoVolume[] DailyUserVolume { get; set; } = Array.Empty<HyperLiquidFeeInfoVolume>();
        /// <summary>
        /// ["<c>feeSchedule</c>"] Fee schedule
        /// </summary>
        [JsonPropertyName("feeSchedule")]
        public HyperLiquidFeeInfoSchedule FeeSchedule { get; set; } = null!;
        /// <summary>
        /// ["<c>userCrossRate</c>"] User cross rate
        /// </summary>
        [JsonPropertyName("userCrossRate")]
        public decimal TakerFeeRate { get; set; }
        /// <summary>
        /// ["<c>userAddRate</c>"] User add rate
        /// </summary>
        [JsonPropertyName("userAddRate")]
        public decimal MakerFeeRate { get; set; }
        /// <summary>
        /// ["<c>userSpotCrossRate</c>"] User cross rate
        /// </summary>
        [JsonPropertyName("userSpotCrossRate")]
        public decimal TakerFeeRateSpot { get; set; }
        /// <summary>
        /// ["<c>userSpotAddRate</c>"] User add rate
        /// </summary>
        [JsonPropertyName("userSpotAddRate")]
        public decimal MakerFeeRateSpot { get; set; }
        /// <summary>
        /// ["<c>activeReferralDiscount</c>"] Active referral discount
        /// </summary>
        [JsonPropertyName("activeReferralDiscount")]
        public decimal ActiveReferralDiscount { get; set; }
        /// <summary>
        /// ["<c>trial</c>"] Trial
        /// </summary>
        [JsonPropertyName("trial")]
        public string? Trial { get; set; }
        /// <summary>
        /// ["<c>feeTrialReward</c>"] Fee trial reward
        /// </summary>
        [JsonPropertyName("feeTrialReward")]
        public decimal? FeeTrialReward { get; set; }
        /// <summary>
        /// ["<c>nextTrialAvailableTimestamp</c>"] Next trial available timestamp
        /// </summary>
        [JsonPropertyName("nextTrialAvailableTimestamp")]
        public DateTime? NextTrialAvailableTimestamp { get; set; }
    }

    /// <summary>
    /// Daily volume
    /// </summary>
    [SerializationModel]
    public record HyperLiquidFeeInfoVolume
    {
        /// <summary>
        /// ["<c>date</c>"] Date
        /// </summary>
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }
        /// <summary>
        /// ["<c>userCross</c>"] User taker volume
        /// </summary>
        [JsonPropertyName("userCross")]
        public decimal UserTaker { get; set; }
        /// <summary>
        /// ["<c>userAdd</c>"] User maker volume
        /// </summary>
        [JsonPropertyName("userAdd")]
        public decimal UserMaker { get; set; }
        /// <summary>
        /// ["<c>exchange</c>"] Exchange
        /// </summary>
        [JsonPropertyName("exchange")]
        public decimal Exchange { get; set; }
    }

    /// <summary>
    /// Fee schedule
    /// </summary>
    [SerializationModel]
    public record HyperLiquidFeeInfoSchedule
    {
        /// <summary>
        /// ["<c>cross</c>"] Taker
        /// </summary>
        [JsonPropertyName("cross")]
        public decimal Taker { get; set; }
        /// <summary>
        /// ["<c>add</c>"] Maker
        /// </summary>
        [JsonPropertyName("add")]
        public decimal Maker { get; set; }
        /// <summary>
        /// ["<c>tiers</c>"] Tiers
        /// </summary>
        [JsonPropertyName("tiers")]
        public HyperLiquidFeeInfoFeeTier Tiers { get; set; } = null!;
        /// <summary>
        /// ["<c>referralDiscount</c>"] Referral discount
        /// </summary>
        [JsonPropertyName("referralDiscount")]
        public decimal ReferralDiscount { get; set; }
    }

    /// <summary>
    /// Fee tier info
    /// </summary>
    [SerializationModel]
    public record HyperLiquidFeeInfoFeeTier
    {
        /// <summary>
        /// ["<c>vip</c>"] VIP tier
        /// </summary>
        [JsonPropertyName("vip")]
        public HyperLiquidFeeInfoFeeTierRate[] VipTier { get; set; } = Array.Empty<HyperLiquidFeeInfoFeeTierRate>();
        /// <summary>
        /// ["<c>mm</c>"] Market maker tier
        /// </summary>
        [JsonPropertyName("mm")]
        public HyperLiquidFeeInfoFeeTierRateMarketMaker[] MarketMakerTier { get; set; } = Array.Empty<HyperLiquidFeeInfoFeeTierRateMarketMaker>();
    }

    /// <summary>
    /// VIP tier rates
    /// </summary>
    [SerializationModel]
    public record HyperLiquidFeeInfoFeeTierRate
    {
        /// <summary>
        /// ["<c>ntlCutoff</c>"] Notional cutoff
        /// </summary>
        [JsonPropertyName("ntlCutoff")]
        public decimal NotionalCutoff { get; set; }
        /// <summary>
        /// ["<c>cross</c>"] Taker rate
        /// </summary>
        [JsonPropertyName("cross")]
        public decimal Taker { get; set; }
        /// <summary>
        /// ["<c>add</c>"] Maker rate
        /// </summary>
        [JsonPropertyName("add")]
        public decimal Maker { get; set; }
    }

    /// <summary>
    /// Market maker rate
    /// </summary>
    [SerializationModel]
    public record HyperLiquidFeeInfoFeeTierRateMarketMaker
    {
        /// <summary>
        /// ["<c>makerFractionCutoff</c>"] Maker fraction cutoff
        /// </summary>
        [JsonPropertyName("makerFractionCutoff")]
        public decimal MakerFractionCutoff { get; set; }
        /// <summary>
        /// ["<c>add</c>"] Maker rate
        /// </summary>
        [JsonPropertyName("add")]
        public decimal Maker { get; set; }
    }


}
