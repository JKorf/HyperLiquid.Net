//using System.Diagnostics;
//using System.Numerics;

//namespace HyperLiquid.Net.Signing
//{
//    /// <summary>
//    /// This class regroup helper functions for computation in the Z/Zp associated with secp256k1
//    /// </summary>
//    internal static class Secp256k1ZCalculator
//    {
//        // Length of Q, the modulus
//        private const int _bitLength = 32 * 8;

//        // The modulus used for Secp256k1
//        internal readonly static BigInteger _q;

//        // The remaining used in modulus operator optimization
//        private readonly static BigInteger _r;

//        static Secp256k1ZCalculator()
//        {
//            // p = 2^256 - 2^32 - 2^9 - 2^8 - 2^7 - 2^6 - 2^4 - 1
//            var b = BigInteger.TryParse("00FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEFFFFFC2F", System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out _q);
//            Debug.Assert(b, "Parsing Q");
//            // Calculate residue
//            _r = (BigInteger.One << _bitLength) - _q;
//        }

//        // optimized mod operator. The argument must be positive or zero
//        static internal BigInteger Secp256k1ModReduce(this BigInteger x)
//        {
//            var y = x;
//            int qLen = _bitLength;
//            BigInteger qMod = BigInteger.One << qLen;
//            while (x > qMod)
//            {
//                BigInteger u = x >> qLen;
//                BigInteger v = x % qMod;
//                u = u * _r;
//                x = u + v;
//            }

//            while (x >= _q)
//            {
//                x = x - _q;
//            }

//            Debug.Assert(x.Secp256k1IsValid(), "X is not valid");
//            Debug.Assert(x == y % _q, "X is good");
//            return x;
//        }

//        static internal BigInteger Secp256k1Square(this BigInteger x)
//        {
//            return (x * x).Secp256k1ModReduce();
//        }

//        static internal BigInteger Secp256k1Multiply(this BigInteger x, BigInteger y)
//        {
//            if (x.IsZero || y.IsOne)
//                return x;
//            if (y.IsZero || x.IsOne)
//                return y;
//            return (x * y).Secp256k1ModReduce();
//        }

//        static internal BigInteger Secp256k1Invert(this BigInteger x)
//        {
//            // Todo : take Bouncy Castle implementation 3 time faster.
//            return BigInteger.ModPow(x, _q - 2, _q).Secp256k1ModReduce();
//        }

//        static internal BigInteger Secp256k1Negate(this BigInteger x)
//        {
//            return x.IsZero ? x : _q - x;
//        }

//        static internal bool Secp256k1IsValid(this BigInteger x) => x >= 0 && x < _q;

//        static internal BigInteger? Secp256k1Sqrt(this BigInteger x)
//        {
//            if (x.IsZero || x.IsOne)
//                return x;

//            BigInteger e = (_q >> 2) + 1;
//            return CheckSqrt(x, BigInteger.ModPow(x, e, _q));
//        }

//        private static BigInteger? CheckSqrt(BigInteger x, BigInteger z)
//        {
//            return z.Secp256k1Square().Equals(x) ? z : null;
//        }

//        static internal BigInteger Secp256k1Add(this BigInteger x1, BigInteger x2)
//        {
//            BigInteger x3 = x1 + x2;
//            if (x3 > _q)
//                x3 -= _q;
//            return x3;
//        }

//        static internal BigInteger Secp256k1Two(this BigInteger x) => x.Secp256k1Add(x);
//        static internal BigInteger Secp256k1Three(this BigInteger x) => x.Secp256k1Add(x.Secp256k1Two());
//        static internal BigInteger Secp256k1Subtract(this BigInteger x1, BigInteger x2)
//        {
//            BigInteger x3 = x1 - x2;
//            if (x3.Sign < 0)
//                x3 += _q;
//            return x3;
//        }

//        static internal BigInteger Secp256k1MultiplyMinusProduct(this BigInteger a, BigInteger b, BigInteger x, BigInteger y)
//        {
//            BigInteger ab = a.Secp256k1Multiply(b);
//            BigInteger xy = x.Secp256k1Multiply(y);
//            return ab.Secp256k1Subtract(xy).Secp256k1ModReduce();
//        }
//    }
//}
