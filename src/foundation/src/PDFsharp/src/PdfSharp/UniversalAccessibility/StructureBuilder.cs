// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Annotations;
using PdfSharp.Pdf.Structure;

namespace PdfSharp.UniversalAccessibility
{
    /// <summary>
    /// Helper class that adds structure to PDF documents.
    /// </summary>
    public class StructureBuilder
    {
        internal StructureBuilder(UAManager uaManager)
        {
            UaManager = uaManager;
            _elementStack = new StructureStack(uaManager);
        }

        /// <summary>
        /// Starts a grouping element.
        /// </summary>
        /// <param name="tag">The structure type to be created.</param>
        public void BeginElement(PdfGroupingElementTag tag)
        {
            BeginGroupingElement(TagToString(tag));
        }

        /// <summary>
        /// Starts a grouping element.
        /// </summary>
        public void BeginGroupingElement(string tag)
        {
            EndMarkedContentsWithId();
            var ste = CreateStructureElement(tag);
            var item = new GroupingItem(ste) { EndMark = true };
            _elementStack.Push(item);
            item.BeginItem();
        }

        /// <summary>
        /// Starts a block-level element.
        /// </summary>
        /// <param name="tag">The structure type to be created.</param>
        public void BeginElement(PdfBlockLevelElementTag tag)
        {
            BeginBlockLevelElement(TagToString(tag));
        }

        /// <summary>
        /// Starts a block-level element.
        /// </summary>
        public void BeginBlockLevelElement(string tag)
        {
            EndMarkedContentsWithId();
            var ste = CreateStructureElement(tag);
            var item = new BlockLevelItem(ste) { EndMark = true };
            _elementStack.Push(item);
            item.BeginItem();
        }

        /// <summary>
        /// Starts an inline-level element.
        /// </summary>
        /// <param name="tag">The structure type to be created.</param>
        public void BeginElement(PdfInlineLevelElementTag tag)
        {
            BeginInlineLevelElement(TagToString(tag));
        }

        /// <summary>
        /// Starts an inline-level element.
        /// </summary>
        public void BeginInlineLevelElement(string tag)
        {
            EndMarkedContentsWithId();
            var ste = CreateStructureElement(tag);
            var item = new InlineLevelItem(ste) { EndMark = true };
            _elementStack.Push(item);
            item.BeginItem();
        }

        /// <summary>
        /// Starts an illustration element.
        /// </summary>
        /// <param name="tag">The structure type to be created.</param>
        /// <param name="altText">The alternative text for this illustration.</param>
        /// <param name="boundingBox">The element’s bounding box.</param>
        public void BeginElement(PdfIllustrationElementTag tag, string altText, XRect boundingBox)
        {
            BeginIllustrationElement(TagToString(tag), altText, boundingBox);
        }

        /// <summary>
        /// Starts an illustration element.
        /// </summary>
        public void BeginIllustrationElement(string tag, string altText, XRect boundingRect)
        {
            EndMarkedContentsWithId();
            var ste = CreateStructureElement(tag);
            var item = new IllustrationItem(ste) { EndMark = true, AltText = altText, BoundingRect = boundingRect };
            _elementStack.Push(item);
            item.BeginItem();
        }

        /// <summary>
        /// Starts an artifact.
        /// </summary>
        public void BeginArtifact()
        {
            EndMarkedContentsWithId();

            var item = new ArtifactItem() { EndMark = true };
            _elementStack.Push(item);
            item.BeginItem();
        }

        /// <summary>
        /// Starts a link element.
        /// </summary>
        /// <param name="linkAnnotation">The PdfLinkAnnotation this link is using.</param>
        /// <param name="altText">The alternative text for this link.</param>
        public void BeginElement(PdfLinkAnnotation linkAnnotation, string altText)
        {
            EndMarkedContentsWithId();
            var ste = CreateStructureElement(TagToString(PdfInlineLevelElementTag.Link));
            var item = new InlineLevelItem(ste) { EndMark = true };
            _elementStack.Push(item);
            item.BeginItem();

            linkAnnotation.Elements.SetString(PdfAnnotation.Keys.Contents, altText);
            AddAnnotationToStructureElement(ste, linkAnnotation);
        }

        /// <summary>
        /// Ends the current element.
        /// </summary>
        public void End()
        {
            var item = _elementStack.Current;

            if (item == null)
                throw new InvalidOperationException("EndElement without previous user created StructureItem.");

            item.EndItem();
            _elementStack.Pop();

            // If item is not user created, call End() again to end its parent item.
            if (!item.EndMark)
                End();
        }

        /// <summary>
        /// Gets the current structure element.
        /// </summary>
        public PdfStructureElement CurrentStructureElement
            => _elementStack.MostInnerStructureElement;

        /// <summary>
        /// Sets the content of the "/Alt" (alternative text) key. Used e.g. for illustrations.
        /// </summary>
        /// <param name="altText">The alternative text.</param>
        public void SetAltText(string altText)
            => _elementStack.MostInnerStructureElement.Elements.SetString(PdfStructureElement.Keys.Alt, altText);

        /// <summary>
        /// Sets the content of the "/E" (expanded text) key. Used for abbreviations.
        /// </summary>
        /// <param name="expandedText">The expanded text representation of the abbreviation.</param>
        public void SetExpandedText(string expandedText)
            => _elementStack.MostInnerStructureElement.Elements.SetString(PdfStructureElement.Keys.E, expandedText);

        /// <summary>
        /// Sets the content of the "/Lang" (language) key.
        /// The chosen language is used for all children of the current structure element until a child has a new language defined.
        /// </summary>
        /// <param name="language">The language of the structure element and its children.</param>
        public void SetLanguage(string language)
            => _elementStack.MostInnerStructureElement.Elements.SetString(PdfStructureElement.Keys.Lang, language);

        /// <summary>
        /// Sets the row span of a table cell.
        /// </summary>
        /// <param name="rowSpan">The number of spanning cells.</param>
        public void SetRowSpan(int rowSpan)
            => _elementStack.MostInnerStructureElement.TableAttributes.Elements.SetInteger(PdfTableAttributes.Keys.RowSpan, rowSpan);

        /// <summary>
        /// Sets the colspan of a table cell.
        /// </summary>
        /// <param name="colSpan">The number of spanning cells.</param>
        public void SetColSpan(int colSpan)
            => _elementStack.MostInnerStructureElement.TableAttributes.Elements.SetInteger(PdfTableAttributes.Keys.ColSpan, colSpan);

        /// <summary>
        /// Starts the marked content. Used for every marked content with an MCID.
        /// </summary>
        /// <param name="steItem">The StructureElementItem to create a marked content for.</param>
        internal void BeginMarkedContentInternal(StructureElementItem steItem)
        {
            //Start in Text mode on new page.
            if (NextMcid == 0)
                UaManager.BeginTextMode();

            var tag = steItem.Element.Elements.GetName(PdfStructureElement.Keys.S);
            var item = new MarkedContentItemWithId(steItem, tag, NextMcid);
            _elementStack.Push(item);
            item.BeginItem();  // Render /Xxx <<MCID x>> BMC.
            AddMarkedContentToStructureElement(steItem.Element, item.Mcid);

            NextMcid++;
        }

        /// <summary>
        /// Ends all open marked contents that have a marked content with ID.
        /// </summary>
        public void EndMarkedContentsWithId()
        {
            var item = _elementStack.Current;

            if (item is MarkedContentItemWithId)
            {
                item.EndItem();
                _elementStack.Pop();

                // Call EndMarkedContentsWithId() for item’s parent.
                EndMarkedContentsWithId();
            }
            else
                Debug.Assert(_elementStack.Items.All(x => x is not MarkedContentItemWithId),
                    "There should not be any open marked content.");
        }

        /// <summary>
        /// The next marked content with ID to be assigned.
        /// </summary>
        // ReSharper disable once IdentifierTypo
        internal int NextMcid { get; set; }

        // ReSharper disable once IdentifierTypo

        /// <summary>
        /// Creates a new indirect structure element dictionary of the specified structure type.
        /// </summary>
        PdfStructureElement CreateStructureElement(string tag)
        {
            var parent = _elementStack.MostInnerStructureElement;

            // Create PdfStructureElement
            var ste = UaManager.Owner.Internals.CreateIndirectObject<PdfStructureElement>();

            // Set parent ...
            ste.Elements.SetReference(PdfStructureElement.Keys.P, parent);

            // ...and structure type.
            ste.Elements.SetName(PdfStructureElement.Keys.S, tag);

            // Create /K PdfArray for parent, if necessary.
            var parentKids = parent.Elements.GetArray(PdfStructureElement.Keys.K);
            if (parentKids == null)
            {
                parentKids = new PdfArray();
                parent.Elements.SetObject(PdfStructureElement.Keys.K, parentKids);
            }

            // Add PdfStructureElement to parent.
            parentKids.Elements.Add(ste);

            return ste;
        }

        /// <summary>
        /// Adds the marked content with the given MCID on the current page to the given structure element.
        /// </summary>
        /// <param name="ste">The structure element.</param>
        /// <param name="mcid">The MCID.</param>
        void AddMarkedContentToStructureElement(PdfStructureElement ste, int mcid)
        {
            var steKids = ste.Elements.GetArray(PdfStructureElement.Keys.K);

            // Create /K PdfArray for StructureElement, if necessary.
            if (steKids == null)
            {
                steKids = new PdfArray();
                ste.Elements.SetObject(PdfStructureElement.Keys.K, steKids);
            }

            // Set the Page of this StructureElement, if not yet set.
            var stePage = ste.Elements.GetReference(PdfStructureElement.Keys.Pg);
            if (stePage == null)
                ste.Elements.SetReference(PdfStructureElement.Keys.Pg, UaManager.CurrentPage);

            // Is the added Marked Content on the Page, the StructureElement began?
            if (stePage == null || stePage.Value == UaManager.CurrentPage)
            {
                // Yes. Add the MCID of the marked content.
                steKids.Elements.Add(new PdfInteger(mcid));
            }
            else
            {
                // No. Add a MarkedContentReference that contains the Page of this Marked Content and the MCID.
                var mcr = new PdfMarkedContentReference();
                mcr.Elements.SetReference(PdfMarkedContentReference.Keys.Pg, UaManager.CurrentPage);
                mcr.Elements.SetInteger(PdfMarkedContentReference.Keys.MCID, mcid);
                steKids.Elements.Add(mcr);
            }

            AddToParentTree(ste, mcid);
        }

        /// <summary>
        /// Creates a new parent element array for the current page and adds it to the ParentTree, if not yet existing.
        /// Adds the structure element to the index of mcid to the parent element array .
        /// Sets the page’s "/StructParents" key to the index of the parent element array in the ParentTree.
        /// </summary>
        /// <param name="ste">The structure element to be added to the parent tree.</param>
        /// <param name="mcid">The MCID of the current marked content (this is equal to the index of the entry in the parent tree node).</param>
        void AddToParentTree(PdfStructureElement ste, int mcid)
        {
            var structTreeRoot = UaManager.StructureTreeRoot;
            var parentTreeRoot = structTreeRoot.Elements.GetDictionary(PdfStructureTreeRoot.Keys.ParentTree) as PdfNumberTreeNode;
            Debug.Assert(parentTreeRoot != null);
            var parentTreeRootNums = parentTreeRoot.Elements.GetArray(PdfNumberTreeNode.Keys.Nums);

            // Create /Nums PdfArray for parentTreeRoot, if necessary.
            if (parentTreeRootNums == null)
            {
                parentTreeRootNums = new PdfArray();
                parentTreeRoot.Elements.SetObject(PdfNumberTreeNode.Keys.Nums, parentTreeRootNums);
            }

            var parentTreeNextKey = structTreeRoot.Elements.GetInteger(PdfStructureTreeRoot.Keys.ParentTreeNextKey);

            // Get /StructParents of Page.
            var structParentsItem = UaManager.CurrentPage.Elements.GetValue(PdfPage.Keys.StructParents);
            var hasStructParents = structParentsItem != null;

            // Create it, if necessary.
            if (!hasStructParents)
            {
                UaManager.CurrentPage.Elements.SetInteger(PdfPage.Keys.StructParents, parentTreeNextKey);
                structTreeRoot.Elements.SetInteger(PdfStructureTreeRoot.Keys.ParentTreeNextKey, parentTreeNextKey + 1);
            }

            var structParents = new PdfInteger(UaManager.CurrentPage.Elements.GetInteger(PdfPage.Keys.StructParents));

            // Get the PdfArray for this page in parentTreeRootNums.
            var isInParentTree = parentTreeRootNums.Elements.OfType<PdfInteger>().Any(x => x.Value == structParents.Value);
            Debug.Assert(hasStructParents == isInParentTree);

            // Create it, if necessary.
            if (!isInParentTree)
            {
                var count = parentTreeRootNums.Elements.Count;
                var lastKey = count > 0 ? parentTreeRootNums.Elements[count - 2] as PdfInteger : new PdfInteger(-1);

                Debug.Assert(lastKey != null && lastKey.Value + 1 == structParents.Value, "The values should be continuous.");

                parentTreeRoot.AddNumber(structParents.Value, new PdfArray());
            }

            var structParentsMatches = parentTreeRootNums.Elements.OfType<PdfInteger>().Where(x => x.Value == structParents.Value).ToList();
            Debug.Assert(structParentsMatches.Count == 1);

            var iKey = parentTreeRootNums.Elements.IndexOf(structParentsMatches.First());
            var iValue = iKey + 1;
            Debug.Assert(parentTreeRootNums.Elements.Count > iValue);

            var parentElementArray = parentTreeRootNums.Elements[iValue] as PdfArray;
            Debug.Assert(parentElementArray != null);

            // Add ste to parentElementArray.
            Debug.Assert(mcid == parentElementArray.Elements.Count, "ParentElementArray index should correspond to the MCID.");
            parentElementArray.Elements.Add(ste);
        }

        /// <summary>
        /// Adds the structure element to the ParentTree.
        /// Sets the annotation’s "/StructParent" key to the index of the structure element in the ParentTree.
        /// </summary>
        /// <param name="ste">The structure element to be added to the parent tree.</param>
        /// <param name="annotation">The annotation to be added.</param>
        void AddToParentTree(PdfStructureElement ste, PdfAnnotation annotation)
        {
            var structTreeRoot = UaManager.StructureTreeRoot;
            var parentTreeRoot = structTreeRoot.Elements.GetDictionary(PdfStructureTreeRoot.Keys.ParentTree) as PdfNumberTreeNode;
            Debug.Assert(parentTreeRoot != null);
            var parentTreeRootNums = parentTreeRoot.Elements.GetArray(PdfNumberTreeNode.Keys.Nums);

            // Create /Nums PdfArray for parentTreeRoot, if necessary.
            if (parentTreeRootNums == null)
            {
                parentTreeRootNums = new PdfArray();
                parentTreeRoot.Elements.SetObject(PdfNumberTreeNode.Keys.Nums, parentTreeRootNums);
            }

            var parentTreeNextKey = structTreeRoot.Elements.GetInteger(PdfStructureTreeRoot.Keys.ParentTreeNextKey);

            // Get /StructParents of Annotation.
            var structParentsItem = annotation.Elements.GetValue(PdfAnnotation.Keys.StructParent);
            var hasStructParents = structParentsItem != null;

            // Create it, if necessary.
            if (!hasStructParents)
            {
                annotation.Elements.SetInteger(PdfAnnotation.Keys.StructParent, parentTreeNextKey);
                structTreeRoot.Elements.SetInteger(PdfStructureTreeRoot.Keys.ParentTreeNextKey, parentTreeNextKey + 1);
            }

            var structParents = new PdfInteger(annotation.Elements.GetInteger(PdfAnnotation.Keys.StructParent));

            // Get the PdfArray for this page in parentTreeRootNums.
            var isInParentTree = parentTreeRootNums.Elements.OfType<PdfInteger>().Any(x => x.Value == structParents.Value);
            Debug.Assert(hasStructParents == isInParentTree);

            // Create it, if necessary.
            if (!isInParentTree)
            {
                var count = parentTreeRootNums.Elements.Count;
                var lastKey = count > 0 ? parentTreeRootNums.Elements[count - 2] as PdfInteger : new PdfInteger(-1);

                Debug.Assert(lastKey != null && lastKey.Value + 1 == structParents.Value, "The values should be continous.");

                parentTreeRoot.AddNumber(structParents.Value, ste);
            }
            else
            {
                Debug.Assert(false, "StructureElement should not already be referenced in ParentTree.");
            }
        }

        /// <summary>
        /// Adds a PdfObjectReference referencing annotation and the current page to the given structure element.
        /// </summary>
        /// <param name="ste">The structure element.</param>
        /// <param name="annotation">The annotation.</param>
        void AddAnnotationToStructureElement(PdfStructureElement ste, PdfAnnotation annotation)
        {
            var page = UaManager.CurrentPage;
            page.Annotations.Add(annotation);

            // Tab order must be set to "/S" (Structure) on pages with Annotations.
            if (page.Elements.GetName(PdfPage.Keys.Tabs) != "/S")
                page.Elements.SetName(PdfPage.Keys.Tabs, "/S");

            var steK = ste.Elements.GetArray(PdfStructureElement.Keys.K);

            // Create /K PdfArray for StructureElement, if necessary.
            if (steK == null)
            {
                steK = new PdfArray();
                ste.Elements.SetObject(PdfStructureElement.Keys.K, steK);
            }

            var objr = new PdfObjectReference();
            objr.Elements.SetReference(PdfObjectReference.Keys.Obj, annotation);
            objr.Elements.SetReference(PdfObjectReference.Keys.Pg, page);

            steK.Elements.Add(objr);

            AddToParentTree(ste, annotation);
        }

        /// <summary>
        /// Called when AddPage was issued.
        /// </summary>
        internal void OnAddPage()
        {
            var current = _elementStack.Current;

            if (current is MarkedContentItemWithId)
                EndMarkedContentsWithId();

            NextMcid = 0;
        }

        /// <summary>
        /// Called when DrawString was issued.
        /// </summary>
        internal void OnDrawString()
        {
            // Dispatch DrawString event to top level stack item.
            var current = _elementStack.Current;
            if (current == null)
                throw new InvalidOperationException("DrawString issued outside of document structure in PDF/UA document.");

            current.OnDrawString();
        }

        /// <summary>
        /// Called when e.g. DrawEllipse was issued.
        /// </summary>
        internal void OnDraw()
        {
            // Dispatch Draw event to top level stack item.
            var current = _elementStack.Current;
            if (current == null)
                throw new InvalidOperationException("Draw method issued outside of document structure in PDF/UA document.");

            current.OnDraw();
        }

        internal ContentWriter Content => new(UaManager.CurrentGraphics);

#pragma warning disable CS0618 // Type or member is obsolete

        string TagToString(PdfGroupingElementTag tag)
        {
            return tag switch
            {
                PdfGroupingElementTag.Document => nameof(PdfGroupingElementTag.Document),
                PdfGroupingElementTag.Part => nameof(PdfGroupingElementTag.Part),
                PdfGroupingElementTag.Art or PdfGroupingElementTag.Article => nameof(PdfGroupingElementTag.Art),
                PdfGroupingElementTag.Sect or PdfGroupingElementTag.Section => nameof(PdfGroupingElementTag.Sect),
                PdfGroupingElementTag.Div or PdfGroupingElementTag.Division => nameof(PdfGroupingElementTag.Div),
                PdfGroupingElementTag.BlockQuote or PdfGroupingElementTag.BlockQuotation => nameof(PdfGroupingElementTag.BlockQuote),
                PdfGroupingElementTag.Caption => nameof(PdfGroupingElementTag.Caption),
                PdfGroupingElementTag.TOC or PdfGroupingElementTag.TableOfContents => nameof(PdfGroupingElementTag.TOC),
                PdfGroupingElementTag.TOCI or PdfGroupingElementTag.TableOfContentsItem => nameof(PdfGroupingElementTag.TOCI),
                PdfGroupingElementTag.Index => nameof(PdfGroupingElementTag.Index),
                PdfGroupingElementTag.NonStruct or PdfGroupingElementTag.NonstructuralElement => nameof(PdfGroupingElementTag.NonStruct),
                PdfGroupingElementTag.Private or PdfGroupingElementTag.PrivateElement => nameof(PdfGroupingElementTag.Private),
                _ => throw new InvalidOperationException($"Invalid tag value '{(int)tag}'")
            };
        }

        string TagToString(PdfBlockLevelElementTag tag)
        {
            return tag switch
            {
                PdfBlockLevelElementTag.P or PdfBlockLevelElementTag.Paragraph => nameof(PdfBlockLevelElementTag.P),
                PdfBlockLevelElementTag.H or PdfBlockLevelElementTag.Heading => nameof(PdfBlockLevelElementTag.H),
                PdfBlockLevelElementTag.H1 or PdfBlockLevelElementTag.Heading1 => nameof(PdfBlockLevelElementTag.H1),
                PdfBlockLevelElementTag.H2 or PdfBlockLevelElementTag.Heading2 => nameof(PdfBlockLevelElementTag.H2),
                PdfBlockLevelElementTag.H3 or PdfBlockLevelElementTag.Heading3 => nameof(PdfBlockLevelElementTag.H3),
                PdfBlockLevelElementTag.H4 or PdfBlockLevelElementTag.Heading4 => nameof(PdfBlockLevelElementTag.H4),
                PdfBlockLevelElementTag.H5 or PdfBlockLevelElementTag.Heading5 => nameof(PdfBlockLevelElementTag.H5),
                PdfBlockLevelElementTag.H6 or PdfBlockLevelElementTag.Heading6 => nameof(PdfBlockLevelElementTag.H6),
                PdfBlockLevelElementTag.L or PdfBlockLevelElementTag.List => nameof(PdfBlockLevelElementTag.L),
                PdfBlockLevelElementTag.Lbl or PdfBlockLevelElementTag.Label => nameof(PdfBlockLevelElementTag.Lbl),
                PdfBlockLevelElementTag.LI or PdfBlockLevelElementTag.ListItem => nameof(PdfBlockLevelElementTag.LI),
                PdfBlockLevelElementTag.LBody or PdfBlockLevelElementTag.ListBody => nameof(PdfBlockLevelElementTag.LBody),
                PdfBlockLevelElementTag.Table => nameof(PdfBlockLevelElementTag.Table),
                PdfBlockLevelElementTag.TR or PdfBlockLevelElementTag.TableRow => nameof(PdfBlockLevelElementTag.TR),
                PdfBlockLevelElementTag.TH or PdfBlockLevelElementTag.TableHeaderCell => nameof(PdfBlockLevelElementTag.TH),
                PdfBlockLevelElementTag.TD or PdfBlockLevelElementTag.TableDataCell => nameof(PdfBlockLevelElementTag.TD),
                PdfBlockLevelElementTag.THead or PdfBlockLevelElementTag.TableHeadRowGroup => nameof(PdfBlockLevelElementTag.THead),
                PdfBlockLevelElementTag.TBody or PdfBlockLevelElementTag.TableBodyRowGroup => nameof(PdfBlockLevelElementTag.TBody),
                PdfBlockLevelElementTag.TFoot or PdfBlockLevelElementTag.TableFooterRowGroup => nameof(PdfBlockLevelElementTag.TFoot),
                _ => throw new InvalidOperationException($"Invalid tag value '{(int)tag}'")
            };
        }

        string TagToString(PdfInlineLevelElementTag tag)
        {
            return tag switch
            {
                PdfInlineLevelElementTag.Span => nameof(PdfInlineLevelElementTag.Span),
                PdfInlineLevelElementTag.Quote or PdfInlineLevelElementTag.Quotation => nameof(PdfInlineLevelElementTag.Quote),
                PdfInlineLevelElementTag.Note => nameof(PdfInlineLevelElementTag.Note),
                PdfInlineLevelElementTag.Reference => nameof(PdfInlineLevelElementTag.Reference),
                PdfInlineLevelElementTag.BibEntry or PdfInlineLevelElementTag.BibliographyEntry => nameof(PdfInlineLevelElementTag.BibEntry),
                PdfInlineLevelElementTag.Code => nameof(PdfInlineLevelElementTag.Code),
                PdfInlineLevelElementTag.Link => nameof(PdfInlineLevelElementTag.Link),
                PdfInlineLevelElementTag.Annot or PdfInlineLevelElementTag.Annotation => nameof(PdfInlineLevelElementTag.Annot),
                PdfInlineLevelElementTag.Ruby => nameof(PdfInlineLevelElementTag.Ruby),
                PdfInlineLevelElementTag.Warichu => nameof(PdfInlineLevelElementTag.Warichu),
                _ => throw new InvalidOperationException($"Invalid tag value '{(int)tag}'")
            };
        }

        string TagToString(PdfIllustrationElementTag tag)
        {
            return tag switch
            {
                PdfIllustrationElementTag.Figure => nameof(PdfIllustrationElementTag.Figure),
                PdfIllustrationElementTag.Formula => nameof(PdfIllustrationElementTag.Formula),
                PdfIllustrationElementTag.Form => nameof(PdfIllustrationElementTag.Form),
                _ => throw new InvalidOperationException($"Invalid tag value '{(int)tag}'")
            };
        }

#pragma warning restore CS0618 // Type or member is obsolete

        /// <summary>
        /// Used to write text directly to the content stream.
        /// </summary>
        internal class ContentWriter
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            public ContentWriter(XGraphics gfx)
            {
                _gfx = gfx;
            }

            /// <summary>
            /// Writes text to the content stream.
            /// </summary>
            /// <param name="content">The text to write to the content stream.</param>
            public void Write(string content)
            {
                _gfx.AppendToContentStream(content);
            }

            readonly XGraphics _gfx;
        }

        internal readonly UAManager UaManager;
        readonly StructureStack _elementStack;
    }
}
