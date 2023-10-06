// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.Diagnostics;
using PdfSharp.Pdf.Annotations;
using PdfSharp.Pdf.IO;

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents a number tree node.
    /// </summary>
    [DebuggerDisplay("({DebuggerDisplay})")]
    public sealed class PdfNumberTreeNode : PdfDictionary
    {
        // Reference: 3.8.6  Number Trees / Page 166

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfNumberTreeNode"/> class.
        /// </summary>
        public PdfNumberTreeNode()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfNumberTreeNode"/> class.
        /// </summary>
        public PdfNumberTreeNode(bool isRoot)  //??? Needed HACK StLa
        {
            _isRoot = isRoot;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is a root node.
        /// </summary>
        public bool IsRoot
        {
            get => _isRoot;
            //private setinit => _isRoot = value;
        }
        readonly bool _isRoot;

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
        /// Gets the number of Nums elements.
        /// </summary>
        public int NumsCount
        {
            get
            {
                var names = Elements.GetArray(Keys.Nums);
                // Entries are key / value pairs, so divide by 2.
                return names != null ? names.Elements.Count / 2 : 0;
            }
        }

        /// <summary>
        /// Adds a child node to this node.
        /// </summary>
        public void AddKid(PdfNumberTreeNode kidNode)
        {
            var kids = Elements.GetArray(Keys.Kids);
            if (kids == null)
            {
                kids = new PdfArray();
                Elements.SetObject(Keys.Kids, kids);
            }
            kids.Elements.Add(kidNode);
            _updateRequired = true;
        }

        /// <summary>
        /// Adds a key/value pair to the Nums array of this node.
        /// </summary>
        public void AddNumber(int key, PdfObject value)
        {
            var nums = Elements.GetArray(Keys.Nums);
            if (nums == null)
            {
                nums = new PdfArray();
                Elements.SetObject(Keys.Nums, nums);
            }
            nums.Elements.Add(new PdfInteger(key));
            nums.Elements.Add(value);
            _updateRequired = true;
        }

        /// <summary>
        /// Gets the least key.
        /// </summary>
        public string LeastKey => "todo";

        /// <summary>
        /// Gets the greatest key.
        /// </summary>
        public string GreatestKey => "todo";

        /// <summary>
        /// Updates the limits by inspecting Kids and Names.
        /// </summary>
        void UpdateLimits()
        {
            if (_updateRequired)
            {
                //TODO Recalc Limits
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
            GetType();
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
            // Reference: TABLE 3.34  Entries in a number tree node dictionary / Page 166

            // ReSharper disable InconsistentNaming

            /// <summary>
            /// (Root and intermediate nodes only; required in intermediate nodes;
            /// present in the root node if and only if Nums is not present)
            /// An array of indirect references to the immediate children of this node.
            /// The children may be intermediate or leaf nodes.
            /// </summary>
            [KeyInfo(KeyType.Array)]
            public const string Kids = "/Kids";

            /// <summary>
            /// (Root and leaf nodes only; required in leaf nodes; present in the root node if and only if Kids is not present)
            /// An array of the form
            ///      [key1 value1 key2 value2 … keyn valuen]
            /// where each keyi is an integer and the corresponding valuei is the object associated with that key.
            /// The keys are sorted in numerical order, analogously to the arrangement of keys in a name tree.
            /// </summary>
            [KeyInfo(KeyType.Array)]
            public const string Nums = "/Nums";

            /// <summary>
            /// (Intermediate and leaf nodes only; required)
            /// An array of two integers, specifying the (numerically) least and greatest keys included in the Nums array
            /// of a leaf node or in the Nums arrays of any leaf nodes that are descendants of an intermediate node.
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
    }
}
