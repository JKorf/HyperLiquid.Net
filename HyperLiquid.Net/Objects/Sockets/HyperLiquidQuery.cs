using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;
using HyperLiquid.Net.Objects.Internal;
using System;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Converters.MessageParsing;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Sockets.Default;
using CryptoExchange.Net.Sockets.Default.Routing;

namespace HyperLiquid.Net.Objects.Sockets
{
    internal class HyperLiquidQuery<T> : Query<HyperLiquidSocketUpdate<T>>
    {
        private string? _errorString;
        private readonly SocketApiClient _client;

        public HyperLiquidQuery(
            SocketApiClient client,
            HyperLiquidSocketRequest request,
            string listenId,
            bool authenticated, 
            int weight = 1) : base(request, authenticated, weight)
        {
            _client = client;
            MessageRouter = MessageRouter.Create([
                MessageRoute.CreateForQuery<HyperLiquidSocketUpdate<T>>("subscriptionResponse", listenId, HandleMessage),
                MessageRoute.CreateForQuery<HyperLiquidSocketUpdate<string>, HyperLiquidSocketUpdate<T>>("error", listenId, HandleError)
                ]);
        }

        public CallResult<HyperLiquidSocketUpdate<T>> HandleError(SocketConnection connection, DateTime receiveTime, string? originalData, HyperLiquidSocketUpdate<string> message)
        {
            var error = message.Data;

            if (error.StartsWith("Already subscribed:")
             || error.StartsWith("Already unsubscribed"))
            {
                // Allow duplicate subscriptions
                return CallResult.Ok(new HyperLiquidSocketUpdate<T> { Channel = message.Channel }, originalData);
            }

            return CallResult<HyperLiquidSocketUpdate<T>>.Fail(new ServerError(_client.GetErrorInfo("Subscription", error)), originalData);
        }

        public CallResult<HyperLiquidSocketUpdate<T>> HandleMessage(SocketConnection connection, DateTime receiveTime, string? originalData, HyperLiquidSocketUpdate<T> message)
        {
            if (_errorString != null 
                && !_errorString.StartsWith("Already subscribed:") // Allow duplicate subscriptions
                && !_errorString.StartsWith("Already unsubscribed")) // Can happen when subscribe returns an error
            {
                var err = _errorString;
                _errorString = null;
                return CallResult.Fail<HyperLiquidSocketUpdate<T>>(new ServerError(_client.GetErrorInfo("Subscription", err)), originalData);
            }

            return CallResult.Ok(message, originalData);
        }
    }
}
