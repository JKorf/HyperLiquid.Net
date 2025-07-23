using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.Sockets;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using HyperLiquid.Net.Objects.Internal;
using System.Linq;

namespace HyperLiquid.Net.Objects.Sockets.Subscriptions
{
    /// <inheritdoc />
    internal class HyperLiquidSubscription<T> : Subscription<HyperLiquidSocketUpdate<HyperLiquidSubscribeRequest>, HyperLiquidSocketUpdate<HyperLiquidUnsubscribeRequest>>
    {
        private readonly string _topic;
        private readonly Dictionary<string, object> _parameters;
        private readonly Action<DataEvent<T>> _handler;

        /// <summary>
        /// ctor
        /// </summary>
        public HyperLiquidSubscription(ILogger logger, string topic, string listenId, Dictionary<string, object>? parameters, Action<DataEvent<T>> handler, bool auth) : base(logger, auth)
        {
            _handler = handler;
            _topic = topic;
            _parameters = parameters ?? new();

            MessageMatcher = MessageMatcher.Create<HyperLiquidSocketUpdate<T>>(listenId, DoHandleMessage);
        }

        /// <inheritdoc />
        public override Query? GetSubQuery(SocketConnection connection)
        {
            var subscription = new Dictionary<string, object>{ { "type", _topic } };
            foreach(var kvp in _parameters)
                subscription.Add(kvp.Key, kvp.Value);

            return new HyperLiquidQuery<HyperLiquidSubscribeRequest>(new HyperLiquidSubscribeRequest
            {
                Subscription = subscription
            }, 
            "subscriptionResponse-" + _topic + ((_parameters.Any() ? "-" : "") + string.Join("-", _parameters.Select(x => x.Value))),
            "error-" + _topic + ((_parameters.Any() ? "-" : "") + string.Join("-", _parameters.Select(x => x.Value))), false);
        }

        /// <inheritdoc />
        public override Query? GetUnsubQuery()
        {
            var subscription = new Dictionary<string, object> { { "type", _topic } };
            foreach (var kvp in _parameters)
                subscription.Add(kvp.Key, kvp.Value);

            return new HyperLiquidQuery<HyperLiquidSubscribeRequest>(new HyperLiquidUnsubscribeRequest
            {
                Subscription = subscription
            },
            "subscriptionResponse-" + _topic + ((_parameters.Any() ? "-" : "") + string.Join("-", _parameters.Select(x => x.Value))),
            "error-" + _topic + ((_parameters.Any() ? "-" : "") + string.Join("-", _parameters.Select(x => x.Value))), false);
        }

        /// <inheritdoc />
        public CallResult DoHandleMessage(SocketConnection connection, DataEvent<HyperLiquidSocketUpdate<T>> message)
        {
            _handler.Invoke(message.As(message.Data.Data!, _topic, null, SocketUpdateType.Update));
            return CallResult.SuccessResult;
        }
    }
}
