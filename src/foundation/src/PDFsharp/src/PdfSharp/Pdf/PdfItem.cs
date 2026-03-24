// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.IO;
using System.Runtime.CompilerServices;

#if PDFSHARP_DEBUG
using static PdfSharp.Diagnostics.DebugBreakHelper;
#endif

namespace PdfSharp.Pdf
{
    /// <summary>
    /// The base class of all PDF objects and primitive PDF types.
    /// </summary>
    public abstract class PdfItem : ICloneable
    {
        /// <summary>
        /// Initialized a new instance of this class.
        /// </summary>
        protected PdfItem()
        {
#if PDFSHARP_DEBUG
            InitItemNumber();
#endif
            ItemFlags = ItemFlags.IsPrimitiveItem;
        }

        /// <summary>
        /// Initialized a new instance of this class.
        /// </summary>
        protected PdfItem(PdfItem item)
        {
#if PDFSHARP_DEBUG
            InitItemNumber();
#endif
            ItemFlags = item.ItemFlags;
        }

        object ICloneable.Clone() => Copy();

        /// <summary>
        /// Creates a copy of this object.
        /// </summary>
        public PdfItem Clone() => (PdfItem)Copy();

        /// <summary>
        /// Implements the copy mechanism. Must be overridden in derived classes.
        /// </summary>
#if !PDFSHARP_DEBUG
        protected virtual object Copy() => MemberwiseClone();
#else
        protected virtual object Copy()
        {
            var item = (PdfItem)MemberwiseClone();
            InitItemNumber();
            return item;
        }
#endif

        /// <summary>
        /// When overridden in a derived class, appends a raw string representation of this object
        /// to the specified PdfWriter.
        /// </summary>
        internal abstract void WriteObject(PdfWriter writer);

        /// <summary>
        /// Some low-level flags for making the code more efficient.
        /// </summary>
        internal ItemFlags ItemFlags;

        /// <summary>
        /// Is PdfItem but not PdfObject.
        /// </summary>
        internal bool IsPureItem => (ItemFlags & ItemFlags.IsPrimitiveItem) is not 0;

        /// <summary>
        /// Is PdfObject, but not PdfArray or PdfDictionary.
        /// </summary>
        internal bool IsPureObject => (ItemFlags & ItemFlags.IsCompoundObject) is not 0;

        /// <summary>
        /// Is PdfArray.
        /// </summary>
        internal bool IsArray => (ItemFlags & ItemFlags.IsArray) is not 0;

        /// <summary>
        /// Is PdfDictionary.
        /// </summary>
        internal bool IsDictionary => (ItemFlags & ItemFlags.IsDictionary) is not 0;

        /// <summary>
        /// Is PdfArray or PdfDictionary.
        /// </summary>
        internal bool IsArrayOrDictionary => (ItemFlags & ItemFlags.IsArrayOrDictionary) is not 0;

        internal bool ShouldTryTransformation => (ItemFlags & ItemFlags.TransformationMask) is 0;

        internal bool IsTransformed => (ItemFlags & ItemFlags.IsTransformed) is not 0;

        internal void SetTransformed()
        {
#if PDFSHARP_DEBUG
            if (ShouldBreak5)
                Debugger.Break();
#endif
            ItemFlags |= ItemFlags.IsTransformed;
        }

        internal void SetTransformationTried()
        {
            //ItemFlags &= ItemFlags.ShTransformationWasTried;
            ItemFlags |= ItemFlags.TransformationWasTried;
        }

        internal void SetMustBeIndirect()
        {
#if PDFSHARP_DEBUG
            if (ShouldBreak1)
                Debugger.Break();
#endif
            ItemFlags |= ItemFlags.MustBeIndirect;
        }

        internal bool MustBeIndirect()
        {
#if PDFSHARP_DEBUG
            if (ShouldBreak1)
                Debugger.Break();
#endif
            return (ItemFlags & ItemFlags.MustBeIndirect) is not 0;
        }

        /// <summary>
        /// Gets a value indicating whether a PDF object dead after an
        /// object type transformation.
        /// </summary>
        internal bool IsDead
        {
            //[MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (ItemFlags & ItemFlags.IsDead) is not 0; }
        }

        /// <summary>
        /// Marks a PDF object as dead.
        /// </summary>
        internal void SetDead()
        {
            Debug.Assert(this is PdfContainer);
#if DEBUG_
            //if (this is PdfObject { ObjectNumber: 96049 })
            //    _ = typeof(int);

            //if (ShouldBreak1)
            //    Debugger.Break();
            if (this is PdfArray array)
            {
                if (array.Count() == 4)
                    Debugger.Break();
            }

#endif
#if PDFSHARP_DEBUG
            PdfSharpDebug.Instance.AddDeadContainer((PdfContainer)this);
#endif
            ItemFlags |= ItemFlags.IsDead;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void EnsureAlive()
        {
            if (IsDead)
            {
                throw new InvalidOperationException(
                    $"This instance of {GetType().FullName} cannot be used anymore, because it is dead. " +
                    "This happens when a PDF object is transformed to a derived class and your code still holds " +
                    "a reference to the old instance. This was wrong all along but is detected now.");
            }
        }

#if PDFSHARP_DEBUG
        void InitItemNumber()
        {
            ItemNumber = ++_itemCounter;
        }

        /// <summary>
        /// Gets the unique item count used for debugging purposes.
        /// </summary>
        public int ItemNumber { get; private set; } = 0;

        static int _itemCounter = 0;
#endif
    }
}
