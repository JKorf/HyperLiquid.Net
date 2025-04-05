using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Math.EC.Endo;
using Org.BouncyCastle.Math.EC.Multiplier;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HyperLiquid.Net
{
    /// <summary>
    /// This class regroup helper functions associated with the secp256k1 Curle 
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
        internal readonly static BigInteger Scale;
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

            b = BigInteger.TryParse("07ae96a2b657c07106e64479eac3434e99cf0497512f58995c1396c28719501ee", System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out Scale);
            Debug.Assert(b, "Parse Scale : PointMap");
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

            if (y.Value.IsEven != (yTilde == 1))
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
            BigInteger X = p.X, Y = p.Y; //, A = Curve.A, B = Curve.B;
            BigInteger lhs = Y.Secp256k1Square();
            BigInteger rhs = ((X.Secp256k1Square() * X) + 7).Secp256k1ModReduce();
            return lhs == rhs;
        }

        public static bool IsInfinity(this Secp256k1Point p) => p == Secp256k1Point.Infinity;
        public static Secp256k1Point MultiplyByN(this Secp256k1Point p)
        {
            // Todo : EGA A supprimer
            var bsecp256k1 = SecNamedCurves.GetByName("secp256k1");

            var bPpoint = bsecp256k1.Curve.ImportPoint(new FpPoint(bsecp256k1.Curve, bsecp256k1.Curve.FromBigInteger(new Org.BouncyCastle.Math.BigInteger(p.X.ToString("X"), 16)), bsecp256k1.Curve.FromBigInteger(new Org.BouncyCastle.Math.BigInteger(p.Y.ToString("X"), 16))));
            //           var bk = new Org.BouncyCastle.Math.BigInteger(k.ToString("X"), 16);
            var bk = bsecp256k1.N;
            var bmp = bPpoint.Multiply(bk);
            if (!bmp.IsInfinity)
            {
                Console.WriteLine("not infiny");
            }

            if (p.IsInfinity())
                return Secp256k1Point.Infinity;
            Secp256k1Point result = MultiplyPositive(p);

            Debug.Assert(result.IsValid());
            return result;
        }

        public static Secp256k1Point Negate(this Secp256k1Point p)
        {
            Debug.Assert(p.IsValid());
            return p.IsInfinity() ? p : new Secp256k1Point(p.X, p.Y.Secp256k1Negate());
        }

        public static Secp256k1Point MultiplyPositive(Secp256k1Point p)
        {
            //(BigInteger a, BigInteger b) = DecomposeScalar(0);
            return Secp256k1Point.Infinity;

            //ECPointMap pointMap = glvEndomorphism.PointMap;
            //return ECAlgorithms.ImplShamirsTrickWNaf(p, a, pointMap, b);
        }

        internal static Secp256k1Point SumOfTwoMultiplies(Secp256k1Point p, BigInteger a, Secp256k1Point q, BigInteger b)
        {
            Secp256k1Point result = ImplSumOfMultipliesGlv(new ECPoint[] { p, q }, new BigInteger[] { a, b }));
            Debug.Assert(p.IsValid());
            return result;
        }
        internal static Secp256k1Point ImplSumOfMultipliesGlv(Secp256k1Point[] ps, BigInteger[] ks)
        {
            int len = ps.Length;

            BigInteger[] abs = new BigInteger[len << 1];
            for (int i = 0, j = 0; i < len; ++i)
            {
                (BigInteger a, BigInteger b) = DecomposeScalar(ks[i] % N);
                abs[j++] = a;
                abs[j++] = b;
            }

            return ImplSumOfMultiplies(ps, Scale, abs);
        }

        internal static Secp256k1Point ImplSumOfMultiplies(ECPoint[] ps, BigInteger scale, BigInteger[] ks)
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

                ECPoint P = ps[i], Q = WNafUtilities.MapPointWithPrecomp(P, width, true, pointMap);
                infos[j0] = WNafUtilities.GetWNafPreCompInfo(P);
                infos[j1] = WNafUtilities.GetWNafPreCompInfo(Q);
                wnafs[j0] = WNafUtilities.GenerateWindowNaf(width, kj0);
                wnafs[j1] = WNafUtilities.GenerateWindowNaf(width, kj1);
            }

            return ImplSumOfMultiplies(negs, infos, wnafs);
        }

        public static int GetWindowSize(BigInteger q)
        {
            int i;
            for (i = 0; i < DEFAULT_WINDOW_SIZE_CUTOFFS.Length && q >= DEFAULT_WINDOW_SIZE_CUTOFFS[i]; i++)
            {
            }

            return i + 2;
        }
        public static Secp256k1Point MapPointWithPrecomp(Secp256k1Point p, int width, bool includeNegated)
        {
            Secp256k1WNafPreCompInfo wnafPreCompP = Precompute(p, width, includeNegated);

            Secp256k1Point q = pointMap.Map(p);
            Secp256k1WNafPreCompInfo wnafPreCompQ = GetWNafPreCompInfo(c.GetPreCompInfo(q, PRECOMP_NAME));

            Secp256k1Point twiceP = wnafPreCompP.Twice;
            if (twiceP != null)
            {
                ECPoint twiceQ = pointMap.Map(twiceP);
                wnafPreCompQ.Twice = twiceQ;
            }

            ECPoint[] preCompP = wnafPreCompP.PreComp;
            ECPoint[] preCompQ = new ECPoint[preCompP.Length];
            for (int i = 0; i < preCompP.Length; ++i)
            {
                preCompQ[i] = pointMap.Map(preCompP[i]);
            }
            wnafPreCompQ.PreComp = preCompQ;

            if (includeNegated)
            {
                ECPoint[] preCompNegQ = new ECPoint[preCompQ.Length];
                for (int i = 0; i < preCompNegQ.Length; ++i)
                {
                    preCompNegQ[i] = preCompQ[i].Negate();
                }
                wnafPreCompQ.PreCompNeg = preCompNegQ;
            }

            c.SetPreCompInfo(q, PRECOMP_NAME, wnafPreCompQ);

            return q;
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
            BigInteger b = (b1 * V1_1) - (b2 * V2_1);

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
