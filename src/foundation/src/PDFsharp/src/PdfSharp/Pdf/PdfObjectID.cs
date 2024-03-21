// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents a PDF object identifier, a pair of object and generation number.
    /// </summary>
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + "}")]
    // ReSharper disable once InconsistentNaming
    public readonly struct PdfObjectID : IComparable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfObjectID"/> class.
        /// </summary>
        /// <param name="objectNumber">The object number.</param>
        public PdfObjectID(int objectNumber)
        {
            Debug.Assert(objectNumber >= 1, "Object number out of range.");
            _objectNumber = objectNumber;
            _generationNumber = 0;
#if DEBUG_
            // Just a place for a breakpoint during debugging.
            if (objectNumber == 5894)
                GetType();
#endif
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfObjectID"/> class.
        /// </summary>
        /// <param name="objectNumber">The object number.</param>
        /// <param name="generationNumber">The generation number.</param>
        public PdfObjectID(int objectNumber, int generationNumber)
        {
            Debug.Assert(objectNumber >= 1, "Object number out of range.");
            //Debug.Assert(generationNumber >= 0 && generationNumber <= 65535, "Generation number out of range.");
#if DEBUG_
            // iText creates generation numbers with a value of 65536... 
            if (generationNumber > 65535)
                Debug.WriteLine(String.Format("Generation number: {0}", generationNumber));
#endif
            _objectNumber = objectNumber;
            _generationNumber = (ushort)generationNumber;
        }

        /// <summary>
        /// Gets or sets the object number.
        /// </summary>
        public int ObjectNumber => _objectNumber;

        readonly int _objectNumber;

        /// <summary>
        /// Gets or sets the generation number.
        /// </summary>
        public int GenerationNumber => _generationNumber;

        readonly ushort _generationNumber;

        /// <summary>
        /// Indicates whether this object is an empty object identifier.
        /// </summary>
        public bool IsEmpty => _objectNumber == 0;

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj is PdfObjectID id)
            {
                if (_objectNumber == id._objectNumber)
                    return _generationNumber == id._generationNumber;
            }
            return false;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode()
            => _objectNumber ^ _generationNumber;

        /// <summary>
        /// Determines whether the two objects are equal.
        /// </summary>
        public static bool operator ==(PdfObjectID left, PdfObjectID right)
            => left.Equals(right);

        /// <summary>
        /// Determines whether the two objects are not equal.
        /// </summary>
        public static bool operator !=(PdfObjectID left, PdfObjectID right)
            => !left.Equals(right);

        /// <summary>
        /// Returns the object and generation numbers as a string.
        /// </summary>
        public override string ToString()
            //return _objectNumber.ToString(CultureInfo.InvariantCulture) + " " + _generationNumber.ToString(CultureInfo.InvariantCulture);
            => Invariant($"{_objectNumber} {_generationNumber}");

        /// <summary>
        /// Creates an empty object identifier.
        /// </summary>
        public static PdfObjectID Empty => new();

        /// <summary>
        /// Compares the current object ID with another object.
        /// </summary>
        public int CompareTo(object? obj)
        {
            if (obj is PdfObjectID id)
            {
                if (_objectNumber == id._objectNumber)
                    return _generationNumber - id._generationNumber;
                return _objectNumber - id._objectNumber;
            }
            return 1;
        }

        /// <summary>
        /// Gets the DebuggerDisplayAttribute text.
        /// </summary>
        internal string DebuggerDisplay => $"id=({ToString()})";
    }
}
