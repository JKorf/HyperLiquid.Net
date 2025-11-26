using CryptoExchange.Net.Converters.MessageParsing.DynamicConverters;
using CryptoExchange.Net.Converters.SystemTextJson;
using HyperLiquid.Net;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Channels;
using static HyperLiquid.Net.Converters.CancelResultConverter;

namespace HyperLiquid.Net.Clients.MessageHandlers
{
    internal class HyperLiquidSocketMessageHandler : JsonSocketMessageHandler
    {
        public override JsonSerializerOptions Options { get; } = HyperLiquidExchange._serializerContext;

        protected override MessageEvaluator[] MessageEvaluators { get; } = [
            new MessageEvaluator {
                Priority = 1,
                Fields = [
                    new PropertyFieldReference("channel") { Constraint = x => x!.Equals("subscriptionResponse", StringComparison.Ordinal) },
                    new PropertyFieldReference("type") { Depth = 3 },
                    new PropertyFieldReference("coin") { Depth = 3 },
                    new PropertyFieldReference("interval") { Depth = 3 },
                ],
                IdentifyMessageCallback = x => $"{x.FieldValue("channel")}-{x.FieldValue("type")}-{x.FieldValue("coin")}-{x.FieldValue("interval")}"
            },
             new MessageEvaluator {
                Priority = 2,
                Fields = [
                    new PropertyFieldReference("channel") { Constraint = x => x!.Equals("subscriptionResponse", StringComparison.Ordinal) },
                    new PropertyFieldReference("type") { Depth = 3 },
                    new PropertyFieldReference("coin") { Depth = 3 },
                ],
                IdentifyMessageCallback = x => $"{x.FieldValue("channel")}-{x.FieldValue("type")}-{x.FieldValue("coin")}"
            },
             new MessageEvaluator {
                Priority = 3,
                Fields = [
                    new PropertyFieldReference("channel") { Constraint = x => x!.Equals("subscriptionResponse", StringComparison.Ordinal) },
                    new PropertyFieldReference("type") { Depth = 3 },
                    new PropertyFieldReference("user") { Depth = 3 },
                ],
                IdentifyMessageCallback = x => $"{x.FieldValue("channel")}-{x.FieldValue("type")}-{x.FieldValue("user")}"
            },
             new MessageEvaluator {
                Priority = 4,
                Fields = [
                    new PropertyFieldReference("channel") { Constraint = x => x!.Equals("subscriptionResponse", StringComparison.Ordinal) },
                    new PropertyFieldReference("type") { Depth = 3 },
                ],
                IdentifyMessageCallback = x => $"{x.FieldValue("channel")}-{x.FieldValue("type")}"
            },

             new MessageEvaluator {
                Priority = 5,
                Fields = [
                    new PropertyFieldReference("channel") { Constraint = x => x!.Equals("error", StringComparison.Ordinal) },
                    new PropertyFieldReference("data") { Depth = 3 },
                ],
                IdentifyMessageCallback = x => {
                    // error message format: "Invalid subscription {\"type\":\"candle\",\"interval\":\"1d\",\"coin\":\"TST2\"}"
                    var error = x.FieldValue("data");
                    if (error!.StartsWith("Invalid subscription")
                      || error.StartsWith("Already subscribed")
                      || error.StartsWith("Already unsubscribed"))
                    {
                        var json = error.Replace("Invalid subscription ", "")
                                        .Replace("Already subscribed: ", "")
                                        .Replace("Already unsubscribed: ", "");
                        JsonDocument jsonDoc;
                        try
                        {
                            jsonDoc = JsonDocument.Parse(json);
                        }
                        catch (Exception)
                        {
                            return "error";
                        }

                        var type = jsonDoc.RootElement.GetProperty("type").GetString();
                        var coin = jsonDoc.RootElement.TryGetProperty("coin", out var coinProp) ? coinProp.GetString() : null;
                        var interval = jsonDoc.RootElement.TryGetProperty("interval", out var intervalProp) ? intervalProp.GetString() : null;
                        var user = jsonDoc.RootElement.TryGetProperty("user", out var userProp) ? userProp.GetString() : null;
                        var id = "error-" + type;
                        if (coin != null)
                            id += "-" + coin;
                        if (interval != null)
                            id += "-" + interval;
                        if (user != null)
                            id += "-" + user;

                        return id;
                    }
                    else
                    {
                        return "error";
                    }
                }
            },

             new MessageEvaluator {
                Priority = 6,
                Fields = [
                    new PropertyFieldReference("channel") { Constraint = x => x!.Equals("user", StringComparison.Ordinal) },
                ],
                StaticIdentifier = "userEvents"
            },

            new MessageEvaluator {
                Priority = 7,
                Fields = [
                    new PropertyFieldReference("channel") { Constraint = x => x!.Equals("trades", StringComparison.Ordinal) },
                    new PropertyFieldReference("coin") { Depth = 3 },
                ],
                IdentifyMessageCallback = x => $"{x.FieldValue("channel")}-{x.FieldValue("coin")}"
            },

            new MessageEvaluator {
                Priority = 8,
                Fields = [
                    new PropertyFieldReference("channel") 
                    { 
                        Constraint = x => x!.Equals("l2Book", StringComparison.Ordinal)
                                        || x.Equals("activeSpotAssetCtx", StringComparison.Ordinal)
                                        || x.Equals("activeAssetCtx", StringComparison.Ordinal)
                                        || x.Equals("activeAssetData", StringComparison.Ordinal)
                    },
                    new PropertyFieldReference("coin") { Depth = 2 },
                ],
                IdentifyMessageCallback = x => $"{x.FieldValue("channel")}-{x.FieldValue("coin")}"
            },

            new MessageEvaluator {
                Priority = 9,
                Fields = [
                    new PropertyFieldReference("channel") { Constraint = x => x!.Equals("candle", StringComparison.Ordinal) },
                    new PropertyFieldReference("s") { Depth = 2 },
                    new PropertyFieldReference("i") { Depth = 2 },
                ],
                IdentifyMessageCallback = x => $"{x.FieldValue("channel")}-{x.FieldValue("s")}-{x.FieldValue("i")}"
            },

            new MessageEvaluator {
                Priority = 10,
                Fields = [
                    new PropertyFieldReference("channel"),
                    new PropertyFieldReference("s") { Depth = 2 },
                ],
                IdentifyMessageCallback = x => $"{x.FieldValue("channel")}-{x.FieldValue("s")}"
            },

            new MessageEvaluator {
                Priority = 11,
                Fields = [
                    new PropertyFieldReference("channel"),
                ],
                IdentifyMessageCallback = x => $"{x.FieldValue("channel")}"
            },

             
        ];
    }
}
