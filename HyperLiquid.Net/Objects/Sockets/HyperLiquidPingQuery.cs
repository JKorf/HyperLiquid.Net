using CryptoExchange.Net.Sockets;
using System;
using System.Collections.Generic;
using HyperLiquid.Net.Objects.Internal;

namespace HyperLiquid.Net.Objects.Sockets
{
    internal class HyperLiquidPingQuery : Query<HyperLiquidPong>
    {
        public HyperLiquidPingQuery() : base(new HyperLiquidPing(), false)
        {
            RequestTimeout = TimeSpan.FromSeconds(5);
            MessageMatcher = MessageMatcher.Create<HyperLiquidPong>("pong");
            MessageRouter = MessageRouter.CreateWithoutHandler<HyperLiquidPong>("pong");
        }
    }
}
