// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using Microsoft.Extensions.Logging;
using PdfSharp.Logging;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;
using System;
using System.Reflection;

// ReSharper disable UnusedMember.Global  // TODO
#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents a node in a name tree.
    /// </summary>
    [DebuggerDisplay("({" + nameof(DebuggerDisplay) + "})")]
    public class PdfNameTreeNode : PdfDictionary
    {
        // Reference 1.7: 3.8.5  Name Trees / Page 161
        // Reference 2.0: 7.9.6  Name trees / Page 119

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfNameTreeNode"/> class.
        /// </summary>
        public PdfNameTreeNode()
        { }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfNameTreeNode(PdfDictionary dict)
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
        public bool IsRoot => Parent == null;

        public bool IsLeaf => Parent != null && Kids == null;

        public bool IsIntermediate => Parent != null && Kids != null;

        public PdfNameTreeKids? Kids
        {
            get
            {
                var kids = Elements.GetValue<PdfNameTreeKids>(Keys.Kids);
                return kids;
            }
        }

        public PdfNameTreeNames? Names
        {
            get
            {
                var names = Elements.GetValue<PdfNameTreeNames>(Keys.Names);
                return names;
            }
        }

        public PdfNameTreeLimits? Limits
        {
            get
            {
                var limits = Elements.GetValue<PdfNameTreeLimits>(Keys.Limits);
                return limits;
            }
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
                // Entries are key-value pairs, so divide by 2.
                return names != null ? names.Elements.Count / 2 : 0;
            }
        }

        /// <summary>
        /// Get the number of names in this node including all children
        /// </summary>
        public int NamesCountTotal => GetNames(true).Count;

        /////// <summary>
        /////// Gets the kids of this item.
        /////// </summary>
        ////public IEnumerable<PdfNameTreeNode> Kids => _kids;

        ////private readonly List<PdfNameTreeNode> _kids = new();

        void Initialize()
        {
#if true
            var kids = Elements.GetValue(Keys.Kids);
            if (kids != null)
            {
                if (kids is PdfArray array)
                {
                    kids = new PdfNameTreeKids(array);
                    Elements[Keys.Kids] = kids;
                }
                else
                {
                    throw new InvalidOperationException("Value of name tree key 'Kids' is not of type PdfArray.");
                }
            }

            var names = Elements.GetValue(Keys.Names);
            if (names != null)
            {
                if (names is PdfArray array)
                {
                    if (names is not PdfNameTreeNames)
                        names = new PdfNameTreeNames(array);
                    Elements[Keys.Names] = names;
                }
                else
                {
                    throw new InvalidOperationException("Value of name tree key 'Names' is not of type PdfArray.");
                }
            }

            var limits = Elements.GetValue(Keys.Limits);
            if (limits != null)
            {
                if (limits is PdfArray array)
                {
                    if (limits is not PdfNameTreeNames)
                        limits = new PdfNameTreeNames(array);
                    Elements[Keys.Limits] = limits;
                }
                else
                {
                    throw new InvalidOperationException("Value of name tree key 'Limits' is not of type PdfArray.");
                }
            }
#else
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
#endif
        }

        /// <summary>
        /// Gets the list of names this node contains
        /// </summary>
        /// <param name="includeKids">Specifies whether the names of the kids should also be returned</param>
        /// <returns>The list of names this node contains</returns>
        /// <remarks>Note that if kids are included, the names are not guaranteed to be sorted</remarks>
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
                var item = Elements.GetValue(Keys.Kids);
                if (item is PdfNumberTreeKids kids)
                {
                    foreach (var kid in kids)
                    {
                        // result.AddRange(kid.GetNames(true));
                        //TODO
                    }
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
                //foreach (var kid in _kids)
                //{
                //    if (!kid.ContainsName(name, true))
                //        return true;
                //}
            }
            return false;
        }
        /// <summary>
        /// Get the value of the item with the specified <paramref name="name"/>.<br></br>
        /// If the value represents a reference, the referenced value is returned.
        /// </summary>
        /// <param name="name">The name whose value should be retrieved</param>
        /// <param name="includeKids">Specifies whether the kids should also be searched</param>
        /// <returns>The value for <paramref name="name"/> when found, otherwise null</returns>
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
                //foreach (var kid in _kids)
                //{
                //    var value = kid.GetValue(name, true);
                //    if (value != null)
                //        return value;
                //}
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
        /// Adds a key-value pair to the Names array of this node.
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
            {
                // Entries are key-value pairs, so add 2.
                i += 2;
            }
            names.Elements.Insert(i, new PdfString(key));
            names.Elements.Insert(i + 1, value);
            _updateRequired = true;  // TODO
        }

        /// <summary>
        /// Gets the least key.
        /// </summary>
        public string LeastKey
        {
            get
            {
                UpdateLimits();
                return _leastKey_;
            }
        }
        string _leastKey_ = "?";

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
        string _greatestKey = "?";

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
                    _leastKey_ = names[0];
                    _greatestKey = names[^1];
                    Elements[Keys.Limits] = new PdfArray(Owner,
                        new PdfString(_leastKey_), new PdfString(_greatestKey));
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

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public sealed class Keys : KeysBase
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
            internal static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));

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
            => $"root:{IsRoot}";
    }

    // TODO
    static class PdfNameTreeNodeHelper
    {
        public static void BuildNameTree(PdfNameTreeNode node)
        {
            var sp = node.ParentInfo;

            //PdfEmbeddedFileStream
        }
    }

    #region Name tree stuff / #NewFile

    public class PdfNameTreeKids : PdfArray
    {
        public PdfNameTreeKids()
        { }

        public PdfNameTreeKids(PdfArray array)
            : base(array)
        { }

        public void Insert(PdfNameTreeNode node)
        {
            // TODO
        }
    }

    public class PdfNameTreeNames : PdfArray
    {
        public PdfNameTreeNames()
        { }

        public PdfNameTreeNames(PdfArray array)
            : base(array)
        {
            //foreach (var item in array)
            //{ }
        }

        public int Count => Elements.Count / 2;

        public NameTreeNameEntry this[int index]
        {
            get
            {
                index *= 2;
                var key = Elements.GetString(index);
                var item = Elements[index + 1];
                var result = new NameTreeNameEntry(key, item);
                return result;
            }
        }

        public PdfItem? this[string key]
        {
            get
            {
                var count = Elements.Count;
                for (int idx = 0; idx < count - 1; idx += 2)
                {
                    var k = Elements.GetString(idx);
                    if (key == k)
                        return Elements[idx + 1];
                }
                return null;
            }
        }

        public string[] NamesKeys
        {
            get
            {
                var count = Elements.Count;
                count >>= 1;  // Suppress odd numbers.
                if (count == 0)
                    return [];

                var keys = new string[count];
                for (int idx = 0; idx < count; idx++)
                {
                    keys[idx] = Elements.GetString(idx * 2);
                }
                return keys;
            }
        }

        internal void TransformItems()
        {
            int count = Elements.Count;
            if (count == 0)
                return;
            if (count % 2 != 0)
                throw new InvalidOperationException("Number of elements in a name tree /Names array must be even.");

            for (int idx = 1; idx < count; idx++)
            {
                var item = Elements[idx];
                //Debug.Assert(item.GetType()==typeof());
            }
        }

        public T TransformType<T>(PdfItem item)
        {
            return default(T)!;
        }

        public void Insert(NameTreeNameEntry nameEntry)
        {
            string key = nameEntry.Key.Value;

            Elements.Insert(0, nameEntry.Key);
            Elements.Insert(1, nameEntry.Value);
            UpdateLimits();
        }

        void UpdateLimits()
        {
            if (ParentInfo != null)
            {
            }
        }
    }

    // Pair of PDF string and PDF item.
    public class NameTreeNameEntry
    {
        public NameTreeNameEntry()
        { }

        public NameTreeNameEntry(PdfString key, PdfItem item)
        {
            Key = key;
            Value = item;
        }

        public NameTreeNameEntry(string key, PdfItem item)
        {
            Key = new(key);
            Value = item;
        }

        public PdfString Key { get; set; } = PdfString.Empty;

        public PdfItem Value { get; set; } = PdfNull.Value;


    }

    public class PdfNameTreeLimits : PdfArray
    {
        public PdfNameTreeLimits()
        {
            Elements.Add(new PdfString());
            Elements.Add(new PdfString());
        }

        protected PdfNameTreeLimits(PdfArray array)
            : base(array)
        {
            EnsureSize();
        }

        public string First
        {
            get => Elements.GetString(0);
            set => Elements[0] = new PdfString(value);
        }

        public string Last
        {
            get => Elements.GetString(1);
            set => Elements[1] = new PdfString(value);
        }

        void EnsureSize()
        {
            if (Elements.Count != 2)
            {
                PdfSharpLogHost.Logger.LogError("NameTreeLimits must have 2 elements.");
            }
        }

    }

    public class PdfNumberTreeKids : PdfArray
    {
        // TODO
    }

    public class PdfNumberTreeNames : PdfArray
    {
        // TODO
    }

    public class PdfNumberTreeLimits : PdfArray
    {
        // TODO
    }

    #endregion
}
