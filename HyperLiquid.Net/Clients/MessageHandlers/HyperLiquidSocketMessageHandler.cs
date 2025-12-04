using CryptoExchange.Net.Converters.MessageParsing.DynamicConverters;
using CryptoExchange.Net.Converters.SystemTextJson;
using HyperLiquid.Net;
using HyperLiquid.Net.Objects.Internal;
using HyperLiquid.Net.Objects.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Channels;
using static HyperLiquid.Net.Converters.CancelResultConverter;

namespace HyperLiquid.Net.Clients.MessageHandlers
{
    internal class HyperLiquidSocketMessageHandler : JsonSocketMessageHandler
    {
        private static readonly HashSet<string> _errorJsonMessages = [
            "Invalid subscription ",
            "Already subscribed: ",
            "Already unsubscribed: ",
            ];

        public override JsonSerializerOptions Options { get; } = HyperLiquidExchange._serializerContext;

        public HyperLiquidSocketMessageHandler()
        {
            AddTopicMapping<HyperLiquidSocketUpdate<HyperLiquidSubscribeRequest>>(x =>
            {
                x.Data.Subscription.TryGetValue("type", out var type);
                x.Data.Subscription.TryGetValue("coin", out var coin);
                x.Data.Subscription.TryGetValue("interval", out var interval);
                x.Data.Subscription.TryGetValue("user", out var user);
                return type?.ToString() + coin?.ToString() + interval?.ToString() + user?.ToString();
            });
            AddTopicMapping<HyperLiquidSocketUpdate<string>>(x =>
            {
                // error message format: "Invalid subscription {\"type\":\"candle\",\"interval\":\"1d\",\"coin\":\"TST2\"}"

                if (x.Channel != "error")
                    return null;

                // Error mapping
                if (!_errorJsonMessages.Any(err => x.Data.StartsWith(err)))
                    return "error";

                var json = x.Data;
                foreach (var item in _errorJsonMessages)
                    json = json.Replace(item, "");

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
                return type?.ToString() + coin?.ToString() + interval?.ToString() + user?.ToString();
            });

            AddTopicMapping<HyperLiquidSocketUpdate<HyperLiquidKline>>(x => x.Data.Symbol + x.Data.Interval);
            AddTopicMapping<HyperLiquidSocketUpdate<HyperLiquidOrderBook>>(x => x.Data.Symbol);
            AddTopicMapping<HyperLiquidSocketUpdate<HyperLiquidTrade[]>>(x => x.Data.First().Symbol);
            AddTopicMapping<HyperLiquidSocketUpdate<HyperLiquidFuturesTickerUpdate>>(x => x.Data.Symbol);
            AddTopicMapping<HyperLiquidSocketUpdate<HyperLiquidFuturesUserSymbolUpdate>>(x => x.Data.Symbol);
            AddTopicMapping<HyperLiquidSocketUpdate<HyperLiquidTickerUpdate>>(x => x.Data.Symbol);
        }

        protected override MessageEvaluator[] TypeEvaluators { get; } = [
            //new MessageEvaluator {
            //    Priority = 1,
            //    Fields = [
            //        new PropertyFieldReference("channel") { Constraint = x => x!.Equals("user", StringComparison.Ordinal) },
            //    ],
            //    StaticIdentifier = "userEvents"
            //},

            new MessageEvaluator {
                Priority = 2,
                Fields = [
                    new PropertyFieldReference("channel"),
                ],
                IdentifyMessageCallback = x => $"{x.FieldValue("channel")}"
            },
             
        ];
    }
}
