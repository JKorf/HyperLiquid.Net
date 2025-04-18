using HyperLiquid.Net.Objects.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace HyperLiquid.Net.Converters
{
    internal class OrderResultConverter : JsonConverter<HyperLiquidOrderResultInt[]>
    {
        public override HyperLiquidOrderResultInt[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var result = new List<HyperLiquidOrderResultInt>();
            var document = JsonDocument.ParseValue(ref reader);
            foreach (var item in document.RootElement.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    var val = item.GetString();
                    if (val == "waitingForTrigger")
                        result.Add(new HyperLiquidOrderResultInt { WaitingForTrigger = new HyperLiquidOrderResult() });
                    else if (val == "waitingForFill")
                        result.Add(new HyperLiquidOrderResultInt { WaitingForFill = new HyperLiquidOrderResult() });
                    else
                        result.Add(new HyperLiquidOrderResultInt { Error = val });
                }
                else
                {
                    if (item.TryGetProperty("resting", out var restingProp))
                    {
                        var desResult = restingProp.Deserialize<HyperLiquidOrderResult>((JsonTypeInfo<HyperLiquidOrderResult>)options.GetTypeInfo(typeof(HyperLiquidOrderResult)));
                        result.Add(new HyperLiquidOrderResultInt { ResultResting = desResult });
                    }
                    else if(item.TryGetProperty("filled", out var filledProp))
                    {
                        var desResult = filledProp.Deserialize<HyperLiquidOrderResult>((JsonTypeInfo<HyperLiquidOrderResult>)options.GetTypeInfo(typeof(HyperLiquidOrderResult)));
                        result.Add(new HyperLiquidOrderResultInt { ResultFilled = desResult });
                    }
                    else if(item.TryGetProperty("error", out var errorProp))
                    {
                        result.Add(new HyperLiquidOrderResultInt { Error = errorProp.GetString() });
                    }
                }
            }

            return result.ToArray();
        }

        public override void Write(Utf8JsonWriter writer, HyperLiquidOrderResultInt[] value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var item in value)
            {
                if (item.Error != null) 
                {
                    writer.WriteStartObject();
                    writer.WriteString("error", item.Error);
                    writer.WriteEndObject();
                }
                else if(item.WaitingForTrigger != null)
                {
                    writer.WriteStringValue("waitingForTrigger");
                }
                else if(item.ResultResting != null)
                {
                    writer.WritePropertyName("resting");
                    writer.WriteStartObject();
                    writer.WriteNumber("oid", item.ResultResting.OrderId);
                    writer.WriteEndObject();
                }
                else if(item.ResultFilled != null)
                {
                    writer.WritePropertyName("filled");
                    writer.WriteStartObject();
                    writer.WriteNumber("oid", item.ResultFilled.OrderId);
                    writer.WriteNumber("totalSz", item.ResultFilled.FilledQuantity!.Value);
                    writer.WriteNumber("avgPx", item.ResultFilled.AveragePrice!.Value);
                    writer.WriteEndObject();
                }
            }

            writer.WriteEndArray();
        }
    }
}
