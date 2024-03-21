// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using PdfSharp.Pdf.Advanced;

#if true_ // #DELETE

namespace PdfSharp.Pdf.IO
{
    /// <summary>
    /// Represents the stack for the shift-reduce parser. It seems that it is only needed for
    /// reduction of indirect references.
    /// </summary>
    class ShiftStack
    {
#if DEBUG_
        public ShiftStack()
        {
            GetType();
        }
#endif
        public PdfItem[] ToArray(int start, int length)
        {
            var items = new PdfItem[length];
            for (int i = 0, j = start; i < length; i++, j++)
                items[i] = _items[j];
            return items;
        }

        /// <summary>
        /// Gets the stack pointer index.
        /// </summary>
        // ReSharper disable InconsistentNaming
        public int SP
        // ReSharper restore InconsistentNaming
            => _sp;

        /// <summary>
        /// Gets the value at the specified index. Valid index is in range 0 up to sp-1.
        /// </summary>
        public PdfItem this[int index]
        {
            get
            {
                if (index >= _sp)
                    throw new ArgumentOutOfRangeException(nameof(index), index, "Value greater than stack index.");
                return _items[index];
            }
        }

        /// <summary>
        /// Gets an item relative to the current stack pointer. The index must be a negative value (-1, -2, etc.).
        /// </summary>
        public PdfItem GetItem(int relativeIndex)
        {
            if (relativeIndex >= 0 || -relativeIndex > _sp)
                throw new ArgumentOutOfRangeException(nameof(relativeIndex), relativeIndex, "Value out of stack range.");
            return _items[_sp + relativeIndex];
        }

        /// <summary>
        /// Gets an item relative to the current stack pointer. The index must be a negative value (-1, -2, etc.).
        /// </summary>
        public int GetInteger(int relativeIndex)
        {
            if (relativeIndex >= 0 || -relativeIndex > _sp)
                throw new ArgumentOutOfRangeException(nameof(relativeIndex), relativeIndex, "Value out of stack range.");
            return ((PdfInteger)_items[_sp + relativeIndex]).Value;
        }

        /// <summary>
        /// Pushes the specified item onto the stack.
        /// </summary>
        public void Shift(PdfItem item)
        {
#if DEBUG_
            if (item is PdfReference reference)
                reference.GetType();
#endif
            Debug.Assert(item != null);
            _items.Add(item);
            _sp++;
        }

        /// <summary>
        /// Removes the last 'count' items.
        /// </summary>
        public void Reduce(int count)
        {
            if (count > _sp)
                throw new ArgumentException("count causes stack underflow.");
            _items.RemoveRange(_sp - count, count);
            _sp -= count;
        }

        /// <summary>
        /// Replaces the last 'count' items with the specified item.
        /// </summary>
        public void Reduce(PdfItem item, int count)
        {
#if DEBUG_
            if (item is PdfReference reference)
                reference.GetType();
#endif
            Debug.Assert(item != null);
            Reduce(count);
            _items.Add(item);
            _sp++;
        }

        /// <summary>
        /// The stack pointer index. Points to the next free item.
        /// </summary>
        int _sp;

        /// <summary>
        /// An array representing the stack.
        /// </summary>
        readonly List<PdfItem> _items = [];
    }
}
#endif