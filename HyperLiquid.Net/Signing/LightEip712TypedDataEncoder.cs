//using HyperLiquid.Net.Utils;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Numerics;
//using System.Text;
//using System.Text.RegularExpressions;

//namespace HyperLiquid.Net.Signing
//{
//    internal static class LightEip712TypedDataEncoder
//    {
//        internal static byte[] EncodeTypedDataRaw(TypedDataRaw typedData)
//        {
//            using (var memoryStream = new MemoryStream())
//            using (var writer = new BinaryWriter(memoryStream))
//            {
//                writer.Write((byte)0x19);
//                writer.Write((byte)0x01);
//                writer.Write(HashStruct(typedData.Types, "EIP712Domain", typedData.DomainRawValues));
//                writer.Write(HashStruct(typedData.Types, typedData.PrimaryType, typedData.Message));

//                writer.Flush();
//                var result = memoryStream.ToArray();
//                return result;
//            }
//        }

//        private static byte[] HashStruct(IDictionary<string, MemberDescription[]> types, string primaryType, IEnumerable<MemberValue> message)
//        {
//            using (var memoryStream = new MemoryStream())
//            using (var writer = new BinaryWriter(memoryStream))
//            {
//                var encodedType = EncodeType(types, primaryType);
//                var typeHash = InternalSha3Keccack.CalculateHash(Encoding.UTF8.GetBytes(encodedType));
//                writer.Write(typeHash);

//                EncodeData(writer, types, message);

//                writer.Flush();
//                return InternalSha3Keccack.CalculateHash(memoryStream.ToArray());
//            }
//        }

//        private static string EncodeType(IDictionary<string, MemberDescription[]> types, string typeName)
//        {
//            var encodedTypes = EncodeTypes(types, typeName);
//            var encodedPrimaryType = encodedTypes.Single(x => x.Key == typeName);
//            var encodedReferenceTypes = encodedTypes.Where(x => x.Key != typeName).OrderBy(x => x.Key).Select(x => x.Value);
//            var fullyEncodedType = encodedPrimaryType.Value + string.Join(string.Empty, encodedReferenceTypes.ToArray());

//            return fullyEncodedType;
//        }

//        private static IList<KeyValuePair<string, string>> EncodeTypes(IDictionary<string, MemberDescription[]> types, string currentTypeName)
//        {
//            var currentTypeMembers = types[currentTypeName];
//            var currentTypeMembersEncoded = currentTypeMembers.Select(x => x.Type + " " + x.Name);
//            var result = new List<KeyValuePair<string, string>>
//            {
//                new KeyValuePair<string, string>(currentTypeName, currentTypeName + "(" + string.Join(",", currentTypeMembersEncoded.ToArray()) + ")")
//            };

//            result.AddRange(currentTypeMembers.Select(x => ConvertToElementType(x.Type)).Distinct().Where(IsReferenceType).SelectMany(x => EncodeTypes(types, x)));

//            return result;
//        }
//        private static string ConvertToElementType(string type)
//        {
//            if (type.Contains("["))
//                return type.Substring(0, type.IndexOf("["));
//            return type;
//        }

//        internal static bool IsReferenceType(string typeName)
//        {
//            switch (typeName)
//            {
//                case var bytes when new Regex("bytes\\d+").IsMatch(bytes):
//                case var @uint when new Regex("uint\\d+").IsMatch(@uint):
//                case var @int when new Regex("int\\d+").IsMatch(@int):
//                case "bytes":
//                case "string":
//                case "bool":
//                case "address":
//                    return false;
//                case var array when array.Contains("["):
//                    return false;
//                default:
//                    return true;
//            }
//        }

//        private static void EncodeData(BinaryWriter writer, IDictionary<string, MemberDescription[]> types, IEnumerable<MemberValue> memberValues)
//        {
//            foreach (var memberValue in memberValues)
//            {
//                switch (memberValue.TypeName)
//                {
//                    case var refType when IsReferenceType(refType):
//                        {
//                            writer.Write(HashStruct(types, memberValue.TypeName, (IEnumerable<MemberValue>)memberValue.Value));
//                            break;
//                        }
//                    case "string":
//                        {
//                            var value = Encoding.UTF8.GetBytes((string)memberValue.Value);
//                            var abiValueEncoded = InternalSha3Keccack.CalculateHash(value);
//                            writer.Write(abiValueEncoded);
//                            break;
//                        }
//                    case "bytes":
//                        {
//                            byte[] value;
//                            if (memberValue.Value is string v)
//                            {
//                                value = v.HexToByteArray();
//                            }
//                            else
//                            {
//                                value = (byte[])memberValue.Value;
//                            }

//                            var abiValueEncoded = InternalSha3Keccack.CalculateHash(value);
//                            writer.Write(abiValueEncoded);
//                            break;
//                        }
//                    default:
//                        {
//                            if (memberValue.TypeName.Contains("["))
//                            {
//                                var items = (IList)memberValue.Value;
//                                var itemsMemberValues = new List<MemberValue>();
//                                foreach (var item in items)
//                                {
//                                    itemsMemberValues.Add(new MemberValue()
//                                    {
//                                        TypeName = memberValue.TypeName.Substring(0, memberValue.TypeName.LastIndexOf("[")),
//                                        Value = item
//                                    });
//                                }
//                                using (var memoryStream = new MemoryStream())
//                                using (var writerItem = new BinaryWriter(memoryStream))
//                                {
//                                    EncodeData(writerItem, types, itemsMemberValues);
//                                    writerItem.Flush();
//                                    writer.Write(InternalSha3Keccack.CalculateHash(memoryStream.ToArray()));
//                                }

//                            }
//                            else if (memberValue.TypeName.StartsWith("int") || memberValue.TypeName.StartsWith("uint"))
//                            {
//                                object value;
//                                if (memberValue.Value is string v)
//                                {
//                                    if (BigInteger.TryParse(v, out BigInteger parsedOutput))                                    
//                                        value = parsedOutput;                                    
//                                    else                                    
//                                        value = memberValue.Value;
//                                }
//                                else
//                                {
//                                    value = memberValue.Value;
//                                }

//                                var abiValueEncoded = AbiValueEncodeInt(memberValue.TypeName, value);
//                                writer.Write(abiValueEncoded);
//                            }
//                            else if (memberValue.TypeName == "bool")
//                            {
//                                var abiValueEncoded = AbiValueEncodeInt("uint8", (bool)memberValue.Value ? 1 : 0);
//                                writer.Write(abiValueEncoded);
//                            }
//                            else
//                            {
//                                var abiValueEncoded = AbiValueEncode(memberValue.TypeName, memberValue.Value);
//                                writer.Write(abiValueEncoded);
//                            }
//                            break;
//                        }
//                }
//            }
//        }

//        private static byte[] AbiValueEncodeInt(string typeName, object value)
//        {
//            int size;
//            bool signed = !typeName.StartsWith("u"); // uint versus int
//            if (signed)
//                size = int.Parse(typeName.Substring(3));
//            else            
//                size = int.Parse(typeName.Substring(4));
            
//            if (size == 0)
//                size = 256;

//            var result = new byte[32];
//            BigInteger v;
//            switch (value)
//            {
//                case int i:
//                    v = new BigInteger(i);
//                    break;
//                case long l:
//                    v = new BigInteger(l);
//                    break;
//                case ulong r:
//                    v = new BigInteger(r);
//                    break;
//                default:
//                    v = new BigInteger(0);
//                    break;
//            }

//            if (signed && v < 0)
//            {
//                // Pad with FF
//                for (int i = 0; i < result.Length; i++)
//                {
//                    result[i] = 0xFF;
//                }
//            }

//            var t = v.ToByteArray();
//            if (BitConverter.IsLittleEndian)
//                t = t.AsEnumerable().Reverse().ToArray();

//            t.CopyTo(result, result.Length - t.Length);
//            return result;
//        }

//        private static byte[] AbiValueEncode(string typeName, object value)
//        {
//            byte[] result;
//            switch (typeName)
//            {
//                case "address":
//                case "bytes32":
//                    {
//                        if (value is byte[] t)
//                        {
//                            if (t.Length == 32)
//                                return t;
//                            result = new byte[32];
//                            t.CopyTo(result, result.Length - t.Length);
//                            return result;
//                        }

//                        result = new byte[32];
//                        switch (value)
//                        {
//                            case string s:
//                                {
//                                    var h = s.HexToByteArray();
//                                    h.CopyTo(result, result.Length - h.Length);
//                                    return result;
//                                }
//                        }

//                        return result;
//                    }
//            }

//            return Array.Empty<byte>();
//        }

//    }
//}