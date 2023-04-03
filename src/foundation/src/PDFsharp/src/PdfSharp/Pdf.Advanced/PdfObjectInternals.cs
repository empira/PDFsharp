// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Provides access to the internal PDF object data structures.
    /// This class prevents the public interfaces from pollution with too many internal functions.
    /// </summary>
    public class PdfObjectInternals
    {
        internal PdfObjectInternals(PdfObject obj)
        {
            _obj = obj;
        }

        readonly PdfObject _obj;

        /// <summary>
        /// Gets the object identifier. Returns PdfObjectID.Empty for direct objects.
        /// </summary>
        public PdfObjectID ObjectID => _obj.ObjectID;

        /// <summary>
        /// Gets the object number.
        /// </summary>
        public int ObjectNumber => _obj.ObjectID.ObjectNumber;

        /// <summary>
        /// Gets the generation number.
        /// </summary>
        public int GenerationNumber => _obj.ObjectID.GenerationNumber;

        /// <summary>
        /// Gets the name of the current type.
        /// Not a very useful property, but can be used for data binding.
        /// </summary>
        public string TypeID
        {
            get
            {
                if (_obj is PdfArray)
                    return "array";
                if (_obj is PdfDictionary)
                    return "dictionary";
                return _obj.GetType().Name;
            }
        }
    }
}
