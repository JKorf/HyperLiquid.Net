using HyperLiquid.Net.Utils;
using NUnit.Framework;

namespace HyperLiquid.Net.UnitTests
{
    [TestFixture()]
    public class HyperLiquidUtilsTests
    {
        [TestCase(0, false)]        // BTC perp
        [TestCase(9999, false)]
        [TestCase(10000, true)]     // PURR/USDC
        [TestCase(10107, true)]     // HYPE/USDC
        [TestCase(99999, true)]
        [TestCase(110000, false)]   // first asset of builder-deployed perp dex 1
        [TestCase(120005, false)]
        public void IsSpotAssetId_ReturnsExpected(int assetId, bool expected)
        {
            Assert.That(HyperLiquidUtils.IsSpotAssetId(assetId), Is.EqualTo(expected));
        }
    }
}
