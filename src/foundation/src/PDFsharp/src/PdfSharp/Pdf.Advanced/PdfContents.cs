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
        public PdfContent CreateSingleContent()
        {
            byte[] bytes = [];
            byte[] bytes1;
            byte[] bytes2;
            foreach (PdfItem iref in Elements)
            {
                PdfDictionary cont = (PdfDictionary)((PdfReference)iref).Value;
                bytes1 = bytes;
                bytes2 = cont.Stream!.UnfilteredValue;
                bytes = new byte[bytes1.Length + bytes2.Length + 1];
                bytes1.CopyTo(bytes, 0);
                bytes[bytes1.Length] = (byte)'\n';
                bytes2.CopyTo(bytes, bytes1.Length + 1);
            }
            PdfContent content = new PdfContent(Owner);
            content.Stream = new PdfDictionary.PdfStream(bytes, content);
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
                    byte[] value;
                    int length;
                    PdfContent content = (PdfContent)((PdfReference)Elements[0]).Value;
                    if (content != null && content.Stream != null)
                    {
                        length = content.Stream.Length;
                        value = new byte[length + 2];
                        value[0] = (byte)'q';
                        value[1] = (byte)'\n';
                        Array.Copy(content.Stream.Value, 0, value, 2, length);
                        content.Stream.Value = value;
                        content.Elements.SetInteger("/Length", length + 2);
                    }
                    content = (PdfContent)((PdfReference)Elements[count - 1]).Value;
                    if (content != null && content.Stream != null)
                    {
                        length = content.Stream.Length;
                        value = new byte[length + 3];
                        Array.Copy(content.Stream.Value, 0, value, 0, length);
                        value[length] = (byte)' ';
                        value[length + 1] = (byte)'Q';
                        value[length + 2] = (byte)'\n';
                        content.Stream.Value = value;
                        content.Elements.SetInteger("/Length", length + 3);
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
