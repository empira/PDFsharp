// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Formats.Cbor;
using System.Text;

namespace ReadC2paInfo
{
    /// <summary>
    /// A CBOR value tagged with a semantic tag number (RFC 8949 §3.4), e.g. tag 18 for COSE_Sign1.
    /// </summary>
    record CborTagged(ulong Tag, object? Value);

    /// <summary>
    /// Minimal CBOR decoder that turns a CBOR byte sequence into a generic, inspectable .NET object
    /// tree: maps become List&lt;KeyValuePair&lt;object?, object?&gt;&gt; (order preserved, keys are
    /// not required to be strings), arrays become List&lt;object?&gt;, and scalars become
    /// string/byte[]/long/bool/double/null. Good enough to explore or pull specific fields out of a
    /// C2PA claim/assertion/COSE structure without depending on the exact schema.
    /// </summary>
    static class Cbor
    {
        public static object? Decode(ReadOnlyMemory<byte> data)
        {
            var reader = new CborReader(data, CborConformanceMode.Lax);
            return ReadValue(reader);
        }

        static object? ReadValue(CborReader reader)
        {
            var state = reader.PeekState();
            switch (state)
            {
                case CborReaderState.UnsignedInteger:
                case CborReaderState.NegativeInteger:
                    return reader.ReadInt64();
                case CborReaderState.ByteString:
                    return reader.ReadByteString();
                case CborReaderState.TextString:
                    return reader.ReadTextString();
                case CborReaderState.StartArray:
                {
                    reader.ReadStartArray();
                    var list = new List<object?>();
                    while (reader.PeekState() != CborReaderState.EndArray)
                        list.Add(ReadValue(reader));
                    reader.ReadEndArray();
                    return list;
                }
                case CborReaderState.StartMap:
                {
                    reader.ReadStartMap();
                    var map = new List<KeyValuePair<object?, object?>>();
                    while (reader.PeekState() != CborReaderState.EndMap)
                    {
                        var key = ReadValue(reader);
                        var value = ReadValue(reader);
                        map.Add(new(key, value));
                    }
                    reader.ReadEndMap();
                    return map;
                }
                case CborReaderState.Boolean:
                    return reader.ReadBoolean();
                case CborReaderState.Null:
                    reader.ReadNull();
                    return null;
                case CborReaderState.Tag:
                {
                    var tag = reader.ReadTag();
                    return new CborTagged((ulong)tag, ReadValue(reader));
                }
                case CborReaderState.SimpleValue:
                    return reader.ReadSimpleValue();
                case CborReaderState.HalfPrecisionFloat:
                case CborReaderState.SinglePrecisionFloat:
                case CborReaderState.DoublePrecisionFloat:
                    return reader.ReadDouble();
                default:
                    throw new NotSupportedException($"Unsupported CBOR state: {state}");
            }
        }

        /// <summary>
        /// Looks up a value in a decoded CBOR map by a text-string key.
        /// </summary>
        public static object? GetMapValue(object? map, string key)
        {
            if (map is List<KeyValuePair<object?, object?>> pairs)
            {
                foreach (var kv in pairs)
                {
                    if (kv.Key is string s && s == key)
                        return kv.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// Looks up a value in a decoded CBOR map by an integer key (used by COSE headers).
        /// </summary>
        public static object? GetMapValueByIntKey(object? map, long key)
        {
            if (map is List<KeyValuePair<object?, object?>> pairs)
            {
                foreach (var kv in pairs)
                {
                    if (kv.Key is long l && l == key)
                        return kv.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// Renders a decoded CBOR value tree as an indented, human-readable dump.
        /// </summary>
        public static string Dump(object? value)
        {
            var sb = new StringBuilder();
            DumpInto(sb, value, 0);
            return sb.ToString();
        }

        static void DumpInto(StringBuilder sb, object? value, int indent)
        {
            var pad = new string(' ', indent * 2);
            switch (value)
            {
                case List<KeyValuePair<object?, object?>> map:
                    foreach (var kv in map)
                    {
                        var keyText = FormatScalar(kv.Key);
                        if (IsComposite(kv.Value))
                        {
                            sb.AppendLine($"{pad}{keyText}:");
                            DumpInto(sb, kv.Value, indent + 1);
                        }
                        else
                            sb.AppendLine($"{pad}{keyText}: {FormatScalar(kv.Value)}");
                    }
                    break;
                case List<object?> list:
                    for (var i = 0; i < list.Count; i++)
                    {
                        var item = list[i];
                        if (IsComposite(item))
                        {
                            sb.AppendLine($"{pad}[{i}]:");
                            DumpInto(sb, item, indent + 1);
                        }
                        else
                            sb.AppendLine($"{pad}[{i}]: {FormatScalar(item)}");
                    }
                    break;
                default:
                    sb.AppendLine($"{pad}{FormatScalar(value)}");
                    break;
            }
        }

        static bool IsComposite(object? value) => value is List<KeyValuePair<object?, object?>> or List<object?>;

        static string FormatScalar(object? value) => value switch
        {
            null => "null",
            byte[] bytes => $"<{bytes.Length} bytes: {Convert.ToHexString(bytes.AsSpan(0, Math.Min(bytes.Length, 16)))}{(bytes.Length > 16 ? "…" : "")}>",
            CborTagged tagged => $"tag({tagged.Tag}) {FormatScalar(tagged.Value)}",
            _ => value.ToString() ?? "",
        };
    }
}
