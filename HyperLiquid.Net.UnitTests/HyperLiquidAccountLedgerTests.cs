using HyperLiquid.Net.Objects.Models;
using NUnit.Framework;
using System.Text.Json;

namespace HyperLiquid.Net.UnitTests
{
    [TestFixture()]
    public class HyperLiquidAccountLedgerTests
    {
        [Test]
        public void AccountLedger_ParsesWithdrawDelta()
        {
            var json = "[{\"time\":1737370449463,\"hash\":\"0x01\",\"delta\":{\"type\":\"withdraw\",\"usdc\":\"10.5\",\"nonce\":1737370449000,\"fee\":\"1.0\"}},"
                + "{\"time\":1737370449464,\"hash\":\"0x02\",\"delta\":{\"type\":\"deposit\",\"usdc\":\"97.142685\"}}]";

            var ledger = JsonSerializer.Deserialize<HyperLiquidAccountLedger>(json, HyperLiquidExchange._serializerContext);

            Assert.That(ledger, Is.Not.Null);
            Assert.That(ledger!.Withdrawals, Has.Length.EqualTo(1));
            Assert.That(ledger.Withdrawals[0].Data.Usdc, Is.EqualTo(10.5m));
            Assert.That(ledger.Withdrawals[0].Data.Nonce, Is.EqualTo(1737370449000L));
            Assert.That(ledger.Withdrawals[0].Data.Fee, Is.EqualTo(1.0m));
            Assert.That(ledger.Deposits, Has.Length.EqualTo(1));
        }
    }
}
