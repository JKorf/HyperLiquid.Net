using CryptoExchange.Net.Sockets;
using System;
using HyperLiquid.Net.Objects.Internal;

namespace HyperLiquid.Net.Objects.Sockets
{
    internal class HyperLiquidPingQuery : Query<HyperLiquidPong>
    {
        public HyperLiquidPingQuery() : base(new HyperLiquidPing(), false)
        {
            RequestTimeout = TimeSpan.FromSeconds(5);
            MessageRouter = MessageRouter.CreateWithoutHandler<HyperLiquidPong>("pong");
        }
    }
}
