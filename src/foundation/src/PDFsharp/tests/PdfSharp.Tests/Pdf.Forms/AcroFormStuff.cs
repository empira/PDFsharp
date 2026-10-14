// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#if NET8_0_OR_GREATER
using System.Diagnostics;
using System.Text;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Annotations;
using PdfSharp.Pdf.Forms;

// TODO: DELETE before first release

namespace PdfSharp.Tests.Pdf.Forms // TODO: FormsCleanUp: Folder Pdf.Forms can be removed as soon as empty.
{
    public static class C1 // TODO: FormsCleanUp: Delete - current code in AnnotationPreparer and AcroFieldPreparer
    {
        public static void CreateAcroFormObjects(PdfDocument doc)
        {
            var catalog = doc.Catalog;
            var acroForm = catalog.GetAcroForm();

            var fields = acroForm?.Elements.GetArray(PdfForm.Keys.Fields);
            if (fields != null)
            {
                var fieldCount = fields.Elements.Count;

                for (int idx = 0; idx < fieldCount; idx++)
                {
                    var field = fields.Elements.GetRequiredDictionary(idx);
                    var ft = field.Elements[PdfFormField.Keys.FT];
                    var subFieldCount = field.Elements.GetArray(PdfForm.Keys.Fields)?.Count() ?? 0;

                    if (field is not PdfFormField)
                    {
                        CreateDerivedField(fields, idx);
                        Debug.Assert(field.IsDead);
                    }

                    var acroField = fields.Elements.GetRequiredDictionary<PdfFormField>(idx);
                    //var acroField = fields.Elements.GetRequiredDictionary(idx);
                    HandleAP(acroField);
                    HandleKids(acroField);
                }
            }
        }

        public static void CreateAnnotationObjects(PdfDocument doc)
        {
            var pages = doc.Catalog.Pages;
            foreach (var page in pages)
            {
                var annots = page.Elements.GetArray(PdfPage.Keys.Annots);
                if (annots != null)
                {
                    var count = annots.Elements.Count;
                    for (int idx = 0; idx < count; idx++)
                    {
                        var annot = annots.Elements.GetDictionary(idx);
                        if (annot is PdfWidgetAnnotation widget)
                        {
                            //  throw new InvalidOperationException("WTF happened???");
                        }
                        else if (annot is PdfFormField field)
                        {
                            var proxyWidget = field.GetAsWidgetAnnotation();
                            if (proxyWidget != null)
                            {
                                annots.Elements[idx] = proxyWidget;
                            }
                            else
                                throw new InvalidOperationException("Should not happen.");
                        }
                        else if (annot != null)
                        {
                            var type = annot.GetType();
                            if (type != typeof(PdfDictionary))
                                throw new InvalidOperationException("Not a dictionary???");
                        }
                        else
                        {
                            throw new InvalidOperationException("Not an annotation???");
                        }
                    }
                }
            }
        }

        static void CreateDerivedField(PdfArray fields, int index)
        {
            var field = fields.Elements.GetRequiredDictionary(index);
            // Is field already transformed?
            if (field is PdfFormField)
                return;

            //var ft = field.Elements.GetName(PdfFormField.Keys.FT);
            var (type, isWidget) = PdfFormField.GetAcroFieldType(field);
            var newField = fields.Elements.GetRequiredDictionary<PdfFormField>(index, VCF.None);

            if (isWidget)
                newField.GetAsWidgetAnnotation();

            Debug.Assert(field.IsDead);
        }

        // ReSharper disable once InconsistentNaming
        static void HandleAP(PdfFormField field)
        {
            var ap = field.Elements.GetDictionary<PdfAnnotationAppearance>(PdfAnnotation.Keys.AP);
            if (ap != null)
            {
                var valN = ap.Elements.GetDictionary(PdfAnnotationAppearance.Keys.N);
                HandleAPEntry(valN);

                var valR = ap.Elements.GetDictionary(PdfAnnotationAppearance.Keys.R);
                HandleAPEntry(valR);

                var valD = ap.Elements.GetDictionary(PdfAnnotationAppearance.Keys.D);
                HandleAPEntry(valD);
            }

            // ReSharper disable once InconsistentNaming
            void HandleAPEntry(PdfDictionary? streamOrDict)
            {
                if (streamOrDict == null)
                    return;

                var stream = streamOrDict.Stream;
                if (stream is null)
                {
                    var keys = streamOrDict.Elements.Keys;
                    foreach (var key in keys)
                    {
                        var xo = streamOrDict.Elements.GetDictionary(key);
                        if (xo is PdfFormXObject)
                            continue;

                        if (xo != null)
                        {
                            Debug.Assert(xo.IsIndirect);
#if true
                            var xo2 = streamOrDict.Elements.GetDictionary(key, VCF.None, typeof(PdfFormXObject));
#else
                        TransformToXObject(xo);
#endif
                            Debug.Assert(xo.IsDead);
                        }
                        else
                        {
                            Debug.Assert(false);
                        }
                    }
                }
                else
                {
                    Debug.Assert(streamOrDict.IsIndirect);
                    TransformToXObject(streamOrDict);
                }

                void TransformToXObject(PdfDictionary dict)
                {
                    if (dict is PdfFormXObject alreadyFormXObject)
                    {
                        _ = typeof(int);
                    }
                    else
                    {
                        dict.Elements.CreateContainer(typeof(PdfFormXObject), dict, true);
                        Debug.Assert(dict.IsDead);
                    }
                }
            }
        }

        static void HandleKids(PdfFormField field)
        {
            var kids = field.Elements.GetArray(PdfFormField.Keys.Kids);
            var hasKids = field.HasKids;
            Debug.Assert(kids == null || (hasKids && kids.Elements.Count > 0) || (!hasKids && kids.Elements.Count == 0));
            if (kids != null)
            {
                // Transform kids to Acro fields.
                var count = kids.Elements.Count;
                for (int idx = 0; idx < count; idx++)
                {
                    CreateDerivedField(kids, idx);
                }

                for (int idx = 0; idx < count; idx++)
                {
                    var acroField2 = kids.Elements.GetDictionary(idx);
                    if (acroField2 is not PdfFormField)
                        _ = typeof(int);

                    var acroField = kids.Elements.GetRequiredDictionary<PdfFormField>(idx);
                    HandleAP(acroField);
                    HandleKids(acroField);
                }
            }
        }
    }

    public static class C2
    {
        public static string CreateFieldListByFields(PdfDocument doc) // TODO: FormsCleanUp: Delete - FormsManager.FlattenForm() and ExportToJson() enumerate the pages and get the fields via its widgets.
        {
            var pageCount = doc.PageCount;
            var sb = new StringBuilder();
            sb.Append("Fields by AcroForm\n");
            int indent = 1;
            var acroForm = doc.Catalog.GetAcroForm();
            if (acroForm != null)
            {
                foreach (var field in acroForm.Fields)
                {
                    HandleField(field);
                }
            }
            return sb.ToString();

            void HandleField(PdfFormField field)
            {
                bool containsWidgetPart = field.ContainsWidgetPart;
                sb.Append(Invariant($"{Indent()}Type: {field.GetType().Name}, {(containsWidgetPart ? "Widget, " : "")}ID: [{field.RequiredReference.ObjectID.ToString()}]\n"));
                indent++;
                {
                    sb.Append($"{Indent()}Name: »{field.Name}«\n");
                    sb.Append($"{Indent()}FullName: »{field.FullName}«\n");
                    sb.Append($"{Indent()}Value: »{field.Value}«\n");
                    if (field.ContainsWidgetPart)
                    {
                        sb.Append($"{Indent()}Widget part:\n");
                        indent++;
                        {
                            var pageRef = field.Elements.GetReference(PdfAnnotation.Keys.P);
                            var page = pageRef != null
                                  ? pageRef.ObjectID.ToString()
                                  : "none";
                            sb.Append($"{Indent()}Page: [{page}]\n");
                        }
                        indent--;
                    }
                    if (field.HasKids)
                    {
                        sb.Append($"{Indent()}Kids: {field.Kids.Elements.Count}\n");
                        indent++;
                        {
                            HandleKids(field.Kids);
                        }
                        indent--;
                    }
                }
                indent--;
            }

#pragma warning disable CS8321 // Local function is declared but never used
            void HandleWidget(PdfFormFields fields)
#pragma warning restore CS8321 // Local function is declared but never used
            {
                foreach (var field in fields)
                {
                    HandleField(field);
                }
            }

            void HandleKids(PdfFormFields fields)
            {
                foreach (var field in fields)
                {
                    HandleField(field);
                }
            }

#pragma warning disable CS8321 // Local function is declared but never used
            void AppendIndent() => sb.Append(new String(' ', indent * 2));
#pragma warning restore CS8321 // Local function is declared but never used
            string Indent() => new String(' ', indent * 2);
        }
    }
}
#endif
