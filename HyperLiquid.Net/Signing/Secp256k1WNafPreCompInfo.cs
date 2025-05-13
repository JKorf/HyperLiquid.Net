﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyperLiquid.Net.Signing
{
    internal sealed class Secp256k1WNafPreCompInfo
    {
        public Secp256k1Point[] PreComp = new Secp256k1Point[0];
        public Secp256k1Point[] PreCompNeg = new Secp256k1Point[0];
        public Secp256k1Point? Twice = null;
    }
}
