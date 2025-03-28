using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace HyperLiquid.Net
{
    internal static class Eip712TypedDataEncoder
    {
        internal static byte[] EncodeTypedDataRaw(TypedDataRaw typedData)
        {
            using (var memoryStream = new MemoryStream())
            using (var writer = new BinaryWriter(memoryStream))
            {
                writer.Write((byte)0x19);  //"1901".HexToByteArray());
                writer.Write((byte)0x01); 
                writer.Write(HashStruct(typedData.Types, "EIP712Domain", typedData.DomainRawValues));
                writer.Write(HashStruct(typedData.Types, typedData.PrimaryType, typedData.Message));

                writer.Flush();
                var result = memoryStream.ToArray();
                return result;
            }
        }

        private static byte[] HashStruct(IDictionary<string, MemberDescription[]> types, string primaryType, IEnumerable<MemberValue> message)
        {
            using (var memoryStream = new MemoryStream())
            using (var writer = new BinaryWriter(memoryStream))
            {
                var encodedType = EncodeType(types, primaryType);
                var typeHash = InternalSha3Keccack.CalculateHash(Encoding.UTF8.GetBytes(encodedType));
                writer.Write(typeHash);

                EncodeData(writer, types, message);

                writer.Flush();
                return InternalSha3Keccack.CalculateHash(memoryStream.ToArray());
            }
        }

        private static string EncodeType(IDictionary<string, MemberDescription[]> types, string typeName)
        {
            var encodedTypes = EncodeTypes(types, typeName);
            var encodedPrimaryType = encodedTypes.Single(x => x.Key == typeName);
            var encodedReferenceTypes = encodedTypes.Where(x => x.Key != typeName).OrderBy(x => x.Key).Select(x => x.Value);
            var fullyEncodedType = encodedPrimaryType.Value + string.Join(string.Empty, encodedReferenceTypes.ToArray());

            return fullyEncodedType;
        }

        private static IList<KeyValuePair<string, string>> EncodeTypes(IDictionary<string, MemberDescription[]> types, string currentTypeName)
        {
            var currentTypeMembers = types[currentTypeName];
            var currentTypeMembersEncoded = currentTypeMembers.Select(x => x.Type + " " + x.Name);
            var result = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(currentTypeName, currentTypeName + "(" + string.Join(",", currentTypeMembersEncoded.ToArray()) + ")")
            };

            result.AddRange(currentTypeMembers.Select(x => ConvertToElementType(x.Type)).Distinct().Where(IsReferenceType).SelectMany(x => EncodeTypes(types, x)));

            return result;
        }
        private static string ConvertToElementType(string type)
        {
            if (type.Contains("["))
            {
                return type.Substring(0, type.IndexOf("["));
            }
            return type;
        }

        internal static bool IsReferenceType(string typeName)
        {
            switch (typeName)
            {
                case var bytes when new Regex("bytes\\d+").IsMatch(bytes):
                case var @uint when new Regex("uint\\d+").IsMatch(@uint):
                case var @int when new Regex("int\\d+").IsMatch(@int):
                case "bytes":
                case "string":
                case "bool":
                case "address":
                    return false;
                case var array when array.Contains("["):
                    return false;
                default:
                    return true;
            }
        }

        private static void EncodeData(BinaryWriter writer, IDictionary<string, MemberDescription[]> types, IEnumerable<MemberValue> memberValues)
        {
            foreach (var memberValue in memberValues)
            {
                switch (memberValue.TypeName)   
                {
                    case var refType when IsReferenceType(refType):
                        {
                            writer.Write(HashStruct(types, memberValue.TypeName, (IEnumerable<MemberValue>)memberValue.Value));
                            break;
                        }
                    case "string":
                        {
                            var value = Encoding.UTF8.GetBytes((string)memberValue.Value);
                            var abiValueEncoded = InternalSha3Keccack.CalculateHash(value);
                            writer.Write(abiValueEncoded);
                            break;
                        }
                    case "bytes":
                        {
                            byte[] value;
                            if (memberValue.Value is string)
                            {
                                value = ((string)memberValue.Value).HexToByteArray();
                            }
                            else
                            {
                                value = (byte[])memberValue.Value;
                            }
                            var abiValueEncoded = InternalSha3Keccack.CalculateHash(value);
                            writer.Write(abiValueEncoded);
                            break;
                        }
                    default:
                        {
                            if (memberValue.TypeName.Contains("["))
                            {
                                var items = (IList)memberValue.Value;
                                var itemsMemberValues = new List<MemberValue>();
                                foreach (var item in items)
                                {
                                    itemsMemberValues.Add(new MemberValue()
                                    {
                                        TypeName = memberValue.TypeName.Substring(0, memberValue.TypeName.LastIndexOf("[")),
                                        Value = item
                                    });
                                }
                                using (var memoryStream = new MemoryStream())
                                using (var writerItem = new BinaryWriter(memoryStream))
                                {
                                    EncodeData(writerItem, types, itemsMemberValues);
                                    writerItem.Flush();
                                    writer.Write(InternalSha3Keccack.CalculateHash(memoryStream.ToArray()));
                                }

                            }
                            else if (memberValue.TypeName.StartsWith("int") || memberValue.TypeName.StartsWith("uint"))
                            {
                                object value;
                                if (memberValue.Value is string)
                                {
                                    BigInteger parsedOutput;
                                    if (BigInteger.TryParse((string)memberValue.Value, out parsedOutput))
                                    {
                                        value = parsedOutput;
                                    }
                                    else
                                    {
                                        value = memberValue.Value;
                                    }
                                }
                                else
                                {
                                    value = memberValue.Value;
                                }
                                var abiValue = new ABIValue(memberValue.TypeName, value);
                                var abiValueEncoded = _abiEncode.GetABIEncoded(abiValue);
                                writer.Write(abiValueEncoded);
                            }
                            else
                            {
                                var abiValue = new ABIValue(memberValue.TypeName, memberValue.Value);
                                var abiValueEncoded = _abiEncode.GetABIEncoded(abiValue);
                                writer.Write(abiValueEncoded);
                            }
                            break;
                        }
                }
            }


        }
        public static byte[] HexToByteArray(this string value)
        {
            {
                byte[] bytes;
                if (string.IsNullOrEmpty(value))
                {
                    bytes = Array.Empty<byte>();
                }
                else
                {
                    var string_length = value.Length;
                    var character_index = value.StartsWith("0x", StringComparison.Ordinal) ? 2 : 0;
                    // Does the string define leading HEX indicator '0x'. Adjust starting index accordingly.               
                    var number_of_characters = string_length - character_index;

                    var add_leading_zero = false;
                    if (0 != number_of_characters % 2)
                    {
                        add_leading_zero = true;

                        number_of_characters += 1; // Leading '0' has been striped from the string presentation.
                    }

                    bytes = new byte[number_of_characters / 2]; // Initialize our byte array to hold the converted string.

                    var write_index = 0;
                    if (add_leading_zero)
                    {
                        bytes[write_index++] = FromCharacterToByte(value[character_index], character_index);
                        character_index += 1;
                    }

                    for (var read_index = character_index; read_index < value.Length; read_index += 2)
                    {
                        var upper = FromCharacterToByte(value[read_index], read_index, 4);
                        var lower = FromCharacterToByte(value[read_index + 1], read_index + 1);

                        bytes[write_index++] = (byte)(upper | lower);
                    }
                }

                return bytes;
            }
        }
        private static byte FromCharacterToByte(char character, int index, int shift = 0)
        {
            var value = (byte)character;
            if (0x40 < value && 0x47 > value || 0x60 < value && 0x67 > value)
            {
                if (0x40 == (0x40 & value))
                    if (0x20 == (0x20 & value))
                        value = (byte)((value + 0xA - 0x61) << shift);
                    else
                        value = (byte)((value + 0xA - 0x41) << shift);
            }
            else if (0x29 < value && 0x40 > value)
            {
                value = (byte)((value - 0x30) << shift);
            }
            else
            {
                throw new FormatException(string.Format(
                    "Character '{0}' at index '{1}' is not valid alphanumeric character.", character, index));
            }

            return value;
        }

    }
}