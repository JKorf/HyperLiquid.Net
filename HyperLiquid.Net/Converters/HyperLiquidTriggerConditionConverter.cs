using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using HyperLiquid.Net.Enums;
using HyperLiquid.Net.Objects.Models;

namespace HyperLiquid.Net.Converters
{
    /// <summary>
    /// Parses the string Hyperliquid provides for a Trigger, being one of:
    ///     `null`
    ///     "N/A"
    ///     "Triggered"
    ///     "Price above 100.123"
    ///     "Price below 100.123"
    /// </summary>
    public class HyperLiquidTriggerConditionConverter : JsonConverter<HyperLiquidTriggerCondition?>
    {
        /// <summary>
        /// Read HyperLiquidTriggerCondition
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public override HyperLiquidTriggerCondition? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString();

#if NETSTANDARD2_0
            if (str == null)
                return null;

            if (str == "N/A")
                return new HyperLiquidTriggerCondition();

            if (str == "Triggered")
                return new HyperLiquidTriggerCondition { Triggered = true };

            var parts = str.Split(' ');
            if (parts.Length == 3 && parts[0] == "Price")
            {
                if (parts[1] == "below")
                    return new HyperLiquidTriggerCondition(
                        TriggerDirection.Below,
                        decimal.Parse(parts[2]));

                if (parts[1] == "above")
                    return new HyperLiquidTriggerCondition(
                        TriggerDirection.Above,
                        decimal.Parse(parts[2]));
            }

            throw new InvalidOperationException(
                "Cannot deserialize HyperLiquidTriggerCondition, contents: " + str);
#else
            return str switch
            {
                null => null,
                "N/A" => new HyperLiquidTriggerCondition(),
                "Triggered" => new HyperLiquidTriggerCondition { Triggered = true },
                _ => str.Split(' ') switch
                {
                    ["Price", "below", var belowPrice] => new HyperLiquidTriggerCondition(TriggerDirection.Below, decimal.Parse(belowPrice)),
                    ["Price", "above", var abovePrice] => new HyperLiquidTriggerCondition(TriggerDirection.Above, decimal.Parse(abovePrice)),
                    _ => throw new InvalidOperationException("Cannot deserialize HyperLiquidTriggerCondition, contents: " + str)
                }
            };
#endif
        }

        /// <summary>
        /// Write HyperLiquidTriggerCondition
        /// </summary>
        public override void Write(Utf8JsonWriter writer, HyperLiquidTriggerCondition? value, JsonSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();
            }
            else if (value is { Active: true, Direction: not null, Price: not null, Triggered: false })
            {
                writer.WriteStringValue($"Price {value.Direction.Value.ToString().ToLowerInvariant()} {value.Price}");
            }
            else if (value is { Triggered: true })
            {
                writer.WriteStringValue("Triggered");
            }
            else
            {
                writer.WriteStringValue("N/A");
            }
        }
    }
}
