using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HyperLiquid.Net
{
    // represent a point on the Secq256k1 curve
    internal record struct Secp256k1Point(BigInteger X, BigInteger Y)
    {
        public static Secp256k1Point Infinity = new Secp256k1Point(Secp256k1ZCalculator.Q, Secp256k1ZCalculator.Q);
    }
}
