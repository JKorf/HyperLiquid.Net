using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using TypedDataRaw = HyperLiquid.Net.Signing.TypedDataRaw;
using MemberValue = HyperLiquid.Net.Signing.MemberValue;
using MemberDescription = HyperLiquid.Net.Signing.MemberDescription;

namespace HyperLiquid.Net.Converters
{
    internal class TypedDataRawConverter : JsonConverter<TypedDataRaw>
    {
        public override TypedDataRaw? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, TypedDataRaw value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("types");
            writer.WriteStartObject();
            
            WriteTypeDefinition(writer, value.PrimaryType, value.Types[value.PrimaryType]);
           
            foreach (var typeEntry in value.Types)
            {
                if (typeEntry.Key != value.PrimaryType && typeEntry.Key != "EIP712Domain")
                {
                    WriteTypeDefinition(writer, typeEntry.Key, typeEntry.Value);
                }
            }

            if (value.Types.ContainsKey("EIP712Domain"))
            {
                WriteTypeDefinition(writer, "EIP712Domain", value.Types["EIP712Domain"]);
            }

            writer.WriteEndObject();

            writer.WritePropertyName("primaryType");
            writer.WriteStringValue(value.PrimaryType);

            writer.WritePropertyName("domain");
            writer.WriteStartObject();
            var domainType = value.Types["EIP712Domain"];
            for (int i = 0; i < domainType.Length; i++)
            {
                writer.WritePropertyName(domainType[i].Name);
                WriteValue(writer, value.DomainRawValues[i].Value, domainType[i].Type, value);
            }
            writer.WriteEndObject();

            writer.WritePropertyName("message");
            writer.WriteStartObject();
            var messageType = value.Types[value.PrimaryType];
            for (int i = 0; i < messageType.Length; i++)
            {
                writer.WritePropertyName(messageType[i].Name);
                WriteValue(writer, value.Message[i].Value, messageType[i].Type, value);
            }
            writer.WriteEndObject();

            writer.WriteEndObject();
        }

        private void WriteValue(Utf8JsonWriter writer, object val, string memberType, TypedDataRaw typedDataRaw)
        {
            if (val == null)
            {
                writer.WriteNullValue();
                return;
            }

            if (memberType.StartsWith("bytes") && !memberType.Contains("["))
            {
                if (val is byte[] bytes)
                {
                    writer.WriteStringValue("0x" + BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant());
                }
                else
                {
                    writer.WriteStringValue(val.ToString());
                }
            }
            else if (memberType.Contains("["))
            {
                writer.WriteStartArray();
                var innerType = memberType.Substring(0, memberType.LastIndexOf("["));
                if (val is System.Collections.IEnumerable enumerable)
                {
                    foreach (var item in enumerable)
                    {
                        if (item is MemberValue[] memberValues)
                        {
                            writer.WriteStartObject();
                            var itemType = typedDataRaw.Types[innerType];
                            for (int i = 0; i < itemType.Length; i++)
                            {
                                writer.WritePropertyName(itemType[i].Name);
                                WriteValue(writer, memberValues[i].Value, itemType[i].Type, typedDataRaw);
                            }
                            writer.WriteEndObject();
                        }
                        else
                        {
                            WriteValue(writer, item, innerType, typedDataRaw);
                        }
                    }
                }
                writer.WriteEndArray();
            }
            else if (IsReferenceType(memberType, typedDataRaw))
            {
                if (val is MemberValue[] memberValues)
                {
                    writer.WriteStartObject();
                    var refType = typedDataRaw.Types[memberType];
                    for (int i = 0; i < refType.Length; i++)
                    {
                        writer.WritePropertyName(refType[i].Name);
                        WriteValue(writer, memberValues[i].Value, refType[i].Type, typedDataRaw);
                    }
                    writer.WriteEndObject();
                }
            }
            else
            {
                switch (val)
                {
                    case string s:
                        writer.WriteStringValue(s);
                        break;
                    case int i:
                        writer.WriteNumberValue(i);
                        break;
                    case long l:
                        writer.WriteNumberValue(l);
                        break;
                    case ulong ul:
                        writer.WriteNumberValue(ul);
                        break;
                    case uint ui:
                        writer.WriteNumberValue(ui);
                        break;
                    case bool b:
                        writer.WriteBooleanValue(b);
                        break;
                    case decimal d:
                        writer.WriteNumberValue(d);
                        break;
                    case double dbl:
                        writer.WriteNumberValue(dbl);
                        break;
                    case float f:
                        writer.WriteNumberValue(f);
                        break;
                    default:
                        JsonSerializer.Serialize(writer, val);
                        break;
                }
            }
        }

        private bool IsReferenceType(string typeName, TypedDataRaw typedDataRaw)
        {
            return typedDataRaw.Types.ContainsKey(typeName);
        }

        private void WriteTypeDefinition(Utf8JsonWriter writer, string typeName, MemberDescription[] members)
        {
            writer.WritePropertyName(typeName);
            writer.WriteStartArray();
            foreach (var member in members)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("name");
                writer.WriteStringValue(member.Name);
                writer.WritePropertyName("type");
                writer.WriteStringValue(member.Type);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }
    }
}
