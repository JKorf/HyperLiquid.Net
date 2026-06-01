using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Questions and outcomes info
    /// </summary>
    public record HyperLiquidQuestionsAndOutcomesInfo
    {
        /// <summary>
        /// ["<c>outcomes</c>"] Outcomes
        /// </summary>
        [JsonPropertyName("outcomes")]
        public HyperLiquidOutcomeInfo[] Outcomes { get; set; } = [];
        /// <summary>
        /// ["<c>questions</c>"] Questions
        /// </summary>
        [JsonPropertyName("questions")]
        public HyperLiquidQuestion[] Questions { get; set; } = [];
    }

    /// <summary>
    /// Question
    /// </summary>
    public record HyperLiquidQuestion
    {
        /// <summary>
        /// ["<c>question</c>"] Id
        /// </summary>
        [JsonPropertyName("question")]
        public long Id { get; set; }
        /// <summary>
        /// ["<c>name</c>"] Name
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>description</c>"] Id
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>fallbackOutcome</c>"] Fallback outcome
        /// </summary>
        [JsonPropertyName("fallbackOutcome")]
        public long FallbackOutcome { get; set; }
        /// <summary>
        /// ["<c>namedOutcomes</c>"] Possible outcomes
        /// </summary>
        [JsonPropertyName("namedOutcomes")]
        public long[] NamedOutcomes { get; set; } = [];
        /// <summary>
        /// ["<c>settledNamedOutcomes</c>"] Settled outcomes
        /// </summary>
        [JsonPropertyName("settledNamedOutcomes")]
        public long[] SettledNamedOutcomes { get; set; } = [];
    }

    /// <summary>
    /// Outcome info
    /// </summary>
    public record HyperLiquidOutcomeInfo
    {
        /// <summary>
        /// ["<c>outcome</c>"] Id
        /// </summary>
        [JsonPropertyName("outcome")]
        public long Id { get; set; }
        /// <summary>
        /// ["<c>name</c>"] Name
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>description</c>"] Description
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        private Dictionary<string, string>? _descriptionDictionary;
        /// <summary>
        /// Description field mapped to dictionary
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, string> DescriptionDictionary
        {
            get
            {
                if (_descriptionDictionary != null)
                    return _descriptionDictionary;

                _descriptionDictionary = new Dictionary<string, string>();
                var metadata = Description.Split('=').Last();
                foreach (var kvp in metadata.Split('|'))
                {
                    if (string.IsNullOrEmpty(kvp))
                        continue;

                    var kvpSplit = kvp.Split(':');
                    if (kvpSplit.Length != 2)
                        continue;

                    _descriptionDictionary.Add(kvpSplit[0], kvpSplit[1]);
                }
                return _descriptionDictionary;
            }
        }
        /// <summary>
        /// ["<c>quoteToken</c>"] Quote asset
        /// </summary>
        [JsonPropertyName("quoteToken")]
        public string QuoteAsset { get; set; } = string.Empty;
        /// <summary>
        /// ["<c>sideSpecs</c>"] Side specs
        /// </summary>
        [JsonPropertyName("sideSpecs")]
        public HyperLiquidOutcomeSpec[] Specs { get; set; } = [];
    }

    /// <summary>
    /// Outcome side spec
    /// </summary>
    public record HyperLiquidOutcomeSpec
    {
        /// <summary>
        /// ["<c>name</c>"] Name
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Outcome side info
    /// </summary>
    public class HyperLiquidOutcomeSideModel
    {
        /// <summary>
        /// Outcome info
        /// </summary>
        public HyperLiquidOutcomeInfo OutcomeInfo { get; set; } = default!;
        /// <summary>
        /// Side info
        /// </summary>
        public HyperLiquidOutcomeSpec Side { get; set; } = default!;
    }
}
