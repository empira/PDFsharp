// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Advanced;

namespace PdfSharp.Pdf
{
    /// <summary>
    /// The base class of all PDF objects and simple PDF types.
    /// </summary>
    public abstract class PdfItem : ICloneable
    {
        /// <summary>Returns this object or if this object is a reference, returns <see cref="PdfReference.Value"/> of the reference.</summary>
        public PdfItem Value => this is PdfReference r ? r.Value : this;
        /// <summary>Returns or creates child with given name.
        /// Note that implementation for this may be provided by specific descendants of the <see cref="PdfItem"/> class but it's not mandatory.
        /// If implementation is not provided by specific type a <see cref="NotSupportedException"/> will be thrown when attempting to use this accessors.</summary>
        public PdfItem this[string name] {
            get => Value.getChildByName(name);
            set => Value.addChildWithName(name, value);
        }

		protected virtual PdfItem getChildByName(String name)
            => throw new NotSupportedException($@"{this.GetType()} does not allow getting children by name.");
		protected virtual void addChildWithName(String name, PdfItem value)
            => throw new NotSupportedException($@"{this.GetType()} does not allow adding children.");


		// All simple types (i.e. derived from PdfItem but not from PdfObject) must be immutable.

		object ICloneable.Clone()
        {
            return Copy();
        }

        /// <summary>
        /// Creates a copy of this object.
        /// </summary>
        public PdfItem Clone() 
            => (PdfItem)Copy();

        /// <summary>
        /// Implements the copy mechanism. Must be overridden in derived classes.
        /// </summary>
        protected virtual object Copy() 
            => MemberwiseClone();

        /// <summary>
        /// When overridden in a derived class, appends a raw string representation of this object
        /// to the specified PdfWriter.
        /// </summary>
        internal abstract void WriteObject(PdfWriter writer);
    }
}
