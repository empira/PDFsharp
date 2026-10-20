namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Base class of document objects that contain text.
    /// </summary>
    public abstract class TextBasedDocumentObject : DocumentObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextBasedDocumentObject"/> class.
        /// </summary>
        /// <param name="textRenderOption">
        /// The text render option.
        /// </param>
        protected TextBasedDocumentObject(TextRenderOption textRenderOption)
        {
            TextRenderOption = textRenderOption;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextBasedDocumentObject"/> class.
        /// </summary>
        /// <param name="parent">
        /// Document object.
        /// </param>
        /// <param name="textRenderOption">
        /// The text render option.
        /// </param>
        protected TextBasedDocumentObject(DocumentObject parent, TextRenderOption textRenderOption) : base(parent)
        {
            TextRenderOption = textRenderOption;
        }

        /// <summary>
        /// Gets or sets the text render option.
        /// </summary>
        public TextRenderOption TextRenderOption { get; private set; }
    }
}