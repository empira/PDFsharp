// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Internal;
using PdfSharp.Drawing;
using PdfSharp.Pdf.IO;
#if GDI
using System.Drawing;
using System.Drawing.Imaging;
#endif
#if WPF
using System.Windows.Media;
#endif

// v7.0.0 REVIEW

#pragma warning disable CS1591 // TODO_DOC: Missing XML comment for publicly visible type or member

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Represents an external form object (e.g. an imported page).
    /// </summary>
    public sealed class PdfFormXObject : PdfXObject, IContentStream
    {
        internal PdfFormXObject(PdfDocument thisDocument)
            : base(thisDocument)
        {
            SetTypeKeys();
            _form = null;
        }

        internal PdfFormXObject(PdfDocument thisDocument, XForm form)
            : base(thisDocument)
        {
            // BUG_OLD: form is not used - not implemented.
            SetTypeKeys();
            _form = form;

            //if (form.IsTemplate)
            //{ }
        }

        /// <summary>
        /// Initializes a new instance of this class using the elements of the specified dictionary.
        /// After this type transformation the specified dictionary is dead and cannot be used anymore.
        /// </summary>
        internal PdfFormXObject(PdfDictionary dict)
            : base(dict)
        {
            _form = null;
        }

        internal double DpiX { get; set; } = 72;

        internal double DpiY { get; set; } = 72;

        internal PdfFormXObject(PdfDocument thisDocument, PdfImportedObjectTable importedObjectTable, XPdfForm form)
            : base(thisDocument)
        {
            Debug.Assert(importedObjectTable != null);
            Debug.Assert(ReferenceEquals(thisDocument, importedObjectTable.Owner));
            SetTypeKeys();

            if (form.IsTemplate)
            {
                Debug.Assert(importedObjectTable == null);
                // TODO_OLD more initialization here???
                return;
            }

            XPdfForm pdfForm = form;
            // Get import page
            var importPages = importedObjectTable.ExternalDocument!.Pages; // NRT
            if (pdfForm.PageNumber < 1 || pdfForm.PageNumber > importPages.Count)
                PsMsgs.ImportPageNumberOutOfRange(pdfForm.PageNumber, importPages.Count, form._path);
            PdfPage importPage = importPages[pdfForm.PageNumber - 1];

            // Import resources
            //var res = importPage.Elements["/Resources"];
            var res = importPage.Elements.GetValue("/Resources"); // #US373
            if (res != null) // Unlikely but possible.
            {
#if true
                // Get root object.
                // #US373 begin
                PdfObject root;
                //if (res is PdfReference reference)
                //    root = reference.Value;
                //else
                    root = (PdfDictionary)res;
                // #US373 end

                root = ImportClosure(importedObjectTable, thisDocument, root);
                // If the root was a direct object, make it indirect.
                if (root.Reference == null)
                    thisDocument.IrefTable.Add(root);

                Debug.Assert(root.Reference != null);
                Elements["/Resources"] = root.Reference;
#else
                // Get transitive closure
                PdfObject[] resources = importPage.Owner.Internals.GetClosure(resourcesRoot);
                int count = resources.Length;
#if DEBUG_
                for (int idx = 0; idx < count; idx++)
                {
                    Debug.Assert(resources[idx].XRef != null);
                    Debug.Assert(resources[idx].XRef.Document != null);
                    Debug.Assert(resources[idx].Document != null);
                    if (resources[idx].ObjectID.ObjectNumber == 12)
                        _ = typeof(int);
                }
#endif
                // 1st step. Already imported objects are reused and new ones are cloned.
                for (int idx = 0; idx < count; idx++)
                {
                    PdfObject obj = resources[idx];
                    if (importedObjectTable.Contains(obj.ObjectID))
                    {
                        // external object was already imported
                        PdfReference iref = importedObjectTable[obj.ObjectID];
                        Debug.Assert(iref != null);
                        Debug.Assert(iref.Value != null);
                        Debug.Assert(iref.Document == Owner);
                        // replace external object by the already clone counterpart
                        resources[idx] = iref.Value;
                    }
                    else
                    {
                        // External object was not imported earlier and must be cloned
                        PdfObject clone = obj.Clone();
                        Debug.Assert(clone.Reference == null);
                        clone.Document = Owner;
                        if (obj.Reference != null)
                        {
                            // add it to this (the importer) document
                            Owner.irefTable.Add(clone);
                            Debug.Assert(clone.Reference != null);
                            // save old object identifier
                            importedObjectTable.Add(obj.ObjectID, clone.Reference);
                            //Debug.WriteLine("Cloned: " + obj.ObjectID.ToString());
                        }
                        else
                        {
                            // The root object (the /Resources value) is not an indirect object
                            Debug.Assert(idx == 0);
                            // add it to this (the importer) document
                            Owner.irefTable.Add(clone);
                            Debug.Assert(clone.Reference != null);
                        }
                        // replace external object by its clone
                        resources[idx] = clone;
                    }
                }
#if DEBUG_
        for (int idx = 0; idx < count; idx++)
        {
          Debug.Assert(resources[idx].XRef != null);
          Debug.Assert(resources[idx].XRef.Document != null);
          Debug.Assert(resources[idx].Document != null);
          if (resources[idx].ObjectID.ObjectNumber == 12)
            _ = typeof(int);
        }
#endif

                // 2nd step. Fix up indirect references that still refers to the import document.
                for (int idx = 0; idx < count; idx++)
                {
                    PdfObject obj = resources[idx];
                    Debug.Assert(obj.Owner != null);
                    FixUpObject(importedObjectTable, importedObjectTable.Owner, obj);
                }

                // Set resources key to the root of the clones
                Elements["/Resources"] = resources[0].Reference;
#endif
            }

            // Take /Rotate into account.
            PdfRectangle rect = importPage.Elements.GetRequiredRectangle(PdfPage.InheritablePageKeys.MediaBox);
            // Reduce rotation to 0, 90, 180, or 270.
            int rotate = (importPage.Elements.GetInteger(PdfPage.InheritablePageKeys.Rotate) % 360 + 360) % 360;
            //rotate = 0;
            if (rotate == 0)
            {
                // Set bounding box to media box.
                Elements["/BBox"] = rect;
            }
            else
            {
                // TODO_OLD: Have to adjust bounding box? (I think not, but I’m not sure -> wait for problem)
                Elements["/BBox"] = rect;

                // Rotate the image such that it is upright.
                XMatrix matrix = new XMatrix();
                double width = rect.Width;
                double height = rect.Height;
                matrix.RotateAtPrepend(-rotate, new XPoint(width / 2, height / 2));

                // Translate the image such that its center lies in the center of the rotated bounding box.
                double offset = (height - width) / 2;
                if (rotate == 90)
                {
                    // TODO_OLD It seems we can simplify this as the sign of offset changes too.
                    if (height > width)
                        matrix.TranslatePrepend(offset, offset); // Tested.
                    else
                        matrix.TranslatePrepend(offset, offset); // TODO_OLD Test case.
                }
                else if (rotate == 270)
                {
                    // TODO_OLD It seems we can simplify this as the sign of offset changes too.
                    if (height > width)
                        matrix.TranslatePrepend(-offset, -offset); // Tested.
                    else
                        matrix.TranslatePrepend(-offset, -offset); // Tested.
                }

                //string item = "[" + PdfEncoders.ToString(matrix) + "]";
                //Elements[Keys.Matrix] = new PdfLiteral(item);
                Elements.SetMatrix(Keys.Matrix, matrix);
            }

            // Preserve filter because the content keeps unmodified.
            PdfContent content = importPage.Contents.CreateSingleContent();
#if !DEBUG
            content.Compressed = true;
#endif
            //var filter = content.Elements["/Filter"];
            var filter = content.Elements.GetValue(PdfStream.Keys.Filter); // #US373
            if (filter != null)
                Elements[PdfStream.Keys.Filter] = filter.Clone(); // #US373: Should we expect references here?

            // (no cloning needed because the bytes keep untouched)
            Stream = content.Stream; // new PdfStream(bytes, this);
            Elements.SetInteger("/Length", content.Stream!.Value.Length);
        }

        public void SetTypeKeys()
        {
            Elements.SetName(Keys.Type, "/XObject");
            Elements.SetName(Keys.Subtype, "/Form");
        }

        internal void SetForm(XForm form)
        {
            _form = form;
        }

        /// <summary>
        /// Gets the PdfResources object of this form.
        /// </summary>
        public PdfResources Resources
        {
            get
            {
                if (_resources == null)
                    _resources = (PdfResources?)Elements.GetValue(Keys.Resources, VCF.Create) ?? NRT.ThrowOnNull<PdfResources>();
                return _resources;
            }
        }
        PdfResources? _resources;

        PdfResources IContentStream.Resources => Resources;

        internal string GetFontName(XGlyphTypeface glyphTypeface, FontType fontType, out PdfFont pdfFont)
        {
            Debug.Assert(ReferenceEquals(_document2, Document));
            pdfFont = Document.FontTable.GetOrCreateFont(glyphTypeface, fontType);
            Debug.Assert(pdfFont != null);
            string name = Resources.AddFont(pdfFont);
            return name;
        }

        string IContentStream.GetFontName(XGlyphTypeface glyphTypeface, FontType fontType, out PdfFont pdfFont)
            => GetFontName(glyphTypeface, fontType, out pdfFont);

        /// <summary>
        /// Gets the resource name of the specified font data within this form XObject.
        /// </summary>
        internal string GetFontName(string idName, byte[] fontData, out PdfFont pdfFont)
        {
            Debug.Assert(ReferenceEquals(_document2, Document));
            pdfFont = Document.FontTable.GetFont(idName, fontData);
            Debug.Assert(pdfFont != null);
            string name = Resources.AddFont(pdfFont);
            return name;
        }

        string IContentStream.GetFontName(string idName, byte[] fontData, out PdfFont pdfFont)
        {
            return GetFontName(idName, fontData, out pdfFont);
        }

        string IContentStream.GetImageName(XImage image)
        {
            throw new NotImplementedException();
        }

        string IContentStream.GetFormName(XForm form)
        {
            throw new NotImplementedException();
        }

#if keep_code_some_time_as_reference
        /// <summary>
        /// Replace all indirect references to external objects by their cloned counterparts
        /// owned by the importer document.
        /// </summary>
        void FixUpObject_old(PdfImportedObjectTable iot, PdfObject value)
        {
            // TODO_OLD: merge with PdfXObject.FixUpObject
            PdfDictionary dict;
            PdfArray array;
            if ((dict = value as PdfDictionary) != null)
            {
                // Set document for cloned direct objects
                if (dict.Owner == null)
                    dict.Document = Owner;
                else
                    Debug.Assert(dict.Owner == Owner);

                // Search for indirect references in all keys
                PdfName[] names = dict.Elements.KeyNames;
                foreach (PdfName name in names)
                {
                    PdfItem item = dict.Elements[name];
                    // Is item an iref?
                    PdfReference iref = item as PdfReference;
                    if (iref != null)
                    {
                        // Does the iref already belong to this document?
                        if (iref.Document == Owner)
                        {
                            // Yes: fine
                            continue;
                        }
                        else
                        {
                            Debug.Assert(iref.Document == iot.ExternalDocument);
                            // No: replace with iref of cloned object
                            PdfReference newXRef = iot[iref.ObjectID];
                            Debug.Assert(newXRef != null);
                            Debug.Assert(newXRef.Document == Owner);
                            dict.Elements[name] = newXRef;
                        }
                    }
                    else if (item is PdfObject)
                    {
                        // Fix up inner objects
                        FixUpObject_old(iot, (PdfObject)item);
                    }
                }
            }
            else if ((array = value as PdfArray) != null)
            {
                // Set document for cloned direct objects
                if (array.Owner == null)
                    array.Document = Owner;
                else
                    Debug.Assert(array.Owner == Owner);

                // Search for indirect references in all array elements
                int count = array.Elements.Count;
                for (int idx = 0; idx < count; idx++)
                {
                    PdfItem item = array.Elements[idx];
                    // Is item an iref?
                    PdfReference iref = item as PdfReference;
                    if (iref != null)
                    {
                        // Does the iref belongs to this document?
                        if (iref.Document == Owner)
                        {
                            // Yes: fine
                            continue;
                        }
                        else
                        {
                            Debug.Assert(iref.Document == iot.ExternalDocument);
                            // No: replace with iref of cloned object
                            PdfReference newXRef = iot[iref.ObjectID];
                            Debug.Assert(newXRef != null);
                            Debug.Assert(newXRef.Document == Owner);
                            array.Elements[idx] = newXRef;
                        }
                    }
                    else if (item is PdfObject)
                    {
                        // Fix up inner objects
                        FixUpObject_old(iot, (PdfObject)item);
                    }
                }
            }
        }
#endif
        public static bool IsFormXObject(PdfDictionary dict)
        {
            // Subtype is required.
            if (dict.Elements.GetName(Keys.Subtype) == "/Form")
                return true;

            // Type is optional.
            if (dict.Elements.GetName(Keys.Type) == "/XObject")
                return true;

            return false;
        }

        internal override void WriteObject(PdfWriter writer)
        {
            _form?.DrawingFinished();
            base.WriteObject(writer);
        }

        XForm? _form;

        /// <summary>
        /// Predefined keys of this dictionary.
        /// </summary>
        public new sealed class Keys : PdfXObject.Keys
        {
            /// <summary>
            /// (Optional) The type of PDF object that this dictionary describes; if present,
            /// must be XObject for a form XObject.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Optional)]
            public const string Type = "/Type";

            /// <summary>
            /// (Required) The type of XObject that this dictionary describes; must be Form
            /// for a form XObject.
            /// </summary>
            [KeyInfo(KeyType.Name | KeyType.Required)]
            public const string Subtype = "/Subtype";

            /// <summary>
            /// (Optional) A code identifying the type of form XObject that this dictionary
            /// describes. The only valid value defined at the time of publication is 1.
            /// Default value: 1.
            /// </summary>
            [KeyInfo(KeyType.Integer | KeyType.Optional)]
            public const string FormType = "/FormType";

            /// <summary>
            /// (Required) An array of four numbers in the form coordinate system, giving the 
            /// coordinates of the left, bottom, right, and top edges, respectively, of the 
            /// form XObject’s bounding box. These boundaries are used to clip the form XObject
            /// and to determine its size for caching.
            /// </summary>
            [KeyInfo(KeyType.Rectangle | KeyType.Required)]
            public const string BBox = "/BBox";

            /// <summary>
            /// (Optional) An array of six numbers specifying the form matrix, which maps
            /// form space into user space.
            /// Default value: the identity matrix [1 0 0 1 0 0].
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Optional)]
            public const string Matrix = "/Matrix";

            /// <summary>
            /// (Optional but strongly recommended; PDF 1.2) A dictionary specifying any
            /// resources (such as fonts and images) required by the form XObject.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional, typeof(PdfResources))]
            public const string Resources = "/Resources";

            /// <summary>
            /// (Optional; PDF 1.4) A group attributes dictionary indicating that the contents
            /// of the form XObject are to be treated as a group and specifying the attributes
            /// of that group (see Section 4.9.2, “Group XObjects”).
            /// Note: If a Ref entry (see below) is present, the group attributes also apply to the
            /// external page imported by that entry, which allows such an imported page to be
            /// treated as a group without further modification.
            /// </summary>
            [KeyInfo(KeyType.Dictionary | KeyType.Optional)]
            public const string Group = "/Group";

            // further keys:
            //Ref
            //Metadata
            //PieceInfo
            //LastModified
            //StructParent
            //StructParents
            //OPI
            //OC
            //Name

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
    }
}
