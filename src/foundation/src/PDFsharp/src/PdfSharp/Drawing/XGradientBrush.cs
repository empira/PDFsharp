// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.ComponentModel;
using PdfSharp.Internal;
#if GDI
using System.Drawing;
using System.Drawing.Drawing2D;
using GdiLinearGradientBrush =System.Drawing.Drawing2D.LinearGradientBrush;
#endif
#if WPF
using System.Windows;
using System.Windows.Media;
using SysPoint = System.Windows.Point;
using SysSize = System.Windows.Size;
using SysRect = System.Windows.Rect;
using WpfBrush = System.Windows.Media.Brush;
#endif
#if WUI
using Windows.UI;
using Windows.UI.Xaml.Media;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
#endif

namespace PdfSharp.Drawing
{
    /// <summary>
    /// Defines a Brush with a linear gradient.
    /// </summary>
    public abstract class XGradientBrush : XBrush
    {
        /// <summary>
        /// Gets or sets a value indicating whether to extend the gradient beyond its bounds.
        /// </summary>
        public bool ExtendLeft
        {
            get => _extendLeft;
            set => _extendLeft = value;
        }
        internal bool _extendLeft;

        /// <summary>
        /// Gets or sets a value indicating whether to extend the gradient beyond its bounds.
        /// </summary>
        public bool ExtendRight
        {
            get => _extendRight;
            set => _extendRight = value;
        }
        internal bool _extendRight;
    }
}
