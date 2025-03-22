﻿using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Spot exchange info
    /// </summary>
    public record HyperLiquidSpotExchangeInfo
    {
        [JsonInclude, JsonPropertyName("universe")]
        private IEnumerable<HyperLiquidSymbolReference> SymbolsInt { get; set; } = [];
        private IEnumerable<HyperLiquidSymbol>? _symbols;

        /// <summary>
        /// Assets
        /// </summary>
        [JsonPropertyName("tokens")]
        public IEnumerable<HyperLiquidAsset> Assets { get; set; } = [];

        /// <summary>
        /// Symbol info
        /// </summary>
        [JsonIgnore]
        public IEnumerable<HyperLiquidSymbol> Symbols
        {
            get
            {
                if (_symbols == null)
                {
                    _symbols = SymbolsInt.Select(x => {
                        var baseAsset = Assets.ElementAt(x.BaseAssetIndex);
                        var quoteAsset = Assets.ElementAt(x.QuoteAssetIndex);
                        return new HyperLiquidSymbol
                        {
                            Index = x.Index,
                            IsCanonical = x.IsCanonical,
                            Name = baseAsset.Name + "/" + quoteAsset.Name,
                            ExchangeName = x.Name,
                            BaseAsset = baseAsset,
                            QuoteAsset = quoteAsset,
                        };
                        }
                    ).ToList();
                }

                return _symbols;
            }
        }
    }

    /// <summary>
    /// Symbol info
    /// </summary>
    public record HyperLiquidSymbol
    {
        /// <summary>
        /// Symbol name, generated by combining the base and quote asset
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Symbol name, as returned by the API
        /// </summary>
        public string ExchangeName { get; set; } = string.Empty;
        /// <summary>
        /// The base asset
        /// </summary>
        public HyperLiquidAsset BaseAsset { get; set; } = default!;
        /// <summary>
        /// The quote asset
        /// </summary>
        public HyperLiquidAsset QuoteAsset { get; set; } = default!;
        /// <summary>
        /// Asset index
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// Is canonical
        /// </summary>
        public bool IsCanonical { get; set; }
    }

    /// <summary>
    /// Asset info
    /// </summary>
    public record HyperLiquidAsset
    {
        /// <summary>
        /// Asset name
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Decimal places for quantities
        /// </summary>
        [JsonPropertyName("szDecimals")]
        public int QuantityDecimals { get; set; }
        /// <summary>
        /// Decimal prices for prices
        /// </summary>
        [JsonPropertyName("weiDecimals")]
        public int PriceDecimals { get; set; }
        /// <summary>
        /// Asset index
        /// </summary>
        [JsonPropertyName("index")]
        public int Index { get; set; }
        /// <summary>
        /// Asset id
        /// </summary>
        [JsonPropertyName("tokenId")]
        public string AssetId { get; set; } = string.Empty;
        /// <summary>
        /// Is canonical
        /// </summary>
        [JsonPropertyName("isCanonical")]
        public bool IsCanonical { get; set; }
        /// <summary>
        /// EVM contract
        /// </summary>
        [JsonPropertyName("evmContract")]
        public HyperLiquidEvmContract? EvmContract { get; set; }
        /// <summary>
        /// Full asset name
        /// </summary>
        [JsonPropertyName("fullName")]
        public string FullName { get; set; } = string.Empty;
    }

    /// <summary>
    /// EVM contract  info
    /// </summary>
    public record HyperLiquidEvmContract
    {
        /// <summary>
        /// Address
        /// </summary>
        [JsonPropertyName("address")]
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// Extra decimals
        /// </summary>
        [JsonPropertyName("evm_extra_wei_decimals")]
        public int? EvmExtraWeiDecimals { get; set; }
    }

    internal record HyperLiquidSymbolReference
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        [JsonInclude, JsonPropertyName("tokens")]
        private int[] Assets { get; set; } = [];
        [JsonIgnore]
        public int BaseAssetIndex => Assets[0];
        [JsonIgnore]
        public int QuoteAssetIndex => Assets[1];

        [JsonPropertyName("index")]
        public int Index { get; set; }
        [JsonPropertyName("isCanonical")]
        public bool IsCanonical { get; set; }
    }
}
