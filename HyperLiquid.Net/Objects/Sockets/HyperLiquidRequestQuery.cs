using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;
using HyperLiquid.Net.Objects.Internal;
using System;
using CryptoExchange.Net.Sockets.Default;
using CryptoExchange.Net.Sockets.Default.Routing;
using CryptoExchange.Net;
using HyperLiquid.Net.Clients.BaseApi;
using CryptoExchange.Net.Objects.Errors;

namespace HyperLiquid.Net.Objects.Sockets
{
    internal class HyperLiquidRequestQuery<TResponse> : Query<TResponse>
    {
        public HyperLiquidRequestQuery(
            HyperLiquidSocketClientApi client,
            string method,
            string type,
            Parameters request,
            bool authenticated, 
            int weight = 1) 
            : base(
                new HyperLiquidRequest
                {
                    Id = ExchangeHelpers.NextId(), 
                    Method = method, 
                    Request = new HyperLiquidRequestWrapper { Type = type, Payload = request } 
                }, 
                authenticated,
                weight)
        {
            if (authenticated)
            {
                client.AuthenticationProvider!.ProcessRequest(client, request);

                MessageRouter = MessageRouter.Create([
                    MessageRoute.CreateForQuery<HyperLiquidSocketUpdate<HyperLiquidSocketResponseAuth<TResponse>>, TResponse>(((HyperLiquidRequest)Request).Id.ToString(), HandleMessage),
                ]);
            }
            else
            {
                MessageRouter = MessageRouter.Create([
                    MessageRoute.CreateForQuery<HyperLiquidSocketUpdate<HyperLiquidSocketResponse<TResponse>>, TResponse>(((HyperLiquidRequest)Request).Id.ToString(), HandleMessage),
                ]);
            }
        }

        public CallResult<TResponse> HandleMessage(SocketConnection connection, DateTime receiveTime, string? originalData, HyperLiquidSocketUpdate<HyperLiquidSocketResponseAuth<TResponse>> message)
        {
            if (!message.Data.Response.Payload.Status.Equals("ok"))
                return CallResult<TResponse>.Fail(new ServerError(ErrorInfo.Unknown with { Message = message.Data.Response.Payload.Status }), originalData);

            return CallResult<TResponse>.Ok(message.Data.Response.Payload.Data!.Data, originalData);
        }

        public CallResult<TResponse> HandleMessage(SocketConnection connection, DateTime receiveTime, string? originalData, HyperLiquidSocketUpdate<HyperLiquidSocketResponse<TResponse>> message)
        {
            return CallResult<TResponse>.Ok(message.Data.Response.Payload.Data, originalData);
        }
    }
}
