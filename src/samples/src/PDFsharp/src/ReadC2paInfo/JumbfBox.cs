// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Text;

namespace ReadC2paInfo
{
    /// <summary>
    /// Minimal recursive reader for JUMBF boxes (ISO/IEC 19566-5), the container format used
    /// to store a C2PA manifest store. Only what is needed to navigate a C2PA manifest is
    /// implemented: box type, label (taken from the box's 'jumd' description box), and payload.
    /// </summary>
    class JumbfBox
    {
        public required string Type { get; init; }
        public string? Label { get; set; }
        public ReadOnlyMemory<byte> Payload { get; init; }
        public List<JumbfBox> Children { get; } = [];

        /// <summary>
        /// Finds the first descendant (or this box) whose label equals the given label.
        /// </summary>
        public JumbfBox? FindByLabel(string label)
        {
            if (Label == label)
                return this;
            foreach (var child in Children)
            {
                var found = child.FindByLabel(label);
                if (found is not null)
                    return found;
            }
            return null;
        }

        /// <summary>
        /// Returns the first child box of the given type (e.g. "cbor", "json"), or null.
        /// </summary>
        public JumbfBox? FirstChildOfType(string type) => Children.FirstOrDefault(c => c.Type == type);

        public static List<JumbfBox> Parse(ReadOnlyMemory<byte> data)
        {
            var boxes = new List<JumbfBox>();
            var offset = 0;
            while (data.Length - offset >= 8)
            {
                var span = data.Span;
                var length = (uint)((span[offset] << 24) | (span[offset + 1] << 16) | (span[offset + 2] << 8) | span[offset + 3]);
                var type = Encoding.ASCII.GetString(span.Slice(offset + 4, 4));

                var headerSize = 8;
                var boxLength = (long)length;
                if (length == 1)
                {
                    // 64-bit extended length box (not expected for the manifests we deal with here).
                    var hi = span.Slice(offset + 8, 8);
                    boxLength = ((long)hi[0] << 56) | ((long)hi[1] << 48) | ((long)hi[2] << 40) | ((long)hi[3] << 32)
                                | ((long)hi[4] << 24) | ((long)hi[5] << 16) | ((long)hi[6] << 8) | hi[7];
                    headerSize = 16;
                }

                if (boxLength < headerSize || offset + boxLength > data.Length)
                    break;

                var payload = data.Slice(offset + headerSize, (int)boxLength - headerSize);
                var box = new JumbfBox { Type = type, Payload = payload };

                if (type == "jumb")
                {
                    box.Children.AddRange(Parse(payload));
                    box.Label = box.FirstChildOfType("jumd")?.Label;
                }
                else if (type == "jumd")
                {
                    box.Label = ExtractJumdLabel(payload.Span);
                }

                boxes.Add(box);
                offset += (int)boxLength;
            }
            return boxes;
        }

        // jumd payload: 16-byte UUID, 1-byte toggles, then (if a label is present) a
        // null-terminated UTF-8 label string.
        static string? ExtractJumdLabel(ReadOnlySpan<byte> payload)
        {
            const int uuidAndTogglesSize = 17;
            if (payload.Length <= uuidAndTogglesSize)
                return null;

            var rest = payload[uuidAndTogglesSize..];
            var nullIndex = rest.IndexOf((byte)0);
            if (nullIndex < 0)
                return null;

            return Encoding.UTF8.GetString(rest[..nullIndex]);
        }
    }
}
