using CryptoExchange.Net;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Errors;
using CryptoExchange.Net.Sockets;
using CryptoExchange.Net.Sockets.Default;
using CryptoExchange.Net.Sockets.Default.Routing;
using HyperLiquid.Net.Clients.BaseApi;
using HyperLiquid.Net.Objects.Internal;
using HyperLiquid.Net.Objects.Models;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Objects.Sockets
{
    internal record HyperLiquidSocketMessage2<T>
    {
        [JsonPropertyName("channel")]
        public string Channel { get; set; } = string.Empty;
        [JsonPropertyName("data")]
        public T Data { get; set; } = default!;
    }

    internal record HyperLiquidSocketEnvelope2<T>
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }
        [JsonPropertyName("response")]
        public T Response { get; set; } = default!;
    }

    internal record HyperLiquidSocketResponse2<T>
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
        [JsonPropertyName("payload")]
        public T Payload { get; set; } = default!;
    }

    internal record HyperLiquidSocketPayload2
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
        [JsonPropertyName("response")]
        public JsonElement Response { get; set; }
    }


    internal class HyperLiquidRequestQuery2<TResponse> : Query<TResponse>
    {
        public HyperLiquidRequestQuery2(
            HyperLiquidSocketClientApi client,
            string method,
            string type,
            Parameters request,
            int weight = 1) 
            : base(
                new HyperLiquidRequest
                {
                    Id = ExchangeHelpers.NextId(), 
                    Method = method, 
                    Request = new HyperLiquidRequestWrapper { Type = type, Payload = request } 
                }, 
                true,
                weight)
        {
            client.AuthenticationProvider!.ProcessRequest(client, request);

            MessageRouter = MessageRouter.Create([
                MessageRoute.CreateForQuery<HyperLiquidSocketMessage2<HyperLiquidSocketEnvelope2<HyperLiquidSocketResponse2<HyperLiquidSocketPayload2>>>, TResponse>(((HyperLiquidRequest)Request).Id.ToString(), HandleMessage),
            ]);            
        }

        public CallResult<TResponse> HandleMessage(SocketConnection connection, DateTime receiveTime, string? originalData, HyperLiquidSocketMessage2<HyperLiquidSocketEnvelope2<HyperLiquidSocketResponse2<HyperLiquidSocketPayload2>>> message)
        {
            // Responses
            // Generic error message:
            // {"channel":"post","data":{"id":7,"response":{"type":"action","payload":{"status":"err","response":"User or API Wallet does not exist."}}}}
            // Failed single operation:
            // {"channel":"post","data":{"id":7,"response":{"type":"action","payload":{"status":"err","response":"Order has zero size."}}}}
            // Failed multi operation:
            // {"channel":"post","data":{"id":7,"response":{"type":"action","payload":{"status":"ok","response":{"type":"cancel","data":{"statuses":[{"error":"Order was never placed, already canceled, or filled. asset=1"}]}}}}}}
            // Success single operation:
            // {"channel":"post","data":{"id":7,"response":{"type":"action","payload":{"status":"ok","response":{"type":"default"}}}}}
            // Success multi operation:
            // {"channel":"post","data":{"id":7,"response":{"type":"action","payload":{"status":"ok","response":{"type":"order","data":{"statuses":[{"resting":{"oid":123}}]}}}}}}

            var payload = message.Data.Response.Payload;
            if (!payload.Status.Equals("ok"))
            {
                string errorMsg;
                if (payload.Response.ValueKind == JsonValueKind.String)
                {
                    errorMsg = payload.Response.GetString()!;
                }
                else if (payload.Response.ValueKind == JsonValueKind.Object
                    && payload.Response.TryGetProperty("type", out var type)
                    && type.ValueKind == JsonValueKind.String)
                {
                    errorMsg = type.GetString()!;
                }
                else
                {
                    errorMsg = payload.Response.GetRawText();
                }

                return CallResult<TResponse>.Fail(new ServerError(ErrorInfo.Unknown with { Message = errorMsg }), originalData);
            }

#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
#pragma warning disable IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
            var response = payload.Response.Deserialize<TResponse>(HyperLiquidExchange._serializerContext);
#pragma warning restore IL3050 // Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
            return CallResult<TResponse>.Ok(response!, originalData);
        }
    }
}
