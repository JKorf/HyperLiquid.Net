using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Math.EC.Endo;
using Org.BouncyCastle.Math.EC.Multiplier;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HyperLiquid.Net
{
    /// <summary>
    /// This class regroup helper functions associated with the secp256k1 Curve 
    /// </summary>
    internal static class Secp256k1PointCalculator
    {
        internal readonly static int A = 0;
        internal readonly static int B = 7;
        internal readonly static BigInteger N;
        internal readonly static BigInteger HalfN;
        internal readonly static BigInteger G1;
        internal readonly static BigInteger G2;
        internal readonly static BigInteger V1_0;
        internal readonly static BigInteger V1_1;
        internal readonly static BigInteger V2_0;
        internal readonly static BigInteger V2_1;
        internal readonly static BigInteger Beta;
        internal readonly static Secp256k1Point G;
        internal readonly static BigInteger ScaleXPointMap;
        private static readonly BigInteger[] DEFAULT_WINDOW_SIZE_CUTOFFS = new BigInteger[] { BigInteger.One << 13, BigInteger.One << 41, BigInteger.One << 121, BigInteger.One << 337, BigInteger.One << 897, BigInteger.One << 2305 };

        static Secp256k1PointCalculator()
        {
            var b = BigInteger.TryParse("00FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEBAAEDCE6AF48A03BBFD25E8CD0364141", System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out N);
            Debug.Assert(b, "Parse N");
            HalfN = N >> 1;
            b = BigInteger.TryParse("003086d221a7d46bcde86c90e49284eb153dab", System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out G1);
            Debug.Assert(b, "Parse G1");
            b = BigInteger.TryParse("00e4437ed6010e88286f547fa90abfe4c42212", System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out G2);
            Debug.Assert(b, "Parse G2");
            b = BigInteger.TryParse("003086d221a7d46bcde86c90e49284eb15", System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out V1_0);
            Debug.Assert(b, "Parse V1_0");
            b = BigInteger.TryParse("00e4437ed6010e88286f547fa90abfe4c3", System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out var v);
            Debug.Assert(b, "Parse V1_1");
            V1_1 = -v;
            b = BigInteger.TryParse("00114ca50f7a8e2f3f657c1108d9d44cfd8", System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out V2_0);
            Debug.Assert(b, "Parse V2_0");
            b = BigInteger.TryParse("003086d221a7d46bcde86c90e49284eb15", System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out V2_1);
            Debug.Assert(b, "Parse V2_1");

            b = BigInteger.TryParse("7ae96a2b657c07106e64479eac3434e99cf0497512f58995c1396c28719501ee", System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out Beta);
            Debug.Assert(b, "Parse Beta");

            b = BigInteger.TryParse("79be667ef9dcbbac55a06295ce870b07029bfcdb2dce28d959f2815b16f81798", System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out var Gx);
            Debug.Assert(b, "Parse G.X");
            b = BigInteger.TryParse("483ada7726a3c4655da4fbfc0e1108a8fd17b448a68554199c47d08ffb10d4b8", System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out var Gy);
            Debug.Assert(b, "Parse G.Y");

            G = new Secp256k1Point(Gx, Gy);
            Debug.Assert(G.IsValid(), "Parse G");

            b = BigInteger.TryParse("07ae96a2b657c07106e64479eac3434e99cf0497512f58995c1396c28719501ee", System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out ScaleXPointMap);
            Debug.Assert(b, "Parse ScaleXPointMap");
        }

        internal static Secp256k1Point DecompressPointSecp256k1(BigInteger X1, int yTilde)
        {
            var x = X1;
            var rhs = ((X1.Secp256k1Square() * X1) + 7).Secp256k1ModReduce();
            var y = rhs.Secp256k1Sqrt();
            /*
             * If y is not a square, then we haven't got a point on the curve
             */
            if (y == null)
                throw new ArgumentException("Invalid point compression");

            if (y.Value.IsEven == (yTilde == 1))
            {
                // Use the other root
                y = y.Value.Secp256k1Negate();
            }

            return new Secp256k1Point(x, y.Value);
        }

        public static bool IsValid(this Secp256k1Point p)
        {
            if (p.IsInfinity())
                return true;

            if (!p.X.Secp256k1IsValid())
                return false;

            if (!p.Y.Secp256k1IsValid())
                return false;

            if (!SatisfiesCurveEquation(p))
                return false;

            return true;
        }

        private static bool SatisfiesCurveEquation(this Secp256k1Point p)
        {
            BigInteger X = p.X, Y = p.Y, Z = p.Z; //, A = Curve.A, B = Curve.B;
            BigInteger B = 7;
            if (!Z.IsOne)
            {
                BigInteger Z2 = Z.Secp256k1Square(), Z4 = Z2.Secp256k1Square(), Z6 = Z2.Secp256k1Multiply(Z4);
                B = B.Secp256k1Multiply(Z6);
            }

            BigInteger lhs = Y.Secp256k1Square();
            BigInteger rhs = X.Secp256k1Square().Secp256k1Multiply(X).Secp256k1Add(B);
            return lhs == rhs;
        }

        public static bool IsInfinity(this Secp256k1Point p) => p == Secp256k1Point.Infinity;
        public static Secp256k1Point MultiplyByN(this Secp256k1Point p)
        {
            if (p.IsInfinity())
                return p;
            Secp256k1Point result = MultiplyPositiveByN(p);

            Debug.Assert(result.IsValid());
            return result;
        }

        public static Secp256k1Point Negate(this Secp256k1Point p)
        {
            //Debug.Assert(p.IsValid());
            return p.IsInfinity() ? p : new Secp256k1Point(p.X, p.Y.Secp256k1Negate());
        }

        public static Secp256k1Point MultiplyPositiveByN(Secp256k1Point p)
        {
            //(BigInteger a, BigInteger b) = DecomposeScalar(0);
            return Secp256k1Point.Infinity;

            //ECPointMap pointMap = glvEndomorphism.PointMap;
            //return ECAlgorithms.ImplShamirsTrickWNaf(p, a, pointMap, b);
        }

        public static bool IsNormalized(this Secp256k1Point p)
        {
            return p.IsInfinity()
                || p.Z.IsOne;
        }

        public static Secp256k1Point Normalize(this Secp256k1Point p)
        {
            if (p.IsInfinity())
            {
                return p;
            }

            if (p.Z.IsOne)
            {
                return p;
            }

            return p.Normalize(p.Z.Secp256k1Invert());
        }

        internal static Secp256k1Point Normalize(this Secp256k1Point p, BigInteger zInv)
        {
            BigInteger zInv2 = zInv.Secp256k1Square(), zInv3 = zInv2.Secp256k1Multiply(zInv);
            return new Secp256k1Point(p.X.Secp256k1Multiply(zInv2), p.Y.Secp256k1Multiply(zInv3));
        }
        internal static Secp256k1Point SumOfTwoMultiplies(Secp256k1PointPreCompCache precompInfos, Secp256k1Point p, BigInteger a, Secp256k1Point q, BigInteger b)
        {
            Secp256k1Point result = ImplSumOfMultipliesGlv(precompInfos, new Secp256k1Point[] { p, q }, new BigInteger[] { a, b });
            Debug.Assert(result.IsValid());
            return result;
        }
        internal static Secp256k1Point ImplSumOfMultipliesGlv(Secp256k1PointPreCompCache precompInfos, Secp256k1Point[] ps, BigInteger[] ks)
        {
            int len = ps.Length;

            BigInteger[] abs = new BigInteger[len << 1];
            for (int i = 0, j = 0; i < len; ++i)
            {
                var ksMod = ks[i] % N;
                if (ksMod < 0)
                    ksMod += N;
                (BigInteger a, BigInteger b) = DecomposeScalar(ksMod);
                abs[j++] = a;
                abs[j++] = b;
            }

            var result = ImplSumOfMultiplies(precompInfos, ps, abs);
            var BCsecp256k1 = SecNamedCurves.GetByName("secp256k1");
            var BCps = new Org.BouncyCastle.Math.EC.ECPoint[ps.Length];
            for (int i = 0; i < ps.Length; i++)
            {
                BCps[i] = BCsecp256k1.Curve.CreatePoint(ToBouncyCastle(ps[i].X), ToBouncyCastle(ps[i].Y));
            }
            var BCks = new Org.BouncyCastle.Math.BigInteger[ks.Length];
            for(int i = 0; i < ks.Length; i++)
            {
                BCks[i] = ToBouncyCastle(ks[i]);
            }


            var BCresult = Org.BouncyCastle.Math.EC.ECAlgorithms.SumOfMultiplies(BCps, BCks);
            BCresult = BCresult.Normalize();
            result = result.Normalize();
            var BCX = BCresult.XCoord.ToBigInteger().ToString(16);
            var BCY = BCresult.YCoord.ToBigInteger().ToString(16);
            var mx = result.X.ToString("x");
            var my = result.Y.ToString("x");
            var good = mx.EndsWith(BCX) && my.EndsWith(BCY);
            if (!good)
            {
                Debug.Assert(good);
            }
            return result;
        }

        internal static Secp256k1Point ImplSumOfMultiplies(Secp256k1PointPreCompCache precompInfos, Secp256k1Point[] ps, BigInteger[] ks)
        {
            int halfCount = ps.Length, fullCount = halfCount << 1;

            bool[] negs = new bool[fullCount];
            Secp256k1WNafPreCompInfo[] infos = new Secp256k1WNafPreCompInfo[fullCount];
            byte[][] wnafs = new byte[fullCount][];

            for (int i = 0; i < halfCount; ++i)
            {
                int j0 = i << 1, j1 = j0 + 1;

                BigInteger kj0 = ks[j0]; negs[j0] = kj0.Sign < 0; kj0 = BigInteger.Abs(kj0);
                BigInteger kj1 = ks[j1]; negs[j1] = kj1.Sign < 0; kj1 = BigInteger.Abs(kj1);

                int width = System.Math.Max(2, System.Math.Min(16, GetWindowSize(BigInteger.Max(kj0, kj1))));

                Secp256k1Point P = ps[i], Q = MapPointWithPrecomp(precompInfos, P, width, true);
                infos[j0] = precompInfos.Get(P) ?? new Secp256k1WNafPreCompInfo();
                infos[j1] = precompInfos.Get(Q) ?? new Secp256k1WNafPreCompInfo();
                wnafs[j0] = GenerateWindowNaf(width, kj0);
                wnafs[j1] = GenerateWindowNaf(width, kj1);
            }

            return ImplSumOfMultiplies(negs, infos, wnafs);
        }

        private static Secp256k1Point ImplSumOfMultiplies(bool[] negs, Secp256k1WNafPreCompInfo[] infos, byte[][] wnafs)
        {
            int len = 0, count = wnafs.Length;
            for (int i = 0; i < count; ++i)
            {
                len = System.Math.Max(len, wnafs[i].Length);
            }

            Secp256k1Point R = Secp256k1Point.Infinity;
            int zeroes = 0;

            for (int i = len - 1; i >= 0; --i)
            {
                Secp256k1Point r = Secp256k1Point.Infinity;

                for (int j = 0; j < count; ++j)
                {
                    byte[] wnaf = wnafs[j];
                    int wi = i < wnaf.Length ? (int)(sbyte)wnaf[i] : 0;
                    if (wi != 0)
                    {
                        int n = System.Math.Abs(wi);
                        Secp256k1WNafPreCompInfo info = infos[j];
                        Secp256k1Point[] table = (wi < 0 == negs[j]) ? info.PreComp : info.PreCompNeg;
                        r = r.Add(table[n >> 1]);
                    }
                }

                if (r == Secp256k1Point.Infinity)
                {
                    ++zeroes;
                    continue;
                }

                if (zeroes > 0)
                {
                    R = R.TimesPow2(zeroes);
                    zeroes = 0;
                }

                R = R.TwicePlus(r);
            }

            if (zeroes > 0)
            {
                R = R.TimesPow2(zeroes);
            }

            return R;
        }

        private static Secp256k1Point TimesPow2(this Secp256k1Point p, int e)
        {
            if (e < 0)
                throw new ArgumentException("cannot be negative", "e");
            if (e == 0 || p.IsInfinity())
                return p;
            if (e == 1)
                return p.Twice();

            BigInteger Y1 = p.Y;
            if (Y1.IsZero)
                return Secp256k1Point.Infinity;


            BigInteger X1 = p.X;
            BigInteger Z1 = p.Z;

            for (int i = 0; i < e; ++i)
            {
                if (Y1.IsZero)
                    return Secp256k1Point.Infinity;

                BigInteger X1Squared = X1.Secp256k1Square();
                BigInteger M = X1Squared.Secp256k1Three();
                BigInteger _2Y1 = Y1.Secp256k1Two();
                BigInteger _2Y1Squared = _2Y1.Secp256k1Multiply(Y1);
                BigInteger S = X1.Secp256k1Multiply(_2Y1Squared).Secp256k1Two();
                BigInteger _4T = _2Y1Squared.Secp256k1Square();
                BigInteger _8T = _4T.Secp256k1Two();

                X1 = M.Secp256k1Square().Secp256k1Subtract(S.Secp256k1Two());
                Y1 = M.Secp256k1Multiply(S.Secp256k1Subtract(X1)).Secp256k1Subtract(_8T);
                Z1 = Z1.IsOne ? _2Y1 : _2Y1.Secp256k1Multiply(Z1);
            }

            return new Secp256k1Point(X1, Y1, Z1);
        }


        public static Secp256k1Point TwicePlus(this Secp256k1Point p, Secp256k1Point b)
        {
            if (p == b)
                return p.ThreeTimes();
            if (p.IsInfinity())
                return b;
            if (b.IsInfinity())
                return p.Twice();

            BigInteger Y1 = p.Y;
            if (Y1.IsZero)
                return b;

            return p.TwiceJacobianModified().Add(b);
        }

        public static byte[] GenerateWindowNaf(int width, BigInteger k)
        {
            if (width == 2)
            {
                return GenerateNaf(k);
            }

            if (width < 2 || width > 8)
                throw new ArgumentException("must be in the range [2, 8]", "width");
            if (k.Sign == 0)
                return Array.Empty<Byte>();

            byte[] wnaf = new byte[257];

            // 2^width and a mask and sign bit set accordingly
            int pow2 = 1 << width;
            int mask = pow2 - 1;
            int sign = pow2 >> 1;

            bool carry = false;
            int length = 0, pos = 0;
            var kb = k.ToByteArray();
            while (pos <= GetBitLength(kb))
            {
                if (TestBit(kb, pos) == carry)
                {
                    ++pos;
                    continue;
                }

                k = k >> (pos);
                kb = k.ToByteArray();
                int digit = kb[0] & mask;
                if (carry)
                {
                    ++digit;
                }

                carry = (digit & sign) != 0;
                if (carry)
                {
                    digit -= pow2;
                }

                length += (length > 0) ? pos - 1 : pos;
                wnaf[length++] = (byte)digit;
                pos = width;
            }

            // Reduce the WNAF array to its actual length
            if (wnaf.Length > length)
            {
                Array.Resize(ref wnaf, length);
            }

            return wnaf;
        }

        private static bool TestBit(byte[] bytes, int pos)
        {
            var byteIndex = pos / 8;
            var byteMask = 1 << (pos % 8);
            return (bytes[byteIndex] & byteMask) > 0;
        }

        private static int GetBitLength(byte[] bytes)
        {
            int bitLength = (bytes.Length - 1) * 8;
            byte mostSignificantByte = bytes[bytes.Length - 1];
            while (mostSignificantByte != 0)
            {
                mostSignificantByte >>= 1;
                bitLength++;
            }

            return bitLength;
        }

        private static byte[] GenerateNaf(BigInteger k)
        {
            if (k.Sign == 0)
                return Array.Empty<Byte>();

            BigInteger _3k = k.Secp256k1Three();
            byte[] _3kb = _3k.ToByteArray();
            int digits = 255;
            byte[] naf = new byte[digits];
            byte[] kb = k.ToByteArray();

            for (int i = 0; i < 32; ++i)
            {

                var a = _3kb.Length > i ? _3kb[i] : (byte)0;
                var b = kb.Length > i ? kb[i] : (byte)0;
                var x = (byte)(a ^ b);
                for (int j = 0; j < 8; j++)
                {
                    if (i == 0 && j == 0)
                        continue;
                    if (i * 8 + j >= digits)
                        break;
                    var mask = ((byte)1) << j;
                    if ((mask & x) > 0)
                    {
                        naf[i - 1] = (byte)((b & mask) > 0 ? -1 : 1);
                        ++i;
                    }
                }
            }
            naf[digits - 1] = 1;

            return naf;
        }


        public static int GetWindowSize(BigInteger q)
        {
            int i;
            for (i = 0; i < DEFAULT_WINDOW_SIZE_CUTOFFS.Length && q >= DEFAULT_WINDOW_SIZE_CUTOFFS[i]; i++)
            {
            }

            return i + 2;
        }

        public static Secp256k1Point ThreeTimes(this Secp256k1Point p)
        {
            if (p.IsInfinity())
                return p;

            var Y1 = p.Y;
            if (Y1.IsZero)
                return p;

            return p.TwiceJacobianModified().Add(p);
        }
        public static Secp256k1Point Twice(this Secp256k1Point p)
        {
            if (p.IsInfinity())
                return p;

            var Y1 = p.Y;
            if (Y1.IsZero)
                return Secp256k1Point.Infinity;

            var result = p.TwiceJacobianModified();
            var bcp = p.ToBouncyCastle();
            var bct = bcp.Twice();
            //if (!EqualBouncyCastle(result, bct))
            //{
            //    var r2 = p.TwiceJacobianModified();
            //    var bct2 = bcp.Twice();
            //    Debug.Assert(false, "failed");
            //}
            return result;
        }

        public static Secp256k1Point Add(this Secp256k1Point p1, Secp256k1Point p2)
        {
            if (p1.IsInfinity())
                return p2;
            if (p2.IsInfinity())
                return p1;
            if (p1 == p2)
                return p1.Twice();


            BigInteger X1 = p1.X, Y1 = p1.Y;
            BigInteger X2 = p2.X, Y2 = p2.Y;

            {
                BigInteger Z1 = p1.Z;
                BigInteger Z2 = p2.Z;

                bool Z1IsOne = Z1.IsOne;

                BigInteger X3, Y3, Z3, Z3Squared;

                if (!Z1IsOne && Z1.Equals(Z2))
                {
                    // TODO Make this available as public method coZAdd?

                    BigInteger dx = X1.Secp256k1Subtract(X2), dy = Y1.Secp256k1Subtract(Y2);
                    if (dx.IsZero)
                    {
                        if (dy.IsZero)
                        {
                            return p1.Twice();
                        }
                        return Secp256k1Point.Infinity;
                    }

                    BigInteger C = dx.Secp256k1Square();
                    BigInteger W1 = X1.Secp256k1Multiply(C), W2 = X2.Secp256k1Multiply(C);
                    BigInteger A1 = W1.Secp256k1Subtract(W2).Secp256k1Multiply(Y1);

                    X3 = dy.Secp256k1Square().Secp256k1Subtract(W1).Secp256k1Subtract(W2);
                    Y3 = W1.Secp256k1Subtract(X3).Secp256k1Multiply(dy).Secp256k1Subtract(A1);
                    Z3 = dx;

                    if (Z1IsOne)
                    {
                        Z3Squared = C;
                    }
                    else
                    {
                        Z3 = Z3.Secp256k1Multiply(Z1);
                    }
                }
                else
                {
                    BigInteger Z1Squared, U2, S2;
                    if (Z1IsOne)
                    {
                        Z1Squared = Z1; U2 = X2; S2 = Y2;
                    }
                    else
                    {
                        Z1Squared = Z1.Secp256k1Square();
                        U2 = Z1Squared.Secp256k1Multiply(X2);
                        BigInteger Z1Cubed = Z1Squared.Secp256k1Multiply(Z1);
                        S2 = Z1Cubed.Secp256k1Multiply(Y2);
                    }

                    bool Z2IsOne = Z2.IsOne;
                    BigInteger Z2Squared, U1, S1;
                    if (Z2IsOne)
                    {
                        Z2Squared = Z2; U1 = X1; S1 = Y1;
                    }
                    else
                    {
                        Z2Squared = Z2.Secp256k1Square();
                        U1 = Z2Squared.Secp256k1Multiply(X1);
                        BigInteger Z2Cubed = Z2Squared.Secp256k1Multiply(Z2);
                        S1 = Z2Cubed.Secp256k1Multiply(Y1);
                    }

                    BigInteger H = U1.Secp256k1Subtract(U2);
                    BigInteger R = S1.Secp256k1Subtract(S2);

                    // Check if b == this or b == -this
                    if (H.IsZero)
                    {
                        if (R.IsZero)
                        {
                            // this == b, i.e. this must be doubled
                            return p1.Twice();
                        }

                        // this == -b, i.e. the result is the point at infinity
                        return Secp256k1Point.Infinity;
                    }

                    BigInteger HSquared = H.Secp256k1Square();
                    BigInteger G = HSquared.Secp256k1Multiply(H);
                    BigInteger V = HSquared.Secp256k1Multiply(U1);

                    X3 = R.Secp256k1Square().Secp256k1Add(G).Secp256k1Subtract(V.Secp256k1Two());
                    Y3 = V.Secp256k1Subtract(X3).Secp256k1MultiplyMinusProduct(R, G, S1);

                    Z3 = H;
                    if (!Z1IsOne)
                    {
                        Z3 = Z3.Secp256k1Multiply(Z1);
                    }
                    if (!Z2IsOne)
                    {
                        Z3 = Z3.Secp256k1Multiply(Z2);
                    }

                    // Alternative calculation of Z3 using fast square
                    //X3 = four(X3);
                    //Y3 = eight(Y3);
                    //Z3 = doubleProductFromSquares(Z1, Z2, Z1Squared, Z2Squared).Multiply(H);

                    if (Z3 == H)
                    {
                        Z3Squared = HSquared;
                    }
                }
                return new Secp256k1Point(X3, Y3, Z3);
            }


        }
        public static Secp256k1Point TwiceJacobianModified(this Secp256k1Point p)
        {
            //Debug.Assert(p.IsValid());
            BigInteger X1 = p.X, Y1 = p.Y, Z1 = p.Z;

            BigInteger X1Squared = X1.Secp256k1Square();
            BigInteger M = X1Squared.Secp256k1Three();
            BigInteger _2Y1 = Y1.Secp256k1Two();
            BigInteger _2Y1Squared = _2Y1.Secp256k1Multiply(Y1);
            BigInteger S = (X1.Secp256k1Multiply(_2Y1Squared)).Secp256k1Two();
            BigInteger X3 = M.Secp256k1Square().Secp256k1Subtract(S.Secp256k1Two());
            BigInteger _4T = _2Y1Squared.Secp256k1Square();
            BigInteger _8T = _4T.Secp256k1Two();
            BigInteger Y3 = M.Secp256k1Multiply(S.Secp256k1Subtract(X3)).Secp256k1Subtract(_8T);
            BigInteger Z3 = Z1.IsOne ? _2Y1 : _2Y1.Secp256k1Multiply(Z1);

            var result = new Secp256k1Point(X3, Y3, Z3);
            //Debug.Assert(result.IsValid());
            return result;

        }

        private static Secp256k1Point MapPointMap(this Secp256k1Point p)
        {
            return p.IsInfinity()
                ? p
                : new Secp256k1Point(p.X.Secp256k1Multiply(ScaleXPointMap), p.Y, p.Z);
        }

        public static Secp256k1Point MapPointWithPrecomp(Secp256k1PointPreCompCache precompInfos, Secp256k1Point p, int width, bool includeNegated)
        {
            Secp256k1WNafPreCompInfo wnafPreCompP = Precompute(precompInfos, p, width, includeNegated);

            Secp256k1Point q = p.MapPointMap();
            Secp256k1WNafPreCompInfo wnafPreCompQ = precompInfos.Get(q) ?? new Secp256k1WNafPreCompInfo();

            Secp256k1Point? twiceP = wnafPreCompP.Twice;
            if (twiceP != null)
            {
                Secp256k1Point twiceQ = twiceP.Value.MapPointMap();
                wnafPreCompQ.Twice = twiceQ;
            }

            Secp256k1Point[] preCompP = wnafPreCompP.PreComp;
            Secp256k1Point[] preCompQ = new Secp256k1Point[preCompP.Length];
            for (int i = 0; i < preCompP.Length; ++i)
            {
                preCompQ[i] = preCompP[i].MapPointMap();
            }
            wnafPreCompQ.PreComp = preCompQ;

            if (includeNegated)
            {
                Secp256k1Point[] preCompNegQ = new Secp256k1Point[preCompQ.Length];
                for (int i = 0; i < preCompNegQ.Length; ++i)
                {
                    preCompNegQ[i] = preCompQ[i].Negate();
                }
                wnafPreCompQ.PreCompNeg = preCompNegQ;
            }

            precompInfos.Set(q, wnafPreCompQ);

            return q;
        }
        public static Org.BouncyCastle.Math.BigInteger ToBouncyCastle(this BigInteger p)
        {
            if (p.Sign >= 0)
                return new Org.BouncyCastle.Math.BigInteger(p.ToString("x"), 16);
            return new Org.BouncyCastle.Math.BigInteger((-p).ToString("x"), 16).Negate();
        }
        public static Org.BouncyCastle.Math.EC.ECPoint ToBouncyCastle(this Secp256k1Point p)
        {
            var BCsecp256k1 = SecNamedCurves.GetByName("secp256k1");
            var result = BCsecp256k1.Curve.CreatePoint(ToBouncyCastle(p.X), ToBouncyCastle(p.Y));
            Org.BouncyCastle.Math.EC.FpFieldElement oldZ = result.GetZCoord(0) as Org.BouncyCastle.Math.EC.FpFieldElement;
            result.GetZCoords()[0] = new FpFieldElement(oldZ.Q, new Org.BouncyCastle.Math.BigInteger(p.Z.ToString("x"), 16));
            return result;
        }

        public static bool EqualBouncyCastle(this Secp256k1Point p, Org.BouncyCastle.Math.EC.ECPoint bcp)
        {
            if (!p.X.ToString("x").EndsWith(bcp.XCoord.ToBigInteger().ToString(16))) return false;
            if (!p.Y.ToString("x").EndsWith(bcp.YCoord.ToBigInteger().ToString(16))) return false;
            if (!p.Z.ToString("x").EndsWith(bcp.GetZCoord(0).ToBigInteger().ToString(16))) return false;
            return true;
        }

        public static Secp256k1WNafPreCompInfo Precompute(Secp256k1PointPreCompCache precompInfos, Secp256k1Point p, int width, bool includeNegated)
        {
            Debug.Assert(p.IsValid());
            Secp256k1WNafPreCompInfo wNafPreCompInfo = precompInfos.Get(p) ?? new Secp256k1WNafPreCompInfo();
            int reqPreCompLen = 1 << System.Math.Max(0, width - 2);
            Secp256k1Point[] preComp = wNafPreCompInfo.PreComp;
            int iniPreCompLen = preComp.Length;

            if (iniPreCompLen < reqPreCompLen)
            {
                Array.Resize(ref preComp, reqPreCompLen);
                if (reqPreCompLen == 1)
                {
                    preComp[0] = p;
                }
                else
                {
                    int curPreCompLen = iniPreCompLen;
                    if (curPreCompLen == 0)
                    {
                        preComp[0] = p;
                        curPreCompLen = 1;
                    }

                    BigInteger? iso = null;
                    if (reqPreCompLen == 2)
                    {
                        preComp[1] = p.ThreeTimes();
                    }
                    else
                    {
                        Secp256k1Point twiceP;
                        Secp256k1Point last = preComp[curPreCompLen - 1];
                        if (wNafPreCompInfo.Twice == null)
                        {
                            twiceP = preComp[0].Twice();
                            wNafPreCompInfo.Twice = twiceP;
                            Debug.Assert(twiceP.IsValid());
                            if (!twiceP.IsInfinity())
                            {
                                iso = twiceP.Z;
                                twiceP = new Secp256k1Point(twiceP.X, twiceP.Y);
                                BigInteger iso2 = iso.Value.Secp256k1Square();
                                BigInteger iso3 = iso2.Secp256k1Multiply(iso.Value);
                                last = new Secp256k1Point(last.X.Secp256k1Multiply(iso2), last.Y.Secp256k1Multiply(iso3), last.Z);
                                if (iniPreCompLen == 0)
                                {
                                    preComp[0] = last;
                                }
                            }
                        }
                        else
                        {
                            twiceP = wNafPreCompInfo.Twice.Value;
                        }
//                        Debug.Assert(eCPoint.IsValid());
                        while (curPreCompLen < reqPreCompLen)
                        {
                            last = (preComp[curPreCompLen++] = last.Add(twiceP));
//                            Debug.Assert(eCPoint2.IsValid());
                        }
                    }

                    NormalizeAll(preComp, iniPreCompLen, reqPreCompLen - iniPreCompLen, iso);
                }
            }

            wNafPreCompInfo.PreComp = preComp;
            if (includeNegated)
            {
                Secp256k1Point[] array2 = wNafPreCompInfo.PreCompNeg;
                int i;
                if (array2 == null)
                {
                    i = 0;
                    array2 = new Secp256k1Point[reqPreCompLen];
                }
                else
                {
                    i = array2.Length;
                    if (i < reqPreCompLen)
                    {
                        Array.Resize(ref array2, reqPreCompLen);
                    }
                }

                for (; i < reqPreCompLen; i++)
                {
                    array2[i] = preComp[i].Negate();
                }

                wNafPreCompInfo.PreCompNeg = array2;
            }

            precompInfos.Set(p, wNafPreCompInfo);

            var r = Org.BouncyCastle.Math.EC.Multiplier.WNafUtilities.Precompute(p.ToBouncyCastle(), width, includeNegated);
            Debug.Assert(wNafPreCompInfo.Twice.Value.X.ToString("x").EndsWith(r.Twice.XCoord.ToBigInteger().ToString(16)), "Twice X");
            Debug.Assert(wNafPreCompInfo.Twice.Value.Y.ToString("x").EndsWith(r.Twice.YCoord.ToBigInteger().ToString(16)), "Twice Y");
            Debug.Assert(wNafPreCompInfo.PreComp.Length == r.PreComp.Length, "PreComp.Length");
            Debug.Assert(wNafPreCompInfo.PreCompNeg.Length == r.PreCompNeg.Length, "PreCompNeg.Length");
            for (int i = 0; i < r.PreComp.Length; i++)
            {
                Debug.Assert(wNafPreCompInfo.PreComp[i].X.ToString("x").EndsWith(r.PreComp[i].XCoord.ToBigInteger().ToString(16)), $"PreComp[{i}] X");
                Debug.Assert(wNafPreCompInfo.PreComp[i].Y.ToString("x").EndsWith(r.PreComp[i].YCoord.ToBigInteger().ToString(16)), $"PreComp[{i}] Y");
                Debug.Assert(wNafPreCompInfo.PreComp[i].Z.ToString("x").EndsWith(r.PreComp[i].GetZCoord(0).ToBigInteger().ToString(16)), $"PreComp[{i}] Z");
            }
            for (int i = 0; i < r.PreCompNeg.Length; i++)
            {
                Debug.Assert(wNafPreCompInfo.PreCompNeg[i].X.ToString("x").EndsWith(r.PreCompNeg[i].XCoord.ToBigInteger().ToString(16)), $"PreCompNeg[{i}] X");
                Debug.Assert(wNafPreCompInfo.PreCompNeg[i].Y.ToString("x").EndsWith(r.PreCompNeg[i].YCoord.ToBigInteger().ToString(16)), $"PreCompNeg[{i}] Y");
                Debug.Assert(wNafPreCompInfo.PreCompNeg[i].Z.ToString("x").EndsWith(r.PreCompNeg[i].GetZCoord(0).ToBigInteger().ToString(16)), $"PreCompNeg[{i}] Z");
            }
            return wNafPreCompInfo;
        }
        private static void NormalizeAll(Secp256k1Point[] points, int off, int len, BigInteger? iso)
        {
            // Figure out which of the points actually need to be normalized

            BigInteger[] zs = new BigInteger[len];
            int[] indices = new int[len];
            int count = 0;
            for (int i = 0; i < len; ++i)
            {
                Secp256k1Point? p = points[off + i];
                if (null != p && (iso != null || !p.Value.IsNormalized()))
                {
                    zs[count] = p.Value.Z;
                    indices[count++] = off + i;
                }
            }

            if (count == 0)
            {
                return;
            }

            MontgomeryTrick(zs, 0, count, iso);

            for (int j = 0; j < count; ++j)
            {
                int index = indices[j];
                points[index] = points[index].Normalize(zs[j]);
            }
        }

        public static void MontgomeryTrick(BigInteger[] zs, int off, int len, BigInteger? scale)
        {
            //* Uses the "Montgomery Trick" to invert many field elements, with only a single actual
            //* field inversion. See e.g. the paper:
            //* "Fast Multi-scalar Multiplication Methods on Elliptic Curves with Precomputation Strategy Using Montgomery Trick"
            //* by Katsuyuki Okeya, Kouichi Sakurai.

            BigInteger[] c = new BigInteger[len];
            c[0] = zs[off];

            int i = 0;
            while (++i < len)
            {
                c[i] = c[i - 1].Secp256k1Multiply(zs[off + i]);
            }

            --i;

            if (scale != null)
            {
                c[i] = c[i].Secp256k1Multiply(scale.Value);
            }

            BigInteger u = c[i].Secp256k1Invert();

            while (i > 0)
            {
                int j = off + i--;
                BigInteger tmp = zs[j];
                zs[j] = c[i].Secp256k1Multiply(u);
                u = u.Secp256k1Multiply(tmp);
            }

            zs[off] = u;
        }

        private static BigInteger CalculateB(BigInteger k, BigInteger g, int t)
        {
            bool negative = (g.Sign < 0);
            var gAbs = negative ? -g : g;
            BigInteger b = k * gAbs;
            var c = b >> (t - 1);
            if (c.IsEven)
            {
                b = (c >> 1);
            }
            else
            {
                b = (c >> 1) + 1;
            }
            return negative ? -b : b;
        }

        private static (BigInteger a, BigInteger b) DecomposeScalar(BigInteger k)
        {
            int bits = 272;
            BigInteger b1 = CalculateB(k, G1, bits);
            BigInteger b2 = CalculateB(k, G2, bits);

            BigInteger a = k - ((b1 * V1_0) + (b2 * V2_0));
            BigInteger b = -((b1 * V1_1) + (b2 * V2_1));

            return (a, b);
        }
        /*
        internal static ECPoint ImplShamirsTrickWNaf(Secp256k1Point P, BigInteger k, ECPointMap pointMapQ, BigInteger l)
        {
            bool negK = k.Sign < 0, negL = l.Sign < 0;

            k = k.Sign < 0 ? -k : k;
            l = l.Sign < 0 ? -l : l;

            int width = System.Math.Max(2, System.Math.Min(16, WNafUtilities.GetWindowSize(System.Math.Max(k.BitLength, l.BitLength))));

            ECPoint Q = WNafUtilities.MapPointWithPrecomp(P, width, true, pointMapQ);
            WNafPreCompInfo infoP = WNafUtilities.GetWNafPreCompInfo(P);
            WNafPreCompInfo infoQ = WNafUtilities.GetWNafPreCompInfo(Q);

            ECPoint[] preCompP = negK ? infoP.PreCompNeg : infoP.PreComp;
            ECPoint[] preCompQ = negL ? infoQ.PreCompNeg : infoQ.PreComp;
            ECPoint[] preCompNegP = negK ? infoP.PreComp : infoP.PreCompNeg;
            ECPoint[] preCompNegQ = negL ? infoQ.PreComp : infoQ.PreCompNeg;

            byte[] wnafP = WNafUtilities.GenerateWindowNaf(width, k);
            byte[] wnafQ = WNafUtilities.GenerateWindowNaf(width, l);

            return ImplShamirsTrickWNaf(preCompP, preCompNegP, wnafP, preCompQ, preCompNegQ, wnafQ);
        }

        private static ECPoint ImplShamirsTrickWNaf(ECPoint[] preCompP, ECPoint[] preCompNegP, byte[] wnafP,
            ECPoint[] preCompQ, ECPoint[] preCompNegQ, byte[] wnafQ)
        {
            int len = System.Math.Max(wnafP.Length, wnafQ.Length);

            ECCurve curve = preCompP[0].Curve;
            ECPoint infinity = curve.Infinity;

            ECPoint R = infinity;
            int zeroes = 0;

            for (int i = len - 1; i >= 0; --i)
            {
                int wiP = i < wnafP.Length ? (int)(sbyte)wnafP[i] : 0;
                int wiQ = i < wnafQ.Length ? (int)(sbyte)wnafQ[i] : 0;

                if ((wiP | wiQ) == 0)
                {
                    ++zeroes;
                    continue;
                }

                ECPoint r = infinity;
                if (wiP != 0)
                {
                    int nP = System.Math.Abs(wiP);
                    ECPoint[] tableP = wiP < 0 ? preCompNegP : preCompP;
                    r = r.Add(tableP[nP >> 1]);
                }
                if (wiQ != 0)
                {
                    int nQ = System.Math.Abs(wiQ);
                    ECPoint[] tableQ = wiQ < 0 ? preCompNegQ : preCompQ;
                    r = r.Add(tableQ[nQ >> 1]);
                }

                if (zeroes > 0)
                {
                    R = R.TimesPow2(zeroes);
                    zeroes = 0;
                }

                R = R.TwicePlus(r);
            }

            if (zeroes > 0)
            {
                R = R.TimesPow2(zeroes);
            }

            return R;
        }
        */

    }
}
