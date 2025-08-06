// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents a name tree node.
    /// </summary>
    [DebuggerDisplay("({" + nameof(DebuggerDisplay) + "})")]
    public sealed class PdfNameTreeNode : PdfDictionary
    {
        // Reference: 3.8.5  Name Trees / Page 161

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfNameTreeNode"/> class.
        /// </summary>
        public PdfNameTreeNode()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfNameTreeNode"/> class.
        /// </summary>
        public PdfNameTreeNode(PdfDictionary dict)
            : base(dict)
        {
            Initialize();
        }

        /// <summary>
        /// Gets the parent of this node or null if this is the root-node
        /// </summary>
        public PdfNameTreeNode? Parent { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is a root node.
        /// </summary>
        public bool IsRoot
        {
            get => Parent == null;
        }

        /// <summary>
        /// Gets the number of Kids elements.
        /// </summary>
        public int KidsCount
        {
            get
            {
                var kids = Elements.GetArray(Keys.Kids);
                return kids != null ? kids.Elements.Count : 0;
            }
        }

        /// <summary>
        /// Gets the number of Names elements.
        /// </summary>
        public int NamesCount
        {
            get
            {
                var names = Elements.GetArray(Keys.Names);
                // Entries are key / value pairs, so divide by 2.
                return names != null ? names.Elements.Count / 2 : 0;
            }
        }

        /// <summary>
        /// Get the number of names in this node including all children
        /// </summary>
        public int NamesCountTotal => GetNames(true).Count;

        /// <summary>
        /// Gets the kids of this item.
        /// </summary>
        public IEnumerable<PdfNameTreeNode> Kids => _kids;

        private readonly List<PdfNameTreeNode> _kids = new();

        private void Initialize()
        {
            var kids = Elements.GetArray(Keys.Kids);
            if (kids != null)
            {
                for (var i = 0; i < kids.Elements.Count; i++)
                {
                    var kidDict = kids.Elements.GetDictionary(i);
                    if (kidDict != null)
                    {
                        var kid = new PdfNameTreeNode(kidDict) { Parent = this };
                        _kids.Add(kid);
                    }
                }
            }
            _updateRequired = true;
            UpdateLimits();
        }
        /// <summary>
        /// Gets the list of names this node contains
        /// </summary>
        /// <param name="includeKids">Specifies whether the names of the kids should also be returned</param>
        /// <returns>The list of names this node contains</returns>
        /// <remarks>Note: When kids are included, the names are not guaranteed to be sorted</remarks>
        public IReadOnlyList<string> GetNames(bool includeKids = false)
        {
            var result = new List<string>();
            var names = Elements.GetArray(Keys.Names);
            if (names != null)
            {
                for (var i = 0; i < names.Elements.Count; i += 2)
                {
                    result.Add(names.Elements.GetString(i));
                }
            }
            if (includeKids)
            {
                foreach (var kid in _kids)
                {
                    result.AddRange(kid.GetNames(true));
                }
            }
            return result;
        }
        /// <summary>
        /// Determines whether this node contains the specified <paramref name="name"/>
        /// </summary>
        /// <param name="name">The name to search for</param>
        /// <param name="includeKids">Specifies whether the kids should also be searched</param>
        /// <returns>true, if this node contains <paramref name="name"/>, false otherwise</returns>
        public bool ContainsName(string name, bool includeKids = false)
        {
            var names = Elements.GetArray(Keys.Names);
            if (names != null)
            {
                for (var i = 0; i < names.Elements.Count; i += 2)
                {
                    if (string.CompareOrdinal(name, names.Elements.GetString(i)) == 0)
                        return true;
                }
            }
            if (includeKids)
            {
                foreach (var kid in _kids)
                {
                    if (!kid.ContainsName(name, true))
                        return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Get the value of the item with the specified <paramref name="name"/>.<br></br>
        /// If the value represents a reference, the referenced value is returned.
        /// </summary>
        /// <param name="name">The name whose value should be retrieved</param>
        /// <param name="includeKids">Specifies whether the kids should also be searched</param>
        /// <returns>The value for <paramref name="name"/> when found, otwerwise null</returns>
        public PdfItem? GetValue(string name, bool includeKids = false)
        {
            var names = Elements.GetArray(Keys.Names);
            if (names != null)
            {
                for (var i = 0; i < names.Elements.Count; i += 2)
                {
                    if (string.CompareOrdinal(name, names.Elements.GetString(i)) == 0)
                    {
                        var item = names.Elements[i + 1];
                        return item is PdfReference itRef ? itRef.Value : item;
                    }
                }
            }
            if (includeKids)
            {
                foreach (var kid in _kids)
                {
                    var value = kid.GetValue(name, true);
                    if (value != null)
                        return value;
                }
            }
            return null;
        }

        /// <summary>
        /// Adds a child node to this node.
        /// </summary>
        public void AddKid(PdfNameTreeNode kidNode)
        {
            var kids = Elements.GetArray(Keys.Kids);
            if (kids == null)
            {
                kids = new PdfArray();
                Elements.SetObject(Keys.Kids, kids);
            }
            kidNode.Parent = this;
            kids.Elements.Add(kidNode);
            _updateRequired = true;
        }

        /// <summary>
        /// Adds a key/value pair to the Names array of this node.
        /// </summary>
        public void AddName(string key, PdfItem value)
        {
            var names = Elements.GetArray(Keys.Names);
            if (names == null)
            {
                names = new PdfArray();
                Elements.SetObject(Keys.Names, names);
            }

            // Insert names sorted by key.
            int i = 0;
            while (i < names.Elements.Count && string.CompareOrdinal(names.Elements.GetString(i), key) < 0)
                // Entries are key / value pairs, so add 2.
                i += 2;

            names.Elements.Insert(i, new PdfString(key));
            names.Elements.Insert(i + 1, value);
            _updateRequired = true;
        }

        /// <summary>
        /// Gets the least key.
        /// </summary>
        public string LeastKey
        {
            get
            {
                UpdateLimits();
                return _leastKey;
            }
        }
        private string _leastKey = "?";

        /// <summary>
        /// Gets the greatest key.
        /// </summary>
        public string GreatestKey
        {
            get
            {
                UpdateLimits();
                return _greatestKey;
            }
        }
        private string _greatestKey = "?";

        /// <summary>
        /// Updates the limits by inspecting Kids and Names.
        /// </summary>
        void UpdateLimits()
        {
            if (_updateRequired)
            {
                var names = GetNames(true).ToList();
                names.Sort(StringComparer.Ordinal);
                if (names.Count > 0)
                {
                    _leastKey = names[0];
                    _greatestKey = names[^1];
                    Elements[Keys.Limits] = new PdfArray(Owner,
                        new PdfString(_leastKey), new PdfString(_greatestKey));
                }
                _updateRequired = false;
            }
        }

        bool _updateRequired;

        internal override void PrepareForSave()
        {
            UpdateLimits();
            // Check consistence...
            base.PrepareForSave();
        }

        internal override void WriteObject(PdfWriter writer)
        {
            _ = typeof(int);  // Suppress warning for next line.
            base.WriteObject(writer);
        }

        ///// <summary>
        ///// Returns the value in the PDF date format.
        ///// </summary>
        //public override string ToString()
        //{
        //    string delta = _value.ToString("zzz").Replace(':', '\'');
        //    return String.Format("D:{0:yyyyMMddHHmmss}{1}'", _value, delta);
        //}

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        internal sealed class Keys : KeysBase
        {
            // Reference: TABLE 3.33  Entries in a name tree node dictionary / Page 162

            // ReSharper disable InconsistentNaming

            /// <summary>
            /// (Root and intermediate nodes only; required in intermediate nodes; 
            /// present in the root node if and only if Names is not present)
            /// An array of indirect references to the immediate children of this node
            /// The children may be intermediate or leaf nodes.
            /// </summary>
            [KeyInfo(KeyType.Array)]
            public const string Kids = "/Kids";

            /// <summary>
            /// (Root and leaf nodes only; required in leaf nodes; present in the root node if and only if Kids is not present)
            /// An array of the form
            ///      [key1 value1 key2 value2 … keyn valuen]
            /// where each keyi is a string and the corresponding valuei is the object associated with that key.
            /// The keys are sorted in lexical order, as described below.
            /// </summary>
            [KeyInfo(KeyType.Array)]
            public const string Names = "/Names";

            /// <summary>
            /// (Intermediate and leaf nodes only; required)
            /// An array of two strings, specifying the (lexically) least and greatest keys included in the Names array 
            /// of a leaf node or in the Namesarrays of any leaf nodes that are descendants of an intermediate node.
            /// </summary>
            [KeyInfo(KeyType.Array)]
            public const string Limits = "/Limits";

            /// <summary>
            /// Gets the KeysMeta for these keys.
            /// </summary>
            public static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));

            static DictionaryMeta? _meta;

            // ReSharper restore InconsistentNaming
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;

        /// <summary>
        /// Gets the DebuggerDisplayAttribute text.
        /// </summary>
        /// <value>The debugger display.</value>
        // ReSharper disable UnusedMember.Local
        string DebuggerDisplay
        // ReSharper restore UnusedMember.Local
            =>
                String.Format("root:{0}", IsRoot);
    }
}
