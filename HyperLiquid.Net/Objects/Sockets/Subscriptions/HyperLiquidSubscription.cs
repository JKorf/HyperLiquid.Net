using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using HyperLiquid.Net.Objects.Internal;
using System.Linq;
using CryptoExchange.Net.Clients;

namespace HyperLiquid.Net.Objects.Sockets.Subscriptions
{
    /// <inheritdoc />
    internal class HyperLiquidSubscription<T> : Subscription
    {
        private readonly SocketApiClient _client;
        private readonly string _topic;
        private readonly Dictionary<string, object> _parameters;
        private readonly Action<DateTime, string?, int, HyperLiquidSocketUpdate<T>> _handler;

        /// <summary>
        /// ctor
        /// </summary>
        public HyperLiquidSubscription(
            ILogger logger,
            SocketApiClient client, 
            string topic, 
            string? listenSuffix, 
            Dictionary<string, object>? parameters,
            Action<DateTime, string?, int, HyperLiquidSocketUpdate<T>> handler, 
            bool auth,
            string? alternativeTopic = null) : base(logger, auth)
        {
            _client = client;
            _handler = handler;
            _topic = topic;
            _parameters = parameters ?? new();

            var listenId = (alternativeTopic ?? topic) + listenSuffix;
            MessageMatcher = MessageMatcher.Create<HyperLiquidSocketUpdate<T>>(listenId, DoHandleMessage);
            MessageRouter = MessageRouter.CreateWithOptionalTopicFilter<HyperLiquidSocketUpdate<T>>(alternativeTopic ?? topic, listenSuffix, DoHandleMessage);
        }

        /// <inheritdoc />
        protected override Query? GetSubQuery(SocketConnection connection)
        {
            var subscription = new Dictionary<string, object>{ { "type", _topic } };
            foreach(var kvp in _parameters)
                subscription.Add(kvp.Key, kvp.Value);

            return new HyperLiquidQuery<HyperLiquidSubscribeRequest>(_client, new HyperLiquidSubscribeRequest
            {
                Subscription = subscription
            }, 
            _topic + string.Join("", _parameters.Select(x => x.Value)), false);
        }

        /// <inheritdoc />
        protected override Query? GetUnsubQuery(SocketConnection connection)
        {
            var subscription = new Dictionary<string, object> { { "type", _topic } };
            foreach (var kvp in _parameters)
                subscription.Add(kvp.Key, kvp.Value);

            return new HyperLiquidQuery<HyperLiquidSubscribeRequest>(_client, new HyperLiquidUnsubscribeRequest
            {
                Subscription = subscription
            },
            _topic + string.Join("", _parameters.Select(x => x.Value)), false);
        }

        /// <inheritdoc />
        public CallResult DoHandleMessage(SocketConnection connection, DateTime receiveTime, string? originalData, HyperLiquidSocketUpdate<T> message)
        {
            _handler.Invoke(receiveTime, originalData, ConnectionInvocations, message);
            //_handler.Invoke(message.As(message.Data.Data!, _topic, null, _firstUpdateIsSnapshot && ConnectionInvocations == 1 ? SocketUpdateType.Snapshot : SocketUpdateType.Update));
            return CallResult.SuccessResult;
        }
    }
}
