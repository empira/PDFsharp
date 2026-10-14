// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

// v7.0.0 Ready

namespace PdfSharp.Pdf.Annotations
{
    /// <summary>
    /// Specifies the annotation flags.
    /// </summary>
    [Flags]
    public enum PdfAnnotationFlags
    {
        // Reference 2.0: 12.5.3  Annotation flags / Page 470

        /// <summary>
        /// Applies only to annotations which do not belong to one of the standard annotation
        /// types and for which no annotation handler is available. If set, do not render the
        /// unknown annotation and do not print it even if the Print flag is set.  If clear,
        /// render such an unknown annotation using an appearance stream specified by its
        /// appearance dictionary, if any.
        /// </summary>
        Invisible = 1 << (1 - 1),

        /// <summary>
        /// (PDF 1.2) If set, do not render the annotation or allow it to interact with the user,
        /// regardless of its annotation type or whether an annotation handler is available.
        /// NOTE 1
        /// In cases where screen space is limited, the ability to hide and show annotations
        /// selectively can be used in combination with appearance streams to render auxiliary
        /// popup information similar in function to online help systems.
        /// </summary>
        Hidden = 1 << (2 - 1),

        /// <summary>
        /// (PDF 1.2) If set, print the annotation when the page is printed unless the Hidden
        /// flag is also set. If clear, never print the annotation, regardless of whether it
        /// is rendered on the screen. If the annotation does not contain any appearance streams
        /// this flag shall be ignored.
        /// NOTE 2
        /// This can be useful for annotations representing interactive push-buttons, which
        /// would serve no meaningful purpose on the printed page.
        /// </summary>
        Print = 1 << (3 - 1),

        /// <summary>
        /// (PDF 1.3) If set, do not scale the annotation’s appearance to match the magnification
        /// of the page. The location of the annotation on the page (defined by the upper-left
        /// corner of its annotation rectangle) shall remain fixed, regardless of the page
        /// magnification. See further discussion following this table.
        /// </summary>
        NoZoom = 1 << (4 - 1),

        /// <summary>
        /// (PDF 1.3) If set, do not rotate the annotation’s appearance to match the rotation of
        /// the page. The upper-left corner of the annotation rectangle shall remain in a fixed
        /// location on the page, regardless of the page rotation. See further discussion
        /// following this table.
        /// </summary>
        NoRotate = 1 << (5 - 1),

        /// <summary>
        /// (PDF 1.3) If set, do not render the annotation on the screen or allow it to interact
        /// with the user. The annotation may be printed (depending on the setting of the Print flag)
        /// but should be considered hidden for purposes of on-screen display and user interaction.
        /// </summary>
        NoView = 1 << (6 - 1),

        /// <summary>
        /// (PDF 1.3) If set, do not allow the annotation to interact with the user. The annotation
        /// may be rendered or printed (depending on the settings of the NoView and Print flags)
        /// but should not respond to mouse clicks or change its appearance in response to mouse
        /// motions.
        /// This flag shall be ignored for widget annotations; its function is subsumed by the
        /// ReadOnly flag of the associated form field.
        /// </summary>
        ReadOnly = 1 << (7 - 1),

        /// <summary>
        /// (PDF 1.4) If set, do not allow the annotation to be deleted or its properties
        /// (including position and size) to be modified by the user. However, this flag does not
        /// restrict changes to the annotation’s contents, such as the value of a form field.
        /// </summary>
        Locked = 1 << (8 - 1),

        /// <summary>
        /// (PDF 1.5) If set, invert the interpretation of the NoView flag for annotation selection
        /// and mouse hovering, causing the annotation to be visible when the mouse pointer hovers
        /// over the annotation or when the annotation is selected.
        /// </summary>
        ToggleNoView = 1 << (9 - 1),

        /// <summary>
        /// (PDF 1.7) If set, do not allow the contents of the annotation to be modified by the user. This
        /// flag does not restrict deletion of the annotation or changes to other annotation properties,
        /// such as position and size.
        /// </summary>
        LockedContents = 1 << (10 - 1),
    }
}
