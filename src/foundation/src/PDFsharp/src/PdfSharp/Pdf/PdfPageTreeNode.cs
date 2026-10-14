// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

// v7.0.0 TODO review

namespace PdfSharp.Pdf
{
    /// <summary>
    /// Represents an entry in the documents page tree that is not a leaf.
    /// </summary>
    //[DebuggerDisplay("(PageCount={" + nameof(Count) + "})")]  // TODO #US279
    public class PdfPageTreeNode : PdfDictionary
    {
        // Reference 2.0: 7.7.3.2  Page tree nodes / Page 102

        internal PdfPageTreeNode(PdfDocument document)
            : base(document)
        {
            Elements.SetName(Keys.Type, "/Pages");
            Elements[Keys.Count] = new PdfInteger(0);
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfPageTreeNode(PdfDictionary dictionary)
            : base(dictionary)
        { }

        /// <summary>
        /// Gets the parent of this tree node or null for the root node.
        /// The root node is PdfPages.
        /// </summary>
        public PdfPageTreeNode? Parent
        {
            get  // TODO =>
            {
                var result = Elements.GetDictionary<PdfPageTreeNode>(Keys.Parent);
                return result;
            }
        }

        /// <summary>
        /// Gets the /kids array of this node.
        /// </summary>
        public PdfPageTreeNodes Kids
        {
            get  // TODO =>
            {
                var result = Elements.GetRequiredArray<PdfPageTreeNodes>(Keys.Kids);
                return result;
            }
        }

        public int Count
        {
            get  // TODO =>
            {
                var result = Elements.GetInteger(Keys.Count);
                return result;
            }
        }

        /// <summary>
        /// Add or overrides all inheritable values from this node to the specified values structure.
        /// </summary>
        internal void GetInheritableValues(ref InheritedValues inheritedValues)
        {
#if true  // @@@
            //var res = Elements.GetDictionary(InheritablePageKeys.Resources);
            //if (res != null)
            //    inheritedValues.Resources = res;

            inheritedValues.Resources = Elements.GetDictionary(PdfPage.InheritablePageKeys.Resources);
            inheritedValues.MediaBox = Elements.GetRectangle(PdfPage.InheritablePageKeys.MediaBox);
            inheritedValues.CropBox = Elements.GetRectangle(PdfPage.InheritablePageKeys.CropBox);

            // TODO Should be written simpler.
            var rotate = Elements.GetValue(PdfPage.InheritablePageKeys.Rotate);
            if (rotate is PdfInteger integer)
                inheritedValues.Rotate = integer;

            //inheritedValues.CropBox = Elements.GetValue<PdfRectangle>(InheritablePageKeys.CropBox);
            //inheritedValues.Rotate = Elements.GetValue<PdfInteger>(InheritablePageKeys.Rotate);
#else
            // Old code from PdfPage - DELETE
            var item = page.Elements[InheritablePageKeys.Resources];
            if (item != null)
            {
                if (item is PdfReference reference)
                    values.Resources = (PdfDictionary)(reference.Value);
                else
                    values.Resources = (PdfDictionary)item;
            }

            item = page.Elements.GetValue(InheritablePageKeys.MediaBox);
            if (item != null)
                values.MediaBox = new PdfRectangle(item);

            item = page.Elements.GetValue(InheritablePageKeys.CropBox);
            if (item != null)
                values.CropBox = new PdfRectangle(item);

            item = page.Elements.GetValue(InheritablePageKeys.Rotate);
            if (item != null)
            {
                //if (item is PdfReference)
                //    item = ((PdfReference)item).Value;
                values.Rotate = (PdfInteger)item;
            }
#endif
        }

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public sealed class Keys : KeysBase
        {
            // Reference 2.0: Table 30 — Required entries in a page tree node / Page 103

            /// <summary>
            /// (Required) The type of PDF object that this dictionary describes; shall be Pages for a
            /// page tree node.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Required, FixedValue = "Pages")]
            public const string Type = "/Type";

            /// <summary>
            /// (Required except in root node; not permitted in the root node; shall be an indirect reference)
            /// The page tree node that is the immediate parent of this one.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Required)]
            public const string Parent = "/Parent";

            /// <summary>
            /// (Required) An array of indirect references to the immediate children of this node.
            /// The children shall only be page objects or other page tree nodes.
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Required)]
            public const string Kids = "/Kids";

            /// <summary>
            /// (Required) The number of leaf nodes (page objects) that are descendants of this node
            /// within the page tree.
            /// NOTE
            /// Since the number of pages descendant from a Pages dictionary can be accurately determined
            /// by examining the tree itself using the Kids arrays, the Count entry is redundant.
            /// A PDF writer shall ensure that the value of the Count key is consistent with the number
            /// of entries in the Kids array and its descendants which definitively determines the number
            /// of descendant pages.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Required)]
            public const string Count = "/Count";

            /// <summary>
            /// Gets the KeysMeta for these keys.
            /// </summary>
            internal static DictionaryMeta Meta => _meta ??= CreateMeta(typeof(Keys));

            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;

        /// <summary>
        /// Represents the inheritable entries from a parent node in a page tree.
        /// Used to displace them to the pages in the leafs of the tree.
        /// </summary>
        internal struct InheritedValues
        {
            // Reference 2.0: 7.7.3.4 Inheritance of page attributes / Page 108

            // Note that it must be a struct because the use based on value type semantic.

            public PdfDictionary? Resources;
            public PdfRectangle? MediaBox;
            public PdfRectangle? CropBox;
            public PdfInteger? Rotate;
        }
    }
}
