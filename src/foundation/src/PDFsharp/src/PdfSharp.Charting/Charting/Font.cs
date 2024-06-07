// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Charting
{
    /// <summary>
    /// Font represents the formatting of characters in a paragraph.
    /// </summary>
    public sealed class Font : DocumentObject
    {
        /// <summary>
        /// Initializes a new instance of the Font class that can be used as a template.
        /// </summary>
        public Font()
        { }

        /// <summary>
        /// Initializes a new instance of the Font class with the specified parent.
        /// </summary>
        internal Font(DocumentObject parent) : base(parent)
        { }

        /// <summary>
        /// Initializes a new instance of the Font class with the specified name and size.
        /// </summary>
        public Font(string name, XUnit size) : this()
        {
            Name = name;
            Size = size;
        }

        #region Methods
        /// <summary>
        /// Creates a copy of the Font.
        /// </summary>
        public new Font Clone() => (Font)DeepCopy();

        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the name of the font.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Gets or sets the size of the font.
        /// </summary>
        public XUnit Size { get; set; }

        /// <summary>
        /// Gets or sets the bold property.
        /// </summary>
        public bool Bold { get; set; }

        /// <summary>
        /// Gets or sets the italic property.
        /// </summary>
        public bool Italic { get; set; }

        /// <summary>
        /// Gets or sets the underline property.
        /// </summary>
        public Underline Underline { get; set; }

        /// <summary>
        /// Gets or sets the color property.
        /// </summary>
        public XColor Color { get; set; } = XColor.Empty;

        /// <summary>
        /// Gets or sets the superscript property.
        /// </summary>
        public bool Superscript
        {
            get => _superscript;
            set
            {
                if (_superscript != value)
                {
                    _superscript = value;
                    _subscript = false;
                }
            }
        }
        bool _superscript;

        /// <summary>
        /// Gets or sets the subscript property.
        /// </summary>
        public bool Subscript
        {
            get => _subscript;
            set
            {
                if (_subscript != value)
                {
                    _subscript = value;
                    _superscript = false;
                }
            }
        }
        bool _subscript;
        #endregion
    }
}
