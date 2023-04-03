// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using System.Runtime.ConstrainedExecution;

namespace MigraDoc.DocumentObjectModel
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
        {
            BaseValues = new FontValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the Font class with the specified parent.
        /// </summary>
        internal Font(DocumentObject parent) : base(parent)
        {
            BaseValues = new FontValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the Font class with the specified name and size.
        /// </summary>
        public Font(string name, Unit size)
        {
            BaseValues = new FontValues(this);
            Values.Name = name;
            Values.Size = size;
        }

        /// <summary>
        /// Initializes a new instance of the Font class with the specified name.
        /// </summary>
        public Font(string name)
        {
            BaseValues = new FontValues(this);
            Values.Name = name;
        }

        /// <summary>
        /// Creates a copy of the Font.
        /// </summary>
        public new Font Clone()
            => (Font)DeepCopy();

        /// <summary>
        /// Applies all non-null properties of a font to this font if the given font's property is different from the given refFont's property.
        /// </summary>
        internal void ApplyFont(Font font, Font? refFont)
        {
            if (font == null)
                throw new ArgumentNullException(nameof(font));
            if (font.Values.Name is not null && font.Values.Name != "" && (refFont is null || font.Values.Name != refFont.Values.Name))
                Values.Name = font.Values.Name;

            var f1 = font.Values.Name is not null && font.Values.Name != "";
            var f2 = String.IsNullOrEmpty(font.Values.Name) is false;
            Debug.Assert(f1 == f2);

            if (!font.Values.Size.IsValueNullOrEmpty() && (refFont == null || font.Size != refFont.Size))
                Size = font.Size;

            if (font.Values.Bold is not null && (refFont == null || font.Values.Bold != refFont.Values.Bold))
                Values.Bold = font.Values.Bold;

            if (font.Values.Italic is not null && (refFont == null || font.Italic != refFont.Italic))
                Italic = font.Italic;

            if (Values.Subscript is not null && (refFont == null || font.Subscript != refFont.Subscript))
                Subscript = font.Subscript;
            else if (Values.Superscript is not null && (refFont == null || font.Superscript != refFont.Superscript))
                Superscript = font.Superscript;

            if (Values.Underline is not null && (refFont == null || font.Underline != refFont.Underline))
                Underline = font.Underline;

            if (!Values.Color.IsValueNullOrEmpty() && (refFont == null || font.Color.Argb != refFont.Color.Argb))
                Color = font.Color;
        }

        /// <summary>
        /// Applies all non-null properties of a font to this font.
        /// </summary>
        public void ApplyFont(Font font)
        {
            if (font == null)
                throw new ArgumentNullException(nameof(font));

            if (!String.IsNullOrEmpty(font.Values.Name))
                Values.Name = font.Values.Name;
            if (!font.Values.Size.IsValueNullOrEmpty())
                Size = font.Size;

            if (font.Values.Bold is not null)
                Values.Bold = font.Values.Bold;

            if (font.Values.Italic is not null)
                Italic = font.Italic;

            if (font.Values.Subscript is not null)
                Subscript = font.Subscript;
            else if (font.Values.Superscript is not null)
                Superscript = font.Superscript;

            if (Values.Underline is not null)
                Underline = font.Underline;

            if (!Values.Color.IsValueNullOrEmpty())
                Color = font.Color;
        }

        /// <summary>
        /// Gets or sets the name of the font.
        /// </summary>
        public string Name
        {
            get => Values.Name ?? "";
            set => Values.Name = value;
        }

        /// <summary>
        /// Gets or sets the size of the font.
        /// </summary>
        public Unit Size
        {
            get => Values.Size ?? Unit.Empty;
            set => Values.Size = value;
        }

        /// <summary>
        /// Gets or sets the bold property.
        /// </summary>
        public bool Bold
        {
            get => Values.Bold ?? false;
            set => Values.Bold = value;
        }

        /// <summary>
        /// Gets or sets the italic property.
        /// </summary>
        public bool Italic
        {
            get => Values.Italic ?? false;
            set => Values.Italic = value;
        }

        // TODO Implement Strikethrough for PDFsharp and MigraDoc.
        // THHO4STLA Implementation for Strikethrough in the forum: http://forum.pdfsharp.net/viewtopic.php?p=4636#p4636
        /// <summary>
        /// Gets or sets the underline property.
        /// </summary>
        public Underline Underline
        {
            get => Values.Underline ?? Underline.None;
            set => Values.Underline = value;
        }

        /// <summary>
        /// Gets or sets the color property.
        /// </summary>
        public Color Color
        {
            get => Values.Color ?? Color.Empty;
            set => Values.Color = value;
        }

        /// <summary>
        /// Gets or sets the superscript property.
        /// </summary>
        public bool Superscript
        {
            get => Values.Superscript ?? false;
            set
            {
                Values.Superscript = value;
                Values.Subscript = null;
            }
        }

        /// <summary>
        /// Gets or sets the subscript property.
        /// </summary>
        public bool Subscript
        {
            get => Values.Subscript ?? false;
            set
            {
                Values.Subscript = value;
                Values.Superscript = null;
            }
        }

        //  + .Name = "Arial"
        //  + .Size = 8
        //  + .Bold = False
        //  + .Italic = False
        //  + .Underline = wdUnderlineDouble
        //  * .UnderlineColor = wdColorOrange
        //    .StrikeThrough = False
        //    .DoubleStrikeThrough = False
        //    .Outline = False
        //    .Emboss = False
        //    .Shadow = False
        //    .Hidden = False
        //  * .SmallCaps = False
        //  * .AllCaps = False
        //  + .Color = wdColorAutomatic
        //    .Engrave = False
        //  + .Superscript = False
        //  + .Subscript = False
        //  * .Spacing = 0
        //  * .Scaling = 100
        //  * .Position = 0
        //    .Kerning = 0
        //    .Animation = wdAnimationNone

        ///// <summary>
        ///// Gets a value indicating whether the specified font exists.
        ///// </summary>
        //[Obsolete("This function is removed from DocumentObjectModel and always returns false.")]
        //public static bool Exists(string fontName)
        //{
        //    //System.Drawing.FontFamily[] families = System.Drawing.FontFamily.Families;
        //    //foreach (System.Drawing.FontFamily family in families)
        //    //{
        //    //  if (String.Compare(family.Name, fontName, true) == 0)
        //    //    return true;
        //    //}
        //    return false;
        //}

        /// <summary>
        /// Get a bitmask of all non-null properties.
        /// </summary>
        FontProperties CheckWhatIsNotNull()
        {
            FontProperties fp = FontProperties.None;
            if (Values.Name is not null)
                fp |= FontProperties.Name;
            if (!Values.Size.IsValueNullOrEmpty())
                fp |= FontProperties.Size;
            if (Values.Bold is not null)
                fp |= FontProperties.Bold;
            if (Values.Italic is not null)
                fp |= FontProperties.Italic;
            if (Values.Underline is not null)
                fp |= FontProperties.Underline;
            if (!Values.Color.IsValueNullOrEmpty())
                fp |= FontProperties.Color;
            if (Values.Superscript is not null)
                fp |= FontProperties.Superscript;
            if (Values.Subscript is not null)
                fp |= FontProperties.Subscript;
            return fp;
        }

        /// <summary>
        /// Converts Font into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            Serialize(serializer, null);
        }

        /// <summary>
        /// Converts Font into DDL. Properties with the same value as in an optionally given
        /// font are not serialized.
        /// </summary>
        internal void Serialize(Serializer serializer, Font? font)
        {
            if (Parent is FormattedText)
            {
                string fontStyle = "";
                if (((FormattedText)Parent).Values.Style is null)
                {
                    // Check if we can use a DDL keyword.
                    var notNull = CheckWhatIsNotNull();

                    if (notNull == FontProperties.Size)
                    {
                        serializer.Write(Invariant($"\\fontsize({Size})"));
                        return;
                    }

                    // BUG Check what this code really does...    THHO4STLA: "Check if we can use a DDL keyword."
                    if (notNull == FontProperties.Bold && Bold)  // or if (Values.Bold is true)???
                    {
                        serializer.Write("\\bold");
                        return;
                    }

                    if (notNull == FontProperties.Italic && Italic)
                    {
                        serializer.Write("\\italic");
                        return;
                    }

                    if (notNull == FontProperties.Color)
                    {
                        serializer.Write(Invariant($"\\fontcolor({Color})"));
                        return;
                    }
                }
                else
                    fontStyle = "(\"" + ((FormattedText)Parent).Style + "\")";

                //bool needBlank = false;  // nice, but later...
                serializer.Write("\\font" + fontStyle + "[");

                if (!String.IsNullOrEmpty(Values.Name))
                    serializer.WriteSimpleAttribute("Name", Name);

#if DEBUG_ // Test
                if (!_size.IsNull && Size != 0 && Size.Point == 0)
                    GetType();
#endif

                if (!Values.Size.IsValueNullOrEmpty())
                    serializer.WriteSimpleAttribute("Size", Size);

                if (Values.Bold is not null)
                    serializer.WriteSimpleAttribute("Bold", Bold);

                if (Values.Italic is not null)
                    serializer.WriteSimpleAttribute("Italic", Italic);

                if (Values.Underline is not null)
                    serializer.WriteSimpleAttribute("Underline", Underline);

                if (Values.Superscript is not null)
                    serializer.WriteSimpleAttribute("Superscript", Superscript);

                if (Values.Subscript is not null)
                    serializer.WriteSimpleAttribute("Subscript", Subscript);

                if (!Values.Color.IsValueNullOrEmpty())
                    serializer.WriteSimpleAttribute("Color", Color);
                serializer.Write("]");
            }
            else
            {
                int pos = serializer.BeginContent("Font");

                // Don't write null values if font is null.
                // Do write null values if font is not null!
                var empty = String.IsNullOrEmpty(Values.Name);
                if ((font == null && !empty) ||
                    (font != null && !empty && Values.Name != font.Values.Name))
                {
                    serializer.WriteSimpleAttribute("Name", Name);
                }

#if DEBUG_
                // Test
                if (!_size.IsNull && Size != 0 && Size.Point == 0)
                    GetType();
#endif

                if (!Values.Size.IsValueNullOrEmpty() &&
                    (font == null || Size != font.Size))
                    serializer.WriteSimpleAttribute("Size", Size);
                // NBool and NEnum have to be compared directly to check whether the value is Null.

                if (Values.Bold is not null && (font == null || Bold != font.Bold || font.Values.Bold is null))
                    serializer.WriteSimpleAttribute("Bold", Bold);

                if (Values.Italic is not null && (font == null || Italic != font.Italic || font.Values.Italic is null))
                    serializer.WriteSimpleAttribute("Italic", Italic);

                if (Values.Underline is not null && (font == null || Underline != font.Underline || font.Values.Underline is null))
                    serializer.WriteSimpleAttribute("Underline", Underline);

                if (Values.Superscript is not null && (font == null || Superscript != font.Superscript || font.Values.Superscript is null))
                    serializer.WriteSimpleAttribute("Superscript", Superscript);

                if (Values.Subscript is not null && (font == null || Subscript != font.Subscript || font.Values.Subscript is null))
                    serializer.WriteSimpleAttribute("Subscript", Subscript);

                if (!Values.Color.IsValueNullOrEmpty() && (font == null || Color.Argb != font.Color.Argb))// && Color.RGB != Color.Transparent.RGB)
                    serializer.WriteSimpleAttribute("Color", Color);
                serializer.EndContent(pos);
            }
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(Font));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public FontValues Values => (FontValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class FontValues : Values
        {
            internal FontValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Name { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Unit? Size { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public bool? Bold { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public bool? Italic { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Underline? Underline { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public Color? Color
            {
                get => _color;
                set => _color = DocumentObjectModel.Color.MakeNullIfEmpty(value);
            }
            Color? _color;

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public bool? Superscript { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public bool? Subscript { get; set; }
        }
    }
}
