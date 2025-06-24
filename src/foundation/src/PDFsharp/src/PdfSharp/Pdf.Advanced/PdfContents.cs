// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Collections;
using PdfSharp.Pdf.Content.Objects;
using PdfSharp.Pdf.IO;
using static PdfSharp.Pdf.PdfDictionary;

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Represents an array of PDF content streams of a page.
    /// </summary>
    public sealed class PdfContents : PdfArray
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfContents"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        public PdfContents(PdfDocument document)
            : base(document)
        { }

        internal PdfContents(PdfArray array)
            : base(array)
        {
            int count = Elements.Count;
            for (int idx = 0; idx < count; idx++)
            {
                // Convert the references from PdfDictionary to PdfContent
                var item = Elements[idx];
                var iref = item as PdfReference;
                // if (iref != null && iref.Value is PdfDictionary)
                if (iref is { Value: PdfDictionary t1 } t2)
                {
                    // The following line is correct!
                    // ReSharper disable once ObjectCreationAsStatement because assignment is done in constructor.
                    new PdfContent((PdfDictionary)iref.Value);
                }
                else
                    throw new InvalidOperationException("Unexpected item in a content stream array.");
            }
        }

        /// <summary>
        /// Appends a new content stream and returns it.
        /// </summary>
        public PdfContent AppendContent()
        {
            Debug.Assert(Owner != null);

            SetModified();
            PdfContent content = new PdfContent(Owner);
            Owner.IrefTable.Add(content);
            Debug.Assert(content.Reference != null);
            Elements.Add(content.Reference);
            return content;
        }

        /// <summary>
        /// Prepends a new content stream and returns it.
        /// </summary>
        public PdfContent PrependContent()
        {
            Debug.Assert(Owner != null);

            SetModified();
            PdfContent content = new PdfContent(Owner);
            Owner.IrefTable.Add(content);
            Debug.Assert(content.Reference != null);
            Elements.Insert(0, content.Reference);
            return content;
        }

        /// <summary>
        /// Creates a single content stream with the bytes from the array of the content streams.
        /// This operation does not modify any of the content streams in this array.
        /// </summary>
        /// <remarks>This operation actually concats multiple streams by new line</remarks>
        public PdfContent CreateSingleContent()
        {
            var content = new PdfContent(Owner);
            var lineEnding = _document.Options.LineEndingBytes;
            using (var stream = new MemoryStream())
            {
                foreach (PdfItem iref in Elements)
                {
                    var cont = (PdfDictionary)((PdfReference)iref).Value;
                    var data = cont.Stream!.UnfilteredValue;
                    stream.Write(data, 0, data.Length);
                    stream.Write(lineEnding, 0, lineEnding.Length);
                }

                if (stream.Length > 0)
                    stream.SetLength(stream.Length - lineEnding.Length);

                content.Stream = new PdfStream(stream.ToArray(), content);
            }
            return content;
        }

        /// <summary>
        /// Replaces the current content of the page with the specified content sequence.
        /// </summary>
        public PdfContent ReplaceContent(CSequence cseq)
        {
            if (cseq == null)
                throw new ArgumentNullException(nameof(cseq));

            return ReplaceContent(cseq.ToContent());
        }

        /// <summary>
        /// Replaces the current content of the page with the specified bytes.
        /// </summary>
        PdfContent ReplaceContent(byte[] contentBytes)
        {
            Debug.Assert(Owner != null);

            PdfContent content = new PdfContent(Owner);

            content.CreateStream(contentBytes);

            Owner.IrefTable.Add(content);
            Elements.Clear();
            Elements.Add(content.ReferenceNotNull);

            return content;
        }

        void SetModified()
        {
            if (!_modified)
            {
                _modified = true;
                int count = Elements.Count;

                if (count == 1)
                {
                    PdfContent content = (PdfContent)((PdfReference)Elements[0]).Value;
                    content.PreserveGraphicsState();
                }
                else if (count > 1)
                {
                    // Surround content streams with q/Q operations
                    var lineEnding = _document.Options.LineEndingBytes;

                    var content = (PdfContent)((PdfReference)Elements[0]).Value;
                    if (content != null && content.Stream != null)
                    {
                        int written = 0;
                        var length = content.Stream.Length;
                        var value = new byte[length + 1 + lineEnding.Length];
                        
                        value[written] = (byte)'q';
                        written++;

                        Array.Copy(lineEnding, 0, value, written, lineEnding.Length);
                        written += lineEnding.Length;

                        Array.Copy(content.Stream.Value, 0, value, written, length);
                        written += length;
#if DEBUG
                        Debug.Assert(written == value.Length);
#endif
                        content.Stream.Value = value;
                        content.Elements.SetInteger("/Length", value.Length);
                    }

                    content = (PdfContent)((PdfReference)Elements[count - 1]).Value;
                    if (content != null && content.Stream != null)
                    {
                        int written = 0;
                        var length = content.Stream.Length;
                        var value = new byte[length + 2 + lineEnding.Length];

                        Array.Copy(content.Stream.Value, 0, value, written, length);
                        written += length;

                        value[written] = (byte)' ';
                        written++;

                        value[written] = (byte)'Q';
                        written++;

                        Array.Copy(lineEnding, 0, value, written, lineEnding.Length);
                        written += lineEnding.Length;
#if DEBUG
                        Debug.Assert(written == value.Length);
#endif
                        content.Stream.Value = value;
                        content.Elements.SetInteger("/Length", value.Length);
                    }
                }
            }
        }

        bool _modified;

        internal override void WriteObject(PdfWriter writer)
        {
            // Save two bytes in PDF stream...
            if (Elements.Count == 1)
                Elements[0].WriteObject(writer);
            else
                base.WriteObject(writer);
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        public new IEnumerator<PdfContent> GetEnumerator()
        {
            return new PdfPageContentEnumerator(this);
        }

        class PdfPageContentEnumerator : IEnumerator<PdfContent>
        {
            internal PdfPageContentEnumerator(PdfContents list)
            {
                _contents = list;
                _index = -1;
            }

            public bool MoveNext()
            {
                if (_index < _contents.Elements.Count - 1)
                {
                    _index++;
                    _currentElement = (PdfContent)((PdfReference)_contents.Elements[_index]).Value;
                    return true;
                }
                _index = _contents.Elements.Count;
                return false;
            }

            public void Reset()
            {
                _currentElement = null;
                _index = -1;
            }

            object IEnumerator.Current => Current;

            public PdfContent Current
            {
                get
                {
                    if (_index == -1 || _index >= _contents.Elements.Count)
                        throw new InvalidOperationException(PsMsgs.ListEnumCurrentOutOfRange);
                    return _currentElement??throw new InvalidOperationException("Current called before MoveNext.");
                }
            }

            public void Dispose()
            {
                // Nothing to do.
            }

            PdfContent? _currentElement;
            int _index;
            readonly PdfContents _contents;
        }
    }
}
