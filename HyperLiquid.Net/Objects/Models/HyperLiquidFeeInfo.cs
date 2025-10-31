using CryptoExchange.Net.Converters.SystemTextJson;
using System;
using System.Collections.Generic;
using System.Text;
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
        /// Daily user volume
        /// </summary>
        [JsonPropertyName("dailyUserVlm")]
        public HyperLiquidFeeInfoVolume[] DailyUserVolume { get; set; } = Array.Empty<HyperLiquidFeeInfoVolume>();
        /// <summary>
        /// Fee schedule
        /// </summary>
        [JsonPropertyName("feeSchedule")]
        public HyperLiquidFeeInfoSchedule FeeSchedule { get; set; } = null!;
        /// <summary>
        /// User cross rate
        /// </summary>
        [JsonPropertyName("userCrossRate")]
        public decimal TakerFeeRate { get; set; }
        /// <summary>
        /// User add rate
        /// </summary>
        [JsonPropertyName("userAddRate")]
        public decimal MakerFeeRate { get; set; }
        /// <summary>
        /// User cross rate
        /// </summary>
        [JsonPropertyName("userSpotCrossRate")]
        public decimal TakerFeeRateSpot { get; set; }
        /// <summary>
        /// User add rate
        /// </summary>
        [JsonPropertyName("userSpotAddRate")]
        public decimal MakerFeeRateSpot { get; set; }
        /// <summary>
        /// Active referral discount
        /// </summary>
        [JsonPropertyName("activeReferralDiscount")]
        public decimal ActiveReferralDiscount { get; set; }
        /// <summary>
        /// Trial
        /// </summary>
        [JsonPropertyName("trial")]
        public string? Trial { get; set; }
        /// <summary>
        /// Fee trial reward
        /// </summary>
        [JsonPropertyName("feeTrialReward")]
        public decimal? FeeTrialReward { get; set; }
        /// <summary>
        /// Next trial available timestamp
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
        /// Date
        /// </summary>
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }
        /// <summary>
        /// User taker volume
        /// </summary>
        [JsonPropertyName("userCross")]
        public decimal UserTaker { get; set; }
        /// <summary>
        /// User maker volume
        /// </summary>
        [JsonPropertyName("userAdd")]
        public decimal UserMaker { get; set; }
        /// <summary>
        /// Exchange
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
        /// Taker
        /// </summary>
        [JsonPropertyName("cross")]
        public decimal Taker { get; set; }
        /// <summary>
        /// Maker
        /// </summary>
        [JsonPropertyName("add")]
        public decimal Maker { get; set; }
        /// <summary>
        /// Tiers
        /// </summary>
        [JsonPropertyName("tiers")]
        public HyperLiquidFeeInfoFeeTier Tiers { get; set; } = null!;
        /// <summary>
        /// Referral discount
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
        /// VIP tier
        /// </summary>
        [JsonPropertyName("vip")]
        public HyperLiquidFeeInfoFeeTierRate[] VipTier { get; set; } = Array.Empty<HyperLiquidFeeInfoFeeTierRate>();
        /// <summary>
        /// Market maker tier
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
        /// Notional cutoff
        /// </summary>
        [JsonPropertyName("ntlCutoff")]
        public decimal NotionalCutoff { get; set; }
        /// <summary>
        /// Taker rate
        /// </summary>
        [JsonPropertyName("cross")]
        public decimal Taker { get; set; }
        /// <summary>
        /// Maker rate
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
        /// Maker fraction cutoff
        /// </summary>
        [JsonPropertyName("makerFractionCutoff")]
        public decimal MakerFractionCutoff { get; set; }
        /// <summary>
        /// Maker rate
        /// </summary>
        [JsonPropertyName("add")]
        public decimal Maker { get; set; }
    }


}
