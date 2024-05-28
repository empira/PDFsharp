// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if GDI
using System.Drawing;
using System.Drawing.Drawing2D;
#endif
#if WPF
using System.Windows.Media;
#endif

namespace PdfSharp.Drawing
{
    // In GDI+ the functions Save/Restore, BeginContainer/EndContainer, Transform, SetClip and ResetClip
    // can be combined in any order. E.g. you can set a clip region, save the graphics state, empty the
    // clip region and draw without clipping. Then you can restore to the previous clip region. With PDF
    // this behavior is hard to implement. To solve this problem I first create an automation that keeps track
    // of all clipping paths and the current transformation when the clip path was set. The automation
    // manages a PDF graphics state stack to calculate the desired behavior. It also takes into consideration
    // not to multiply with inverse matrices when the user sets a new transformation matrix.
    // After the design worked on pager I decided not to implement it because it is much too large-scale.
    // Instead I lay down some rules how to use the XGraphics class.
    //
    // * Before you set a transformation matrix save the graphics state (Save) or begin a new container
    //   (BeginContainer).
    // 
    // * Instead of resetting the transformation matrix, call Restore or EndContainer. If you reset the
    //   transformation, in PDF must be multiplied with the inverse matrix. That leads to round off errors
    //   because in PDF file only 3 digits are used and Acrobat internally uses fixed point numbers (until
    //   version 6 or 7 I think).
    //
    // * When no clip path is defined, you can set or intersect a new path.
    //
    // * When a clip path is already defined, you can always intersect with a new one (which leads in general
    //   to a smaller clip region).
    //
    // * When a clip path is already defined, you can only reset it to the empty region (ResetClip) when
    //   the graphics state stack is at the same position as it was when the clip path was defined. Otherwise
    //   an error occurs.
    //
    // Keeping these rules leads to easy to read code and best results in PDF output.

    /// <summary>
    /// Represents the internal state of an XGraphics object.
    /// Used when the state is saved and restored.
    /// </summary>
    class InternalGraphicsState
    {
        public InternalGraphicsState(XGraphics gfx)
        {
            _gfx = gfx;
        }

        public InternalGraphicsState(XGraphics gfx, XGraphicsState state)
        {
            _gfx = gfx;
            State = state;
            State.InternalState = this;
        }

        public InternalGraphicsState(XGraphics gfx, XGraphicsContainer container)
        {
            _gfx = gfx;
            //State = null!; // BUG
            container.InternalState = this;
        }

        /// <summary>
        /// Gets or sets the current transformation matrix.
        /// </summary>
        public XMatrix Transform { get; set; }

        /// <summary>
        /// Called after this instanced was pushed on the internal graphics stack.
        /// </summary>
        public void Pushed()
        {
#if GDI
            // Nothing to do.
#endif
        }

        /// <summary>
        /// Called after this instanced was popped from the internal graphics stack.
        /// </summary>
        public void Popped()
        {
            Invalid = true;
#if GDI
            // Nothing to do.
#endif
#if WPF
            // Pop all objects pushed in this state.
            if (_gfx.TargetContext == XGraphicTargetContext.WPF)
            {
                for (int idx = 0; idx < _transformPushLevel; idx++)
                    _gfx._dc.Pop();
                _transformPushLevel = 0;
                for (int idx = 0; idx < _geometryPushLevel; idx++)
                    _gfx._dc.Pop();
                _geometryPushLevel = 0;
            }
#endif
        }

        public bool Invalid;

#if GDI_
        /// <summary>
        /// The GDI+ GraphicsState if constructed from XGraphicsState.
        /// </summary>
        public GraphicsState GdiGraphicsState;
#endif

#if WPF
        public void PushTransform(MatrixTransform transform)
        {
            _gfx._dc.PushTransform(transform);
            _transformPushLevel++;
        }
        int _transformPushLevel;

        public void PushClip(Geometry geometry)
        {
            _gfx._dc.PushClip(geometry);
            _geometryPushLevel++;
        }
        int _geometryPushLevel;
#endif

        readonly XGraphics _gfx;

        internal XGraphicsState State = default!;
    }
}
