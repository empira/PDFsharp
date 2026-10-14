// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.IO;

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

namespace PdfSharp.Pdf.Internal
{
    // TODO implementation
    /// <summary>
    /// Represents a PDF dictionary object xxxxxxxxxxXXXXXXXXXXXXXXXX TODO make ready
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public sealed class PdfDebugDictionary : PdfDictionary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfIntegerObject"/> class.
        /// </summary>
        public PdfDebugDictionary()
        { }

        ///// <summary>
        ///// Initializes a new instance of the <see cref="PdfIntegerObject"/> class.
        ///// </summary>
        //public PdfDebugDictionary(int value)
        //{ }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfIntegerObject"/> class.
        /// </summary>
        public PdfDebugDictionary(PdfDocument document)
            : base(document)
        { }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfDebugDictionary(PdfDictionary dict)
            : base(dict)
        { }

        /// <summary>
        /// Gets or sets the StreamLength of the stream.
        /// Value is not checked and can be different from the actual length of the stream.
        /// </summary>
        public int StreamLength { get; set; }  // TODO Enforce setting

        /// <summary>
        /// Writes the integer literal.
        /// </summary>
        internal override void WriteObject(PdfWriter writer)
        {
            writer.WriteBeginObject(this);

            if (Stream is not null && writer.EffectiveSecurityHandler != null)
            {
                // Encryption could change the size of the stream.
                // Encrypt the bytes before writing the dictionary to get and update the actual size.
                var bytes = (byte[])Stream.Value.Clone();
                writer.EffectiveSecurityHandler.EncryptStream(ref bytes, this);
                Stream.Value = bytes;
            }
            Elements[PdfStream.Keys.Length] = new PdfInteger(StreamLength);
            var keys = Elements.KeyNames;

#if DEBUG
            // Sort keys for debugging purposes. Comparing PDF files with for example programs like
            // Araxis Merge is easier with sorted keys.
            if (writer.Layout == PdfWriterLayout.Verbose)
            {
                var list = new List<PdfName>(keys);
                list.Sort(PdfName.Comparer);
                list.CopyTo(keys, 0);
            }
#endif

            foreach (var key in keys)
                WriteDictionaryElement(writer, key);
            if (Stream != null)
                WriteDictionaryStream(writer);
            writer.WriteEndObject();
        }

        /// <summary>
        /// Gets the DebuggerDisplayAttribute text.
        /// </summary>
        //string DebuggerDisplay => Invariant($"PdfDebugDictionary({ObjectID.DebuggerDisplay},[{Elements.Count}])={_elements?.DebuggerDisplay}");
        string DebuggerDisplay => "TODO";
    }
}

namespace PdfSharp.Pdf.Internal
{
    // TODO implementation

    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    public sealed class PdfDebugArray : PdfArray
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfIntegerObject"/> class.
        /// </summary>
        public PdfDebugArray()
        { }

        ///// <summary>
        ///// Initializes a new instance of the <see cref="PdfIntegerObject"/> class.
        ///// </summary>
        //public PdfDebugDictionary(int value)
        //{ }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfIntegerObject"/> class.
        /// </summary>
        public PdfDebugArray(PdfDocument document)
            : base(document)
        { }

        /// <summary>
        /// Gets the DebuggerDisplayAttribute text.
        /// </summary>
        //string DebuggerDisplay => Invariant($"PdfDebugDictionary({ObjectID.DebuggerDisplay},[{Elements.Count}])={_elements?.DebuggerDisplay}");
        string DebuggerDisplay => "TODO";
    }
}
