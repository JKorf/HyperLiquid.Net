using HyperLiquid.Net.Objects.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HyperLiquid.Net.Converters
{
    internal class AccountLedgerConverter : JsonConverter<HyperLiquidAccountLedger>
    {
        public override HyperLiquidAccountLedger? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var result = new HyperLiquidAccountLedger();
            var deposits = new List<HyperLiquidUserLedger<HyperLiquidDeposit>>();
            var withdrawals = new List<HyperLiquidUserLedger<HyperLiquidWithdrawal>>();
            var transfers = new List<HyperLiquidUserLedger<HyperLiquidInternalTransfer>>();
            var liquidations = new List<HyperLiquidUserLedger<HyperLiquidLiquidation>>();
            var spotTransfers = new List<HyperLiquidUserLedger<HyperLiquidSpotTransfer>>();

            var document = JsonDocument.ParseValue(ref reader);
            foreach (var item in document.RootElement.EnumerateArray())
            {
                var type = item.GetProperty("delta").GetProperty("type").GetString();
                switch (type)
                {
                    case "deposit":
                        deposits.Add(item.Deserialize<HyperLiquidUserLedger<HyperLiquidDeposit>>(options)!);
                        break;
                    case "withdrawal":
                        withdrawals.Add(item.Deserialize<HyperLiquidUserLedger<HyperLiquidWithdrawal>>(options)!);
                        break;
                    case "accountClassTransfer":
                        transfers.Add(item.Deserialize<HyperLiquidUserLedger<HyperLiquidInternalTransfer>>(options)!);
                        break;
                    case "liquidation":
                        liquidations.Add(item.Deserialize<HyperLiquidUserLedger<HyperLiquidLiquidation>>(options)!);
                        break;
                    case "spotTransfer":
                        spotTransfers.Add(item.Deserialize<HyperLiquidUserLedger<HyperLiquidSpotTransfer>>(options)!);
                        break;
                }
            }

            result.Deposits = deposits.ToArray();
            result.Withdrawals = withdrawals.ToArray();
            result.InternalTransfer = transfers.ToArray();
            result.Liquidations = liquidations.ToArray();
            result.SpotTransfers = spotTransfers.ToArray();
            return result;
        }

        public override void Write(Utf8JsonWriter writer, HyperLiquidAccountLedger value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var item in value.Withdrawals)
                JsonSerializer.Serialize(writer, item);
            foreach (var item in value.Deposits)
                JsonSerializer.Serialize(writer, item);
            foreach (var item in value.Liquidations)
                JsonSerializer.Serialize(writer, item);
            foreach (var item in value.SpotTransfers)
                JsonSerializer.Serialize(writer, item);
            foreach (var item in value.InternalTransfer)
                JsonSerializer.Serialize(writer, item);
            writer.WriteEndArray();
        }
    }
}
