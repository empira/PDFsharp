// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// v7.0.0 TODO review

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

namespace PdfSharp.Pdf.Signatures
{
    /// <summary>
    /// An internal stream class used to create digital signatures.
    /// It is based on a stream plus a collection of ranges that define the significant content of this stream.
    /// The ranges are used to exclude one or more areas of the original stream.
    /// </summary>
    class RangedStream : Stream  // StL: Can I say 'RangedStream' in English? SlicedStream? TODO SlicesdStream
    {
        public RangedStream(Stream originalStream, List<Range> ranges)
        {
            if (originalStream.CanRead != true)
                throw new InvalidOperationException("A readable stream is required when creating a signed PDF.");

            if (originalStream.CanSeek != true)
                throw new InvalidOperationException("A seekable stream is required when creating a signed PDF.");

            Stream = originalStream;
            SizeType previousPosition = 0;
            _ranges = ranges.OrderBy(item => item.Offset).ToArray();
            // _ranges = [.. ranges.OrderBy(item => item.Offset)];  // Visual Studio considered this as the simpler expression.
            foreach (var range in ranges)
            {
                if (range.Offset < previousPosition)
                    throw new ArgumentException("Ranges are not continuous.", nameof(range));
                previousPosition = range.Offset + range.Length;
            }
        }

        public override bool CanRead => true;

        public override bool CanSeek => throw new NotImplementedException($"Cannot seek in a {nameof(RangedStream)}.");

        public override bool CanWrite => false;

        public override long Length => _ranges.Sum(item => item.Length);

        IEnumerable<Range> GetPreviousRanges(long position)
            => _ranges.Where(item => item.Offset < position && item.Offset + item.Length < position);

        Range? GetCurrentRange(long position)
            => _ranges.FirstOrDefault(item => item.Offset <= position && item.Offset + item.Length > position);

        public override long Position
        {
            get => GetPreviousRanges(Stream.Position).Sum(item => item.Length) + Stream.Position - GetCurrentRange(Stream.Position)!.Offset;
            set
            {
                Range? currentRange = null;
                List<Range> previousRanges = [];
                SizeType maxPosition = 0;
                foreach (var range in _ranges)
                {
                    currentRange = range;
                    maxPosition += range.Length;
                    if (maxPosition > value)
                        break;
                    previousRanges.Add(range);
                }
                Debug.Assert(currentRange != null);

                SizeType positionInCurrentRange = (SizeType)(value - previousRanges.Sum(item => item.Length));
                Stream.Position = currentRange.Offset + positionInCurrentRange;
            }
        }

        public override void Flush()
            => throw new NotImplementedException(nameof(Flush));

        public override int Read(byte[] buffer, int offset, int count)
        {
            var length = Stream.Length;

            // Possible, but needs extra coding we do not need until there is a real world use case.
            // Write to issues (at) pdfsharp.net if you need super large signed PDF files.
            if (length > Int32.MaxValue)
                throw new NotImplementedException("You currently cannot sign a PDF file larger than 2 GiByte. We'll check if it is possible if someone needs it.");

            int read = 0;
            // Optimized read if possible.
            if (offset == 0 && count == Length)
            {
                foreach (var range in _ranges)
                {
                    Debug.Assert(range.Length <= UInt32.MaxValue, "Signing of large PDF files is not implemented.");
                    int rangeLength = (int)range.Length;
                    Stream.Position = range.Offset;
                    read += Stream.Read(buffer, offset, rangeLength);
                    offset += rangeLength;
                }
                Debug.Assert(read == count);
                return read;
            }

            // We come here e.g. with Bouncy Castle signer.

            // We calculate the current range for each byte in the stream using LINQ.
            // This works, but is very inefficient. If we get performance issues
            // it should be reimplemented by using the ranges here.
            // But this is correct and works, so YAGNI.
            for (int i = 0; i < count; i++)
            {
                if (Stream.Position == length)
                    break;

                PerformSkipIfNeeded();
                read += Stream.Read(buffer, offset++, 1);
            }
            return read;
        }

        void PerformSkipIfNeeded()
        {
            var currentRange = GetCurrentRange(Stream.Position);
            if (currentRange == null)
                Stream.Position = GetNextRange().Offset;
        }

        Range GetNextRange()
            => _ranges.OrderBy(item => item.Offset).First(item => item.Offset > Stream.Position);

        public override long Seek(long offset, SeekOrigin origin)
            => throw new NotImplementedException(nameof(Seek));

        public override void SetLength(long value)
            => throw new NotImplementedException(nameof(SetLength));

        public override void Write(byte[] buffer, int offset, int count)
            => throw new NotImplementedException(nameof(Write));

        readonly Range[] _ranges;

        Stream Stream { get; }

        internal class Range(SizeType offset, SizeType length)
        {
            public SizeType Offset { get; set; } = offset;

            public SizeType Length { get; set; } = length;
        }
    }
}

namespace PdfSharp.Pdf.Internal
{
    public class ManagerBase
    {
        public ManagerBase(PdfDocument document)
        {
            if (document == null!)
                throw new ArgumentNullException(nameof(document));
            Document = document;
        }

        internal void Foo2() { }

        internal bool IsInitialized;

        internal PdfDocument Document { get; }
    }
}

namespace PdfSharp.Pdf.Signatures
{
    using PdfSharp.Pdf.Internal;

    /// <summary>
    /// The PdfAManager bundles PDF/A specific functionality of a PDF document.
    /// </summary>
    public class SigningManager : ManagerBase // RENAME?
    {
        // TODOs:

        /// <summary>
        /// Initialized a new instance of this class for the specified document
        /// </summary>
        /// <param name="document"></param>
        SigningManager(PdfDocument document) : base(document)
        {
            Initialize();
        }

        public string SignatureType { get; } = "(TODO)";  // TODO

        /// <summary>
        /// Gets or creates the PdfAManager for the specified document.
        /// </summary>
        public static SigningManager ForDocument(PdfDocument document)
            => document.SigningManager ??= new(document);

        void Initialize()
        {
            Document.EnsureNotDisposed();
            if (!IsInitialized)
            {
                IsInitialized = true;
                Document.EnsureNotYetSaved();
                if (Document.IsImported)
                {
                    // TODO: Get PDF/A conformance
                }
            }
        }
    }
}
