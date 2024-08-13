// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using MigraDoc.DocumentObjectModel.Visitors;

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Represents the collection of all styles.
    /// </summary>
    public class Styles : DocumentObjectCollection, IVisitable
    {
        /// <summary>
        /// Initializes a new instance of the Styles class.
        /// </summary>
        public Styles()
        {
            BaseValues = new StylesValues(this);
            SetupStyles();
        }

        /// <summary>
        /// Initializes a new instance of the Styles class with the specified parent.
        /// </summary>
        internal Styles(DocumentObject parent)
            : base(parent)
        {
            BaseValues = new StylesValues(this);
            SetupStyles();
        }

        /// <summary>
        /// Creates a deep copy of this object.
        /// </summary>
        public new Styles Clone()
            => (Styles)base.DeepCopy();

        /// <summary>
        /// Gets a style by its name, or null if no such style exists.
        /// If name is null, null is returned.
        /// </summary>
        public Style? this[string? styleName]
        {
            get
            {
                if (styleName != null)
                {
                    int count = Count;
                    // index starts from 1; DefaultParagraphFont cannot be modified.
                    for (int index = 1; index < count; index++)
                    {
                        var style = this[index];
                        if (String.Compare(style.Name, styleName, StringComparison.OrdinalIgnoreCase) == 0)
                            return style;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Gets a style by index. 
        /// </summary>
        internal new Style this[int index]
            => (Style?)base[index] ?? throw new InvalidOperationException("Style is null.");

        /// <summary>
        /// Gets the index of a style by name.
        /// </summary>
        /// <param name="styleName">Name of the style looking for.</param>
        /// <returns>Index or -1 if it does not exist.</returns>
        public int GetIndex(string styleName)
        {
            if (styleName == null)
                throw new ArgumentNullException(nameof(styleName));

            int count = Count;
            for (int index = 0; index < count; index++)
            {
                var style = this[index];
                if (String.Compare(style.Name, styleName, StringComparison.OrdinalIgnoreCase) == 0)
                    return index;
            }
            return -1;
        }

        /// <summary>
        /// Adds a new style to the styles collection.
        /// </summary>
        /// <param name="name">Name of the style.</param>
        /// <param name="baseStyleName">Name of the base style.</param>
        public Style AddStyle(string name, string baseStyleName)
        {
            if (name == null || baseStyleName == null)
                throw new ArgumentNullException(name == null ? nameof(name) : nameof(baseStyleName));

            if (name == "" || baseStyleName == "")
                throw new ArgumentException(name == "" ? nameof(name) : nameof(baseStyleName));

            var style = new Style();
            style.Values.Name = name;
            style.Values.BaseStyle = baseStyleName;
            Add(style);
            // Add(style) may add a clone of style, therefore we return the style by name.
            return this[name] ?? throw new InvalidOperationException("AddStyle went wrong.");
        }

        /// <summary>
        /// Adds a DocumentObject to the styles collection. Will sometimes add a clone of the DocumentObject, not the object passed as parameter.
        /// </summary>
        public override void Add(DocumentObject? value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value is not Style style)
                throw TH.ArgumentException_StyleExpected(value.GetType());

            bool isRootStyle = style.IsRootStyle;

            if (style.BaseStyle == "" && !isRootStyle)
                throw TH.ArgumentException_UndefinedBaseStyle(style.Name);

            Style? baseStyle = null;
            int styleIndex = GetIndex(style.BaseStyle);

            if (styleIndex != -1)
                baseStyle = this[styleIndex];
            else if (!isRootStyle)
                throw TH.ArgumentException_UndefinedBaseStyle(style.Name);

            if (baseStyle != null)
                style.Values.StyleType = baseStyle.Type;

            int index = GetIndex(style.Name);

            if (index >= 0)
            {
                // Here a clone of the object will be added to the list, not the original object.
                style = style.Clone();
                style.Parent = this;
                ((IList<DocumentObject?>)this)[index] = style;
            }
            else
                base.Add(value);
        }

        /// <summary>
        /// Gets the default paragraph style.
        /// </summary>
        public Style Normal => this[Style.DefaultParagraphName] ?? throw new InvalidOperationException("Access to style DefaultParagraphName failed.");

        /// <summary>
        /// Gets or sets a comment associated with this object.
        /// </summary>
        public string Comment
        {
            get => Values.Comment ?? "";
            set => Values.Comment = value;
        }

        /// <summary>
        /// Initialize the built-in styles.
        /// </summary>
        internal void SetupStyles()
        {
            // First standard style.
            Style style = new(Style.DefaultParagraphFontName, null)
            {
                IsReadOnly = true,
                Values =
                {
                    StyleType = StyleType.Character,
                    BuiltIn = true
                }
            };
            Add(style);

            // Normal 'standard' (Paragraph Style).
            style = new(Style.DefaultParagraphName, null)
            {
                Values =
                {
                    StyleType = (int)StyleType.Paragraph,
                    BuiltIn = true
                },
                Font =
                {
                    Name = "Arial", // Not "Verdana" anymore.
                    Size = 10,
                    Bold = false,
                    Italic = false,
                    Underline = Underline.None,
                    Color = Colors.Black,
                    Subscript = false,
                    Superscript = false
                },
                ParagraphFormat =
                {
                    Alignment = ParagraphAlignment.Left,
                    FirstLineIndent = 0,
                    LeftIndent = 0,
                    RightIndent = 0,
                    KeepTogether = false,
                    KeepWithNext = false,
                    SpaceBefore = 0,
                    SpaceAfter = 0,
                    LineSpacing = 10,
                    LineSpacingRule = LineSpacingRule.Single,
                    OutlineLevel = OutlineLevel.BodyText,
                    PageBreakBefore = false,
                    WidowControl = true
                }
            };
            Add(style);

            // Heading1 'Überschrift 1' (Paragraph Style).
            style = new(StyleNames.Heading1, StyleNames.Normal)
            {
                Values =
                {
                    BuiltIn = true
                },
                ParagraphFormat =
                {
                    OutlineLevel = OutlineLevel.Level1
                }
            };
            Add(style);

            // Heading2 'Überschrift 2' (Paragraph Style).
            style = new(StyleNames.Heading2, StyleNames.Heading1)
            {
                Values =
                {
                    BuiltIn = true
                },
                ParagraphFormat =
                {
                    OutlineLevel = OutlineLevel.Level2
                }
            };
            Add(style);

            // Heading3 'Überschrift 3' (Paragraph Style).
            style = new(StyleNames.Heading3, StyleNames.Heading2)
            {
                Values =
                {
                    BuiltIn = true
                },
                ParagraphFormat =
                {
                    OutlineLevel = OutlineLevel.Level3
                }
            };
            Add(style);

            // Heading4 'Überschrift 4' (Paragraph Style).
            style = new(StyleNames.Heading4, StyleNames.Heading3)
            {
                Values =
                {
                    BuiltIn = true
                },
                ParagraphFormat =
                {
                    OutlineLevel = OutlineLevel.Level4
                }
            };
            Add(style);

            // Heading5 'Überschrift 5' (Paragraph Style).
            style = new(StyleNames.Heading5, StyleNames.Heading4)
            {
                Values =
                {
                    BuiltIn = true
                },
                ParagraphFormat =
                {
                    OutlineLevel = OutlineLevel.Level5
                }
            };
            Add(style);

            // Heading6 'Überschrift 6' (Paragraph Style).
            style = new(StyleNames.Heading6, StyleNames.Heading5)
            {
                Values =
                {
                    BuiltIn = true
                },
                ParagraphFormat =
                {
                    OutlineLevel = OutlineLevel.Level6
                }
            };
            Add(style);

            // Heading7 'Überschrift 7' (Paragraph Style).
            style = new(StyleNames.Heading7, StyleNames.Heading6)
            {
                Values =
                {
                    BuiltIn = true
                },
                ParagraphFormat =
                {
                    OutlineLevel = OutlineLevel.Level7
                }
            };
            Add(style);

            // Heading8 'Überschrift 8' (Paragraph Style).
            style = new(StyleNames.Heading8, StyleNames.Heading7)
            {
                Values =
                {
                    BuiltIn = true
                },
                ParagraphFormat =
                {
                    OutlineLevel = OutlineLevel.Level8
                }
            };
            Add(style);

            // Heading9 'Überschrift 9' (Paragraph Style).
            style = new(StyleNames.Heading9, StyleNames.Heading8)
            {
                Values =
                {
                    BuiltIn = true
                },
                ParagraphFormat =
                {
                    OutlineLevel = OutlineLevel.Level9
                }
            };
            Add(style);

            // List 'Liste' (Paragraph Style).
            style = new(StyleNames.List, StyleNames.Normal)
            {
                Values =
                {
                    BuiltIn = true
                }
            };
            Add(style);

            // Footnote 'Fußnote' (Paragraph Style).
            style = new Style(StyleNames.Footnote, StyleNames.Normal)
            {
                Values =
                {
                    BuiltIn = true
                }
            };
            Add(style);

            // Header 'Kopfzeile' (Paragraph Style).
            style = new Style(StyleNames.Header, StyleNames.Normal)
            {
                Values =
                {
                    BuiltIn = true
                }
            };
            Add(style);

            // -33: Footer 'Fußzeile' (Paragraph Style).
            style = new Style(StyleNames.Footer, StyleNames.Normal)
            {
                Values =
                {
                    BuiltIn = true
                }
            };
            Add(style);

            // Hyperlink 'Hyperlink' (Character Style).
            style = new Style(StyleNames.Hyperlink, StyleNames.DefaultParagraphFont)
            {
                Values =
                {
                    BuiltIn = true
                }
            };
            Add(style);

            // InvalidStyleName 'Ungültiger Formatvorlagenname' (Paragraph Style).
            style = new Style(StyleNames.InvalidStyleName, StyleNames.Normal)
            {
                Values =
                {
                    BuiltIn = true
                },
                Font =
                {
                    Bold = true,
                    Underline = Underline.Dash,
                    Color = new Color(0xFF00FF00)
                }
            };
            Add(style);
        }

        /// <summary>
        /// Converts Styles into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            serializer.WriteComment(Values.Comment);
            int pos = serializer.BeginContent("\\styles");
            {
                // A style can only be added to Styles if its base style exists. Therefore, the
                // styles collection is consistent at any one time by definition. But because it 
                // is possible to change the base style of a style, the sequence of the styles 
                // in the styles collection can be in an order that a style comes before its base
                // style. The styles in an DDL file must be ordered such that each style appears
                // after its base style. We cannot simply reorder the styles collection, because
                // the predefined styles are expected at a fixed position.
                // The solution is to reorder the styles during serialization.
                int count = Count;
                bool[] fSerialized = new bool[count];  // already serialized
                fSerialized[0] = true;                       // consider DefaultParagraphFont as serialized
                bool[] fSerializePending = new bool[count];  // currently serializing
                bool newLine = false;  // gets true if at least one style was written
                                       //Start from 1 and do not serialize DefaultParagraphFont
                for (int index = 1; index < count; index++)
                {
                    if (!fSerialized[index])
                    {
                        //var style = this[index];  // Code not used??
                        SerializeStyle(serializer, index, ref fSerialized, ref fSerializePending, ref newLine);
                    }
                }
            }
            serializer.EndContent(pos);
        }

        /// <summary>
        /// Serializes a style but serializes its base style first (if that was not yet done).
        /// </summary>
        void SerializeStyle(Serializer serializer, int index,
            ref bool[] fSerialized, ref bool[] fSerializePending, ref bool newLine)
        {
            var style = this[index];

            // It is not possible to modify the default paragraph font.
            if (style.Name == Style.DefaultParagraphFontName)
                return;

            // Circular dependencies cannot occur if changing the base style is implemented
            // correctly. But before we proof that, we check it here.
            if (fSerializePending[index])
            {
                var message = $"Circular dependency detected according to style '{style.Name}'.";
                throw new InvalidOperationException(message);
            }

            // Only style 'Normal' has no base style
            if (style.BaseStyle != "")
            {
                int idxBaseStyle = GetIndex(style.BaseStyle);
                if (idxBaseStyle != -1)
                {
                    if (!fSerialized[idxBaseStyle])
                    {
                        fSerializePending[index] = true;
                        SerializeStyle(serializer, idxBaseStyle, ref fSerialized, ref fSerializePending, ref newLine);
                        fSerializePending[index] = false;
                    }
                }
            }
            int pos2 = serializer.BeginBlock();
            if (newLine)
                serializer.WriteLineNoCommit();
            style.Serialize(serializer);
            if (serializer.EndBlock(pos2))
                newLine = true;
            fSerialized[index] = true;
        }

        /// <summary>
        /// Allows the visitor object to visit the document object and its child objects.
        /// </summary>
        void IVisitable.AcceptVisitor(DocumentObjectVisitor visitor, bool visitChildren)
        {
            visitor.VisitStyles(this);

            var visitedStyles = new Dictionary<Style, object>();
            foreach (var style in this)
                VisitStyle(visitedStyles, (Style)style!, visitor, visitChildren); // BUG style may be null and not of appropriate type
        }

        /// <summary>
        /// Ensures that base styles are visited first.
        /// </summary>
        static void VisitStyle(Dictionary<Style, object> visitedStyles, Style style, DocumentObjectVisitor visitor, bool visitChildren)
        {
            if (!visitedStyles.ContainsKey(style))
            {
                var baseStyle = style.GetBaseStyle();
                if (baseStyle != null && !visitedStyles.ContainsKey(baseStyle)) //baseStyle != ""
                    VisitStyle(visitedStyles, baseStyle, visitor, visitChildren);
                ((IVisitable)style).AcceptVisitor(visitor, visitChildren);
                visitedStyles.Add(style, null!);
            }
        }

        /// <summary>
        /// Gets the built-in styles of MigraDoc. The collection is for reference only
        /// and must not be changed.
        /// </summary>
        public static Styles BuiltInStyles { get; } = [];

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(Styles));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public StylesValues Values => (StylesValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class StylesValues : Values
        {
            internal StylesValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public string? Comment { get; set; }
        }
    }
}
