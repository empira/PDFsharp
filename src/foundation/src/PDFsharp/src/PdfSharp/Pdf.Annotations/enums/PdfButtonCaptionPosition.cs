namespace PdfSharp.Pdf.Annotations.enums
{
    /// <summary>
    /// <see cref="PdfPushButtonField"/>s only:<br></br>
    /// Specifies where to position the text of the widget annotation's caption relative to its icon
    /// </summary>
    public enum PdfButtonCaptionPosition
    {
        /// <summary>
        /// No icon, only caption
        /// </summary>
        CaptionOnly = 0,
        /// <summary>
        /// No caption, icon only
        /// </summary>
        IconOnly,
        /// <summary>
        /// Caption is placed below the icon
        /// </summary>
        BelowIcon,
        /// <summary>
        /// Caption is placed above the icon
        /// </summary>
        AboveIcon,
        /// <summary>
        /// Caption is placed right of the icon
        /// </summary>
        RightOfIcon,
        /// <summary>
        /// Caption is placed left of the icon
        /// </summary>
        LeftOfIcon,
        /// <summary>
        /// Caption is overlaid directly on the icon
        /// </summary>
        Overlaid
    }
}
