using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HyperLiquid.Net.Objects.Models
{
    /// <summary>
    /// Settled outcome info
    /// </summary>
    public record HyperLiquidSettledOutcome
    {
        /// <summary>
        /// ["<c>spec</c>"] Specs
        /// </summary>
        [JsonPropertyName("spec")]
        public HyperLiquidOutcomeInfo Spec { get; set; } = default!;
        /// <summary>
        /// ["<c>settleFraction</c>"] Settle fraction
        /// </summary>
        [JsonPropertyName("settleFraction")]
        public decimal SettleFraction { get; set; }
        /// <summary>
        /// ["<c>details</c>"] Details
        /// </summary>
        [JsonPropertyName("details")]
        public string Details { get; set; } = string.Empty;

        private Dictionary<string, string>? _detailsDictionary;
        /// <summary>
        /// Details field mapped to dictionary
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, string> DetailsDictionary
        {
            get
            {
                if (_detailsDictionary != null)
                    return _detailsDictionary;

                _detailsDictionary = new Dictionary<string, string>();
                foreach(var kvp in Details.Split('|'))
                {
                    if (string.IsNullOrEmpty(kvp))
                        continue;

                    var kvpSplit = kvp.Split(':');
                    if (kvpSplit.Length != 2)
                        continue;

                    _detailsDictionary.Add(kvpSplit[0], kvpSplit[1]);
                }
                return _detailsDictionary;
            }
        }
    }
}
