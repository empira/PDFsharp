﻿// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace PdfSharp.Windows
{
    // Create a host visual derived from the FrameworkElement class.
    // This class provides layout, event handling, and container support for
    // the child visual objects.
    /// <summary>
    /// Used to present Visuals in the PagePreview.
    /// </summary>
    public class VisualPresenter : FrameworkElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VisualPresenter"/> class.
        /// </summary>
        public VisualPresenter()
        {
            _children = new(this);
        }

        /// <summary>
        /// Gets the number of visual child elements within this element.
        /// </summary>
        protected override int VisualChildrenCount
            => _children.Count;

        /// <summary>
        /// Overrides <see cref="M:System.Windows.Media.Visual.GetVisualChild(System.Int32)"/>, and returns a child at the specified index from a collection of child elements.
        /// </summary>
        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _children.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            return _children[index];
        }

        /// <summary>
        /// Gets the children collection.
        /// </summary>
        public VisualCollection Children
            => _children;
        readonly VisualCollection _children;
    }
}
