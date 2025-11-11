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
    internal class HyperLiquidSubscription<T> : Subscription<HyperLiquidSocketUpdate<HyperLiquidSubscribeRequest>, HyperLiquidSocketUpdate<HyperLiquidUnsubscribeRequest>>
    {
        private readonly SocketApiClient _client;
        private readonly string _topic;
        private readonly Dictionary<string, object> _parameters;
        private readonly Action<DataEvent<T>> _handler;
        private readonly bool _firstUpdateIsSnapshot;

        /// <summary>
        /// ctor
        /// </summary>
        public HyperLiquidSubscription(
            ILogger logger,
            SocketApiClient client, 
            string topic, 
            string listenId, 
            Dictionary<string, object>? parameters,
            Action<DataEvent<T>> handler, 
            bool auth,
            bool firstUpdateIsSnapshot = false) : base(logger, auth)
        {
            _client = client;
            _handler = handler;
            _topic = topic;
            _parameters = parameters ?? new();
            _firstUpdateIsSnapshot = firstUpdateIsSnapshot;

            MessageMatcher = MessageMatcher.Create<HyperLiquidSocketUpdate<T>>(listenId, DoHandleMessage);
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
            "subscriptionResponse-" + _topic + ((_parameters.Any() ? "-" : "") + string.Join("-", _parameters.Select(x => x.Value))),
            "error-" + _topic + ((_parameters.Any() ? "-" : "") + string.Join("-", _parameters.Select(x => x.Value))), false);
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
            "subscriptionResponse-" + _topic + ((_parameters.Any() ? "-" : "") + string.Join("-", _parameters.Select(x => x.Value))),
            "error-" + _topic + ((_parameters.Any() ? "-" : "") + string.Join("-", _parameters.Select(x => x.Value))), false);
        }

        /// <inheritdoc />
        public CallResult DoHandleMessage(SocketConnection connection, DataEvent<HyperLiquidSocketUpdate<T>> message)
        {
            _handler.Invoke(message.As(message.Data.Data!, _topic, null, _firstUpdateIsSnapshot && ConnectionInvocations == 1 ? SocketUpdateType.Snapshot : SocketUpdateType.Update));
            return CallResult.SuccessResult;
        }
    }
}
