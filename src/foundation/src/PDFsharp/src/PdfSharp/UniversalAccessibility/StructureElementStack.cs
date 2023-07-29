// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Structure;

namespace PdfSharp.UniversalAccessibility
{
    enum StructureTypeCategory
    {
        GroupingElement,
        BlockLevelStructureElement,
        InlineLevelStructureElement,
        IllustrationElement
    }

    /// <summary>
    /// Base class of items of the structure stack.
    /// </summary>
    class StructureItem
    {
        public StructureBuilder StructureBuilder { get; set; } = default!;

        /// <summary>
        /// True if a user function call issued the creation of this item.
        /// </summary>
        public bool EndMark; // Means: End() pops items as long as in the last one EndMark is true.

        public virtual void BeginItem()
        { }

        public virtual void EndItem()
        { }

        /// <summary>
        /// Called when DrawString is executed on the current XGraphics object.
        /// </summary>
        public virtual void OnDrawString()
        {
            // Nothing to do in base class.
        }

        /// <summary>
        /// Called when a draw method is executed on the current XGraphics object.
        /// </summary>
        public virtual void OnDraw()
        {
            // Nothing to do in base class.
        }
    }

    /// <summary>
    /// Base class of marked content items of the structure stack.
    /// </summary>
    class MarkedContentItem : StructureItem
    {
        public MarkedContentItem(string tag)
        {
            Tag = tag;
        }

        public string Tag;

        /// <summary>
        /// True if content stream was in text mode (BT) when marked content sequence starts;
        /// false otherwise (ET). Used to balance BT and ET before issuing EMC.
        /// </summary>
        public bool InTextMode;

        public override void BeginItem()
        {
            // Save the text mode state before starting the marked content.
            InTextMode = StructureBuilder.UaManager.IsInTextMode();
        }

        public override void EndItem()
        {
            // Restore the previous text mode state.
            if (InTextMode != StructureBuilder.UaManager.IsInTextMode())
            {
                if (InTextMode)
                    StructureBuilder.UaManager.BeginTextMode();
                else
                    StructureBuilder.UaManager.BeginGraphicMode();
            }
        }
    }

    /// <summary>
    /// Represents a marked content stream with MCID.
    /// </summary>
    class MarkedContentItemWithId : MarkedContentItem
    {
        public MarkedContentItemWithId(StructureElementItem item, string tag, int mcid) :
            base(tag)
        {
            StructureElementItem = item;
            Mcid = mcid;
        }

        /// <summary>
        /// The nearest structure element item on the stack.
        /// </summary>
        public StructureElementItem StructureElementItem;

        public int Mcid;

        public override void BeginItem()
        {
            base.BeginItem();
            // Begin marked content.
            StructureBuilder.Content.Write($"{Tag}<</MCID {Mcid}>>BDC\n");
        }

        public override void EndItem()
        {
            // End marked content.
            StructureBuilder.Content.Write("EMC\n");
            base.EndItem();
        }
    }

    /// <summary>
    /// Represents marked content identifying an artifact.
    /// </summary>
    class ArtifactItem : MarkedContentItem
    {
        public ArtifactItem() :
            base("/Artifact")
        { }

        public override void BeginItem()
        {
            base.BeginItem();
            // Begin artifact.
            StructureBuilder.Content.Write("/Artifact BMC\n");
        }

        public override void EndItem()
        {
            // End artifact.
            StructureBuilder.Content.Write("EMC\n");
            base.EndItem();
        }
    }

    /// <summary>
    /// Base class of structure element items of the structure stack.
    /// </summary>
    class StructureElementItem : StructureItem
    {
        protected StructureElementItem(PdfStructureElement element, StructureTypeCategory category)
        {
            Element = element;
            Category = category;
        }

        /// <summary>
        /// The current structure element.
        /// </summary>
        public PdfStructureElement Element;

        /// <summary>
        /// The category of the current structure element.
        /// </summary>
        public StructureTypeCategory Category;
    }

    /// <summary>
    /// Represents all grouping elements.
    /// </summary>
    class GroupingItem : StructureElementItem
    {
        public GroupingItem(PdfStructureElement element) :
            base(element, StructureTypeCategory.GroupingElement)
        { }

        public override void BeginItem()
        {
            // Nothing to do for grouping elements.
        }

        public override void EndItem()
        {
            // Nothing to do for grouping elements.
        }

        public override void OnDrawString()
        {
            // Issue warning because of missing block level item.
        }
    }

    /// <summary>
    /// Represents all block-level elements.
    /// </summary>
    class BlockLevelItem : StructureElementItem
    {
        public BlockLevelItem(PdfStructureElement element) :
            base(element, StructureTypeCategory.BlockLevelStructureElement)
        { }

        public override void BeginItem()
        {
            // Nothing to do for block level elements.
        }

        public override void EndItem()
        {
            // Nothing to do for block level elements.
        }

        public override void OnDrawString()
        {
            // Create marked content with structure type of element.
            StructureBuilder.BeginMarkedContentInternal(this);
        }

        public override void OnDraw()
        {
            // Create marked content with structure type of element.
            StructureBuilder.BeginMarkedContentInternal(this);
        }
    }

    /// <summary>
    /// Represents all inline-level elements.
    /// </summary>
    class InlineLevelItem : StructureElementItem
    {
        public InlineLevelItem(PdfStructureElement element) :
            base(element, StructureTypeCategory.InlineLevelStructureElement)
        { }

        public override void BeginItem()
        {
            // Nothing to do for inline level elements.
        }

        public override void EndItem()
        {
            // Nothing to do for inline level elements.
        }

        public override void OnDrawString()
        {
            // Create marked content with structure type of element.
            StructureBuilder.BeginMarkedContentInternal(this);
        }

        public override void OnDraw()
        {
            // Create marked content with structure type of element.
            StructureBuilder.BeginMarkedContentInternal(this);
        }
    }

    /// <summary>
    /// Represents all illustration elements.
    /// </summary>
    class IllustrationItem : StructureElementItem
    {
        public IllustrationItem(PdfStructureElement element) :
            base(element, StructureTypeCategory.IllustrationElement)
        { }

        /// <summary>
        /// The alternate text.
        /// </summary>
        public string? AltText { get; set; }

        /// <summary>
        /// The bounding box.
        /// </summary>
        public XRect BoundingRect { get; set; }

        public override void BeginItem()
        {
            // Graphic content is expected. Change to GraphicMode before entering Marked Content.
            StructureBuilder.UaManager.BeginGraphicMode();
        }

        public override void EndItem()
        {
            // Apply Properties.
            Element.Elements.SetString(PdfStructureElement.Keys.Alt, AltText ?? "");
            Element.LayoutAttributes.Elements.SetRectangle(PdfLayoutAttributes.Keys.BBox, new PdfRectangle(BoundingRect));
        }

        public override void OnDrawString()
        {
            // Create marked content with structure type of element
            StructureBuilder.BeginMarkedContentInternal(this);
        }

        public override void OnDraw()
        {
            // Create marked content with structure type of element
            StructureBuilder.BeginMarkedContentInternal(this);
        }
    }

    class StructureStack
    {
        public StructureStack(UAManager uaManager)
        {
            _uaManager = uaManager;
        }

        public PdfStructureElement MostInnerStructureElement
        {
            get
            {
                for (int idx = _list.Count - 1; idx >= 0; idx--)
                {
                    if (_list[idx] is StructureElementItem item)
                    {
                        Debug.Assert(item.Element != null);
                        return item.Element;
                    }
                }
                return _uaManager.StructureTreeElementDocument;
            }
        }

        public void Push(StructureItem item)
        {
            // Fit out every item with the structure builder.
            item.StructureBuilder = _uaManager.StructureBuilder;
            _list.Add(item);
        }

        public StructureItem Pop()
        {
            int idx = _list.Count - 1;
            if (idx < 0)
                throw new InvalidOperationException("Pop on empty stack.");

            var item = _list[idx];
            _list.RemoveAt(idx);
            return item;
        }

        public StructureItem? Current
        {
            get
            {
                int count = _list.Count;
                if (count == 0)
                    return null;
                return _list[count - 1];
            }
        }

        public IEnumerable<StructureItem> Items => _list.AsEnumerable();

        /// <summary>
        /// The UAManager of the document this stack belongs to.
        /// </summary>
        readonly UAManager _uaManager;

        /// <summary>
        /// The StructureItem stack.
        /// </summary>
        readonly List<StructureItem> _list = new List<StructureItem>();
    }
}
