//namespace HyperLiquid.Net.Signing
//{
//    internal static class InternalSha3Keccack
//    {
//        internal static byte[] CalculateHash(byte[] data)
//        {
//            var digest = new InternalKeccakDigest256();
//            var output = new byte[digest.GetDigestSize()];
//            digest.BlockUpdate(data, data.Length);
//            digest.DoFinal(output, 0);
//            return output;
//        }
//    }
//}