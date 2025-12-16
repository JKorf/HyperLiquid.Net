using System.Collections.Generic;

namespace HyperLiquid.Net.Signing
{
    internal class Secp256k1PointPreCompCache
    {
        private Dictionary<Secp256k1Point, Secp256k1WNafPreCompInfo> _preCompInfos = new Dictionary<Secp256k1Point, Secp256k1WNafPreCompInfo>();
        
        public Secp256k1WNafPreCompInfo? Get(Secp256k1Point point)
        {
            if (_preCompInfos.TryGetValue(point, out var info))
                return info;

            return null;
        }

        public void Set(Secp256k1Point point, Secp256k1WNafPreCompInfo info)
        {
            _preCompInfos[point] = info;
        }

    }
}
