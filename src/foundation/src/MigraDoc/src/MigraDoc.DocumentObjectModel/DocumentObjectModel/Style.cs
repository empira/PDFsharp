// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel.Visitors;

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Represents style templates for paragraph or character formatting.
    /// </summary>
    public sealed class Style : DocumentObject, IVisitable
    {
        /// <summary>
        /// Initializes a new instance of the Style class.
        /// </summary>
        public Style()
        {
            BaseValues = new StyleValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the Style class with the specified parent.
        /// </summary>
        internal Style(DocumentObject parent) : base(parent)
        {
            BaseValues = new StyleValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the Style class with name and base style name.
        /// </summary>
        public Style(string name, string? baseStyleName) : this()
        {
            // baseStyleName can be null or empty
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (name == "")
                throw new ArgumentException(nameof(name));

            Values.Name = name;
            Values.BaseStyle = baseStyleName;
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new Style Clone()
            => (Style)DeepCopy();

        /// <summary>
        /// Implements the deep copy of the object.
        /// </summary>
        protected override object DeepCopy()
        {
            var style = (Style)base.DeepCopy();
            if (style.Values.ParagraphFormat != null)
            {
                style.Values.ParagraphFormat = style.Values.ParagraphFormat.Clone();
                style.Values.ParagraphFormat.Parent = style;
            }
            return style;
        }

        /// <summary>
        /// Returns the value with the specified name and value access.
        /// </summary>
        public override object? GetValue(string name, GV flags)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (name == "")
                throw new ArgumentException(nameof(name));

            if (name.ToLower().StartsWith("font", StringComparison.Ordinal))
                return ParagraphFormat.GetValue(name);

            return base.GetValue(name, flags);
        }

        /// <summary>
        /// Indicates whether the style is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get => _readOnly;
            internal set => _readOnly = value;
        }
        bool _readOnly;

        /// <summary>
        /// Gets the font of ParagraphFormat.
        /// Calling style.Font is just a shortcut to style.ParagraphFormat.Font.
        /// </summary>
        public Font Font // TODO_OLD: Move to Values?
        {
            get => ParagraphFormat.Font;
            // SetParent will be called inside ParagraphFormat.
            set => ParagraphFormat.Font = value;
        }

        /// <summary>
        /// Gets the name of the style.
        /// </summary>
        public string Name => Values.Name ?? "";

        /// <summary>
        /// Gets the ParagraphFormat. To prevent read-only styles from being modified, a copy of its ParagraphFormat
        /// is returned in this case.
        /// </summary>
        public ParagraphFormat ParagraphFormat
        {
            get
            {
                Values.ParagraphFormat ??= new ParagraphFormat(this);
                if (_readOnly)
                    return Values.ParagraphFormat.Clone();  // BUG_OLD: ??? 
                return Values.ParagraphFormat;
            }
            set
            {
                SetParentOf(value);
                Values.ParagraphFormat = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the base style.
        /// </summary>
        public string BaseStyle
        {
            get => Values.BaseStyle ?? "";
            set
            {
                if (value == null || value == "" && !String.IsNullOrEmpty(Values.BaseStyle))
                    throw new ArgumentException(MdDomMsgs.EmptyBaseStyle.Message);

                // Self assignment is allowed. Treat null like "".
                if (value == "" && String.IsNullOrEmpty(Values.BaseStyle)) // BUG_OLD???
                    return;

                // Self assignment is allowed.
                if (String.Compare(Values.BaseStyle, value, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    Values.BaseStyle = value; // character case may change...
                    return;
                }

                if (String.Compare(Values.Name, DefaultParagraphName, StringComparison.OrdinalIgnoreCase) == 0 ||
                    String.Compare(Values.Name, DefaultParagraphFontName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    var msg = $"Style '{Values.Name}' has no base style and that cannot be altered.";
                    throw new ArgumentException(msg);
                }

                var styles = (Styles?)Parent;
                // The base style must exist.
                int idxBaseStyle = styles?.GetIndex(value) ?? -1;  // Prevent exception here. We throw below.
                if (idxBaseStyle == -1)
                {
                    var msg = $"Base style '{value}' does not exist.";
                    throw new ArgumentException(msg);
                }

                if (idxBaseStyle >= 0) // BUG_OLD THHO4STLA Was "idxBaseStyle > 1".
                {
                    // styles cannot be null if idxBaseStyle >= 0.
                    Debug.Assert(styles != null, nameof(styles) + " != null");

                    // Is this style in the base style chain of the new base style?
                    var style = styles[idxBaseStyle];
                    while (style != null)
                    {
                        if (style == this)
                        {
                            var msg = $"Base style '{value}' leads to a circular dependency.";
                            throw new ArgumentException(msg);
                        }

                        style = styles[style.BaseStyle];
                    }
                }

                // Now setting new base style is safe.
                Values.BaseStyle = value;
            }
        }

        /// <summary>
        /// Gets the StyleType of the style.
        /// </summary>
        public StyleType Type
        {
            get
            {
                if (Values.StyleType is null)
                {
                    if (String.Compare(Values.BaseStyle, DefaultParagraphFontName,
                            StringComparison.OrdinalIgnoreCase) == 0)
                        Values.StyleType = StyleType.Character;
                    else
                    {
                        var baseStyle = GetBaseStyle();
                        if (baseStyle == null)
                            throw new InvalidOperationException("User-defined style has no valid base Style.");

                        Values.StyleType = baseStyle.Type;
                    }
                }

                return (StyleType)Values.StyleType;
            }
        }

        /// <summary>
        /// Determines whether the style is the style Normal or DefaultParagraphFont.
        /// </summary>
        internal bool IsRootStyle =>
            String.Compare(Name, DefaultParagraphFontName, StringComparison.OrdinalIgnoreCase) == 0 ||
            String.Compare(Name, DefaultParagraphName, StringComparison.OrdinalIgnoreCase) == 0;

        /// <summary>
        /// Get the BaseStyle of the current style.
        /// </summary>
        public Style? GetBaseStyle()
        {
            if (IsRootStyle)
                return null;

            var styles = Parent as Styles;
            if (styles == null)
                throw new InvalidOperationException("A parent object is required for this operation; access failed");
            if (Values.BaseStyle == "")
                throw new ArgumentException("User-defined Style defined without a BaseStyle");

            //REVIEW KlPo4StLa Special treatment for DefaultParagraphFont faulty (DefaultParagraphFont not returned via styles["name"]).
            if (Values.BaseStyle == DefaultParagraphFontName)
                return styles[0];

            return Values.BaseStyle is not null
                ? styles[Values.BaseStyle]
                : null;
        }

        /// <summary>
        /// Indicates whether the style is a predefined (built-in) style.
        /// </summary>
        public bool BuiltIn => Values.BuiltIn ?? false;

        /// <summary>
        /// Gets or sets a comment associated with this object.
        /// </summary>
        public string Comment
        {
            get => Values.Comment ?? "";
            set => Values.Comment = value;
        }

        // Names of the root styles. Root styles have no BaseStyle.

        /// <summary>
        /// Name of the default character style.
        /// </summary>
        public const string DefaultParagraphFontName = StyleNames.DefaultParagraphFont;

        /// <summary>
        /// Name of the default paragraph style.
        /// </summary>
        public const string DefaultParagraphName = StyleNames.Normal;

        /// <summary>
        /// Converts Style into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            // For built-in styles all properties that differ from their default values
            // are serialized.
            // For user-defined styles all non-null properties are serialized.
            Styles builtInStyles = Styles.BuiltInStyles;
            Style? refStyle;
            //Font? refFont;
            ParagraphFormat? refFormat;

            serializer.WriteComment(Values.Comment);
            if (BuiltIn)
            {
                // BaseStyle is never null, but empty only for "Normal" and "DefaultParagraphFont"
                if (BaseStyle == "")
                {
                    // case: style is "Normal"
                    if (String.Compare(Values.Name, DefaultParagraphName, StringComparison.OrdinalIgnoreCase) != 0)
                        throw new ArgumentException("Internal Error: BaseStyle not set.");

                    refStyle = builtInStyles[builtInStyles.GetIndex(Name)];
                    refFormat = refStyle.ParagraphFormat;
                    //refFont = refFormat.Font;
                    string name = DdlEncoder.QuoteIfNameContainsBlanks(Name);
                    serializer.WriteLineNoCommit(name);
                }
                else
                {
                    // case: any built-in style except "Normal"
                    refStyle = builtInStyles[builtInStyles.GetIndex(Name)];
                    refFormat = refStyle.ParagraphFormat;
                    //refFont = refFormat.Font;
                    if (String.Compare(BaseStyle, refStyle.BaseStyle, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        // case: built-in style with unmodified base style name
                        string name = DdlEncoder.QuoteIfNameContainsBlanks(Name);
                        serializer.WriteLineNoCommit(name);
                        // It’s fine if we have the predefined base style, but...
                        // ...the base style may have been modified or may even have a modified base style.
                        // Methinks it’s wrong to compare with the built-in style, so let’s compare with the
                        // real base style:
                        refStyle = Document.Styles[Document.Styles.GetIndex(Values.BaseStyle!)];  // BUG_OLD: Base style can be null
                        refFormat = refStyle.ParagraphFormat;
                        //refFont = refFormat.Font;
                        // Note: we must write "Underline = none" if the base style has "Underline = single" - we cannot
                        // detect this if we compare with the built-in style that has no underline.
                        // Known problem: Default values like "OutlineLevel = Level1" will now be serialized.
                        // TODO_OLD: Optimize DDL output, remove redundant default values.
                    }
                    else
                    {
                        // case: built-in style with modified base style name
                        string name = DdlEncoder.QuoteIfNameContainsBlanks(Name);
                        string baseName = DdlEncoder.QuoteIfNameContainsBlanks(BaseStyle);
                        serializer.WriteLine(name + " : " + baseName);
                        refStyle = Document.Styles[Document.Styles.GetIndex(Values.BaseStyle!)];// BUG_OLD: Base style can be null
                        refFormat = refStyle.ParagraphFormat;
                        //refFont = refFormat.Font;
                    }
                }
            }
            else
            {
                // case: user-defined style. Base style always exists.
                string name = DdlEncoder.QuoteIfNameContainsBlanks(Name);
                string baseName = DdlEncoder.QuoteIfNameContainsBlanks(BaseStyle);
                serializer.WriteLine(name + " : " + baseName);

#if true
                //var refStyle0 = Document.Styles[Document.Styles.GetIndex(Values.BaseStyle!)];  // BUG_OLD: Base style can be null
                refStyle = Document.Styles[Values.BaseStyle!];  // BUG_OLD: Base style can be null
                refFormat = refStyle?.ParagraphFormat;
#else
                refFormat = null;
#endif
            }

            serializer.BeginContent();

            if (!ParagraphFormat.IsValueNullOrEmpty())
            {
                if (!ParagraphFormat.Values.Font.IsValueNullOrEmpty())
                    Font.Serialize(serializer, refFormat?.Font);

                if (Type == StyleType.Paragraph)
                    ParagraphFormat.Serialize(serializer, "ParagraphFormat", refFormat);
            }

            serializer.EndContent();
        }

        /// <summary>
        /// Sets all properties to Null that have the same value as the base style.
        /// </summary>
        void Optimize()
        {
            // Just here as a reminder to do it...
        }

        /// <summary>
        /// Allows the visitor object to visit the document object and its child objects.
        /// </summary>
        void IVisitable.AcceptVisitor(DocumentObjectVisitor visitor, bool visitChildren)
        {
            visitor.VisitStyle(this);
        }

        /// <summary>
        /// Returns the metaobject of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(Style));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public StyleValues Values => (StyleValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class StyleValues : Values
        {
            internal StyleValues(DocumentObject owner) : base(owner)
            { }

            // If somebody changes the name of a style that is already used (by its name)
            // has bad luck. Code cannot save developers for every possible crap.
            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Name { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? BaseStyle { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public ParagraphFormat? ParagraphFormat { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public StyleType? StyleType { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public bool? BuiltIn { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Comment { get; set; }
        }
    }
}
