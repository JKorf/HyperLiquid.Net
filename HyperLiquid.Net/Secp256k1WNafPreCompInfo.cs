using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyperLiquid.Net
{
    internal record Secp256k1WNafPreCompInfo(Secp256k1Point[] PreComp, Secp256k1Point[] PreCompNeg, Secp256k1Point Twice)
    {
    }
}
