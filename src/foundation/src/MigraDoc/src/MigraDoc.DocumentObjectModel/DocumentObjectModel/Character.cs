// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

namespace MigraDoc.DocumentObjectModel
{
    /// <summary>
    /// Represents a special character in paragraph text.
    /// </summary>
    // TODO: Change this class and store symbolName and char in different fields.
    public class Character : DocumentObject
    {
        // ===== \space =====
        /// <summary>
        /// A regular blank.
        /// </summary>
        public static readonly Character Blank = new Character(SymbolName.Blank);
        /// <summary>
        /// An n-width blank.
        /// </summary>
        public static readonly Character En = new Character(SymbolName.En);
        /// <summary>
        /// An em-width blank.
        /// </summary>
        public static readonly Character Em = new Character(SymbolName.Em);
        /// <summary>
        /// A blank that is a quarter of em wide.
        /// </summary>
        public static readonly Character EmQuarter = new Character(SymbolName.EmQuarter);
        /// <summary>
        /// A blank that is a quarter of em wide.
        /// </summary>
        public static readonly Character Em4 = new Character(SymbolName.Em4);

        // ===== Used to serialize as \tab, \linebreak =====
        /// <summary>
        /// A tabulator.
        /// </summary>
        public static readonly Character Tab = new Character(SymbolName.Tab);
        /// <summary>
        /// A line break.
        /// </summary>
        public static readonly Character LineBreak = new Character(SymbolName.LineBreak);
        //public static readonly Character MarginBreak         = new Character(SymbolName.MarginBreak);

        // ===== \symbol =====
        /// <summary>
        /// The Euro symbol €.
        /// </summary>
        public static readonly Character Euro = new Character(SymbolName.Euro);
        /// <summary>
        /// The copyright symbol ©.
        /// </summary>
        public static readonly Character Copyright = new Character(SymbolName.Copyright);
        /// <summary>
        /// The trademark symbol ™.
        /// </summary>
        public static readonly Character Trademark = new Character(SymbolName.Trademark);
        /// <summary>
        /// The registered trademark symbol ®.
        /// </summary>
        public static readonly Character RegisteredTrademark = new Character(SymbolName.RegisteredTrademark);
        /// <summary>
        /// The bullet symbol •.
        /// </summary>
        public static readonly Character Bullet = new Character(SymbolName.Bullet);
        /// <summary>
        /// The not symbol ¬.
        /// </summary>
        public static readonly Character Not = new Character(SymbolName.Not);
        /// <summary>
        /// The em dash —.
        /// </summary>
        public static readonly Character EmDash = new Character(SymbolName.EmDash);
        /// <summary>
        /// The en dash –.
        /// </summary>
        public static readonly Character EnDash = new Character(SymbolName.EnDash);
        /// <summary>
        /// A no-break space.
        /// </summary>
        public static readonly Character NonBreakableBlank = new Character(SymbolName.NonBreakableBlank);
        /// <summary>
        /// A no-break space.
        /// </summary>
        public static readonly Character HardBlank = new Character(SymbolName.HardBlank);

        /// <summary>
        /// Initializes a new instance of the Character class.
        /// </summary>
        public Character()
        {
            BaseValues = new CharacterValues(this);
        }

        /// <summary>
        /// Initializes a new instance of the Character class with the specified parent.
        /// </summary>
        internal Character(DocumentObject parent) : base(parent)
        {
            BaseValues = new CharacterValues(this); 
        }

        /// <summary>
        /// Initializes a new instance of the Character class with the specified SymbolName.
        /// </summary>
        Character(SymbolName name) : this()
        {
            Values.SymbolName = name;
        }

        /// <summary>
        /// Gets or sets the SymbolName. Returns 0 if the type is defined by a character.
        /// </summary>
        public SymbolName SymbolName
        {
            get => Values.SymbolName ?? default; // BUG What if value is null? Show ? character
            set => Values.SymbolName = value;
        }

        /// <summary>
        /// Gets or sets the SymbolName as character. Returns 0 if the type is defined via an enum.
        /// </summary>
        public char Char
        {
            get
            {
                if (((uint)(Values.SymbolName ?? 0) & 0xF0000000) == 0)
                    return (char)Values.SymbolName!;
                return '\0';
            }
            set => Values.SymbolName = (SymbolName)value; // BUG
        }

        /// <summary>
        /// Gets or sets the number of times the character is repeated.
        /// </summary>
        public int Count
        {
            get => Values.Count ?? 0;
            set => Values.Count = value;
        }

        /// <summary>
        /// Converts Character into DDL.
        /// </summary>
        internal override void Serialize(Serializer serializer)
        {
            string text = "";
            if (Values.Count != null && Values.Count == 1)
            {
                if (SymbolName == SymbolName.Tab)
                    text = "\\tab ";
                else if (SymbolName == SymbolName.LineBreak)
                    text = "\\linebreak\x0D\x0A";
                else if (SymbolName == SymbolName.ParaBreak)
                    text = "\x0D\x0A\x0D\x0A";
                //else if (symbolType == SymbolName.MarginBreak)
                //  text = "\\marginbreak ";

                if (text != "")
                {
                    serializer.Write(text);
                    return;
                }
            }

            if (((uint)(Values.SymbolName ?? 0) & 0xF0000000) == 0xF0000000)
            {
                // SymbolName == SpaceType?
                if (((uint)(Values.SymbolName ?? 0) & 0xFF000000) == 0xF1000000)
                {
                    if (SymbolName == SymbolName.Blank)
                    {
                        //Note: Don't try to optimize it by leaving away the braces in case a single space is added.
                        //This would lead to confusion with '(' in directly following text.
                        text = "\\space(" + Count + ")";
                    }
                    else
                    {
                        if (Values.Count != null && Values.Count == 1)
                            text = "\\space(" + SymbolName + ")";
                        else
                            text = "\\space(" + SymbolName + ", " + Count + ")";
                    }
                }
                else
                {
                    text = "\\symbol(" + SymbolName + ")";
                }
            }
            else
            {
                // symbolType is a (Unicode) character
                text = " \\chr(0x" + (Values.SymbolName ?? 0).ToString("X") + ")";

            }
            serializer.Write(text);
        }

        /// <summary>
        /// Returns the meta object of this instance.
        /// </summary>
        internal override Meta Meta => TheMeta;

        static readonly Meta TheMeta = new(typeof(Character));

        /// <summary>
        /// Gets the underlying internal nullable values for the properties of this document object.
        /// </summary>
        public CharacterValues Values => (CharacterValues)BaseValues;

        /// <summary>
        /// Contains the internal nullable values for the properties of the enclosing document object class.
        /// </summary>
        public class CharacterValues : Values
        {
            internal CharacterValues(DocumentObject owner) : base(owner)
            { }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public SymbolName? SymbolName { get; set; }

            /// <summary>
            /// Gets or sets the internal nullable implementation value of the enclosing document object property.
            /// See enclosing document object class for documentation of this property.
            /// </summary>
            public int? Count { get; set; } = 1;
        }
    }
}
