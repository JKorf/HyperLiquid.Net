using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using System.Collections.Generic;
using HyperLiquid.Net.Objects.Internal;
using System;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Converters.MessageParsing;
using CryptoExchange.Net.Clients;

namespace HyperLiquid.Net.Objects.Sockets
{
    internal class HyperLiquidQuery<T> : Query<HyperLiquidSocketUpdate<T>>
    {
        private string? _errorString;
        private readonly SocketApiClient _client;

        public HyperLiquidQuery(SocketApiClient client, HyperLiquidSocketRequest request, string listenId, string errorListenId, bool authenticated, int weight = 1) : base(request, authenticated, weight)
        {
            _client = client;
            MessageMatcher = MessageMatcher.Create<HyperLiquidSocketUpdate<T>>([listenId, errorListenId], HandleMessage);
            MessageRouter = MessageRouter.Create<HyperLiquidSocketUpdate<T>>([listenId, errorListenId], HandleMessage);
        }

        public override CallResult<object> Deserialize(IMessageAccessor message, Type type)
        {
            if (message.GetValue<string>(MessagePath.Get().Property("channel"))?.Equals("error") == true)
            {
                // Error is set, the actual model doesn't matter
                _errorString = message.GetValue<string>(MessagePath.Get().Property("data"));
                return new CallResult<object>(new HyperLiquidSocketUpdate<T>());
            }

            return base.Deserialize(message, type);
        }

        public CallResult<HyperLiquidSocketUpdate<T>> HandleMessage(SocketConnection connection, DateTime receiveTime, string? originalData, HyperLiquidSocketUpdate<T> message)
        {
            if (_errorString != null 
                && !_errorString.StartsWith("Already subscribed:") // Allow duplicate subscriptions
                && !_errorString.StartsWith("Already unsubscribed")) // Can happen when subscribe returns an error
            {
                var err = _errorString;
                _errorString = null;
                return new CallResult<HyperLiquidSocketUpdate<T>>(new ServerError(_client.GetErrorInfo("Subscription", err)));
            }

            return new CallResult<HyperLiquidSocketUpdate<T>>(message, originalData, null);
        }
    }
}
