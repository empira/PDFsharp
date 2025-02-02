﻿using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Annotations;

namespace PdfSharp.Pdf.AcroForms
{
    /// <summary>
    /// Imports <see cref="PdfAcroForm"/>s
    /// </summary>
    internal sealed class PdfAcroFormImporter : PdfObject
    {
        /// <summary>
        /// Create a new <see cref="PdfAcroFormImporter"/> for importing into the specified <paramref name="targetDocument"/>
        /// </summary>
        /// <param name="targetDocument"></param>
        internal PdfAcroFormImporter(PdfDocument targetDocument)
            : base(targetDocument)
        {
        }

        internal void ImportAcroForm(PdfAcroForm remoteForm,
            Func<PdfAcroField, bool>? fieldFilter = null,
            Action<PdfAcroField, PdfAcroField>? fieldHandler = null)
        {
            // skip, if there is no AcroForm or an AcroForm without fields
            if (remoteForm == null || !remoteForm.Fields.Names.Any())
                return;

            var importedObjectTable = Owner.FormTable.GetImportedObjectTable(remoteForm.Owner);
            var needNewForm = Owner.Catalog.AcroForm == null;
            var localForm = Owner.GetOrCreateAcroForm();
            if (needNewForm)
            {
                if (remoteForm.Elements.ContainsKey(PdfAcroForm.Keys.CO))
                    localForm.Elements[PdfAcroForm.Keys.CO] = ImportClosure(importedObjectTable, _document, remoteForm.Elements.GetObject(PdfAcroForm.Keys.CO)!);
                if (remoteForm.Elements.ContainsKey(PdfAcroForm.Keys.DA))
                    localForm.Elements[PdfAcroForm.Keys.DA] = remoteForm.Elements[PdfAcroForm.Keys.DA];
                if (remoteForm.Elements.ContainsKey(PdfAcroForm.Keys.DR))
                    localForm.Elements[PdfAcroForm.Keys.DR] = ImportClosure(importedObjectTable, _document, remoteForm.Elements.GetObject(PdfAcroForm.Keys.DR)!);
                if (remoteForm.Elements.ContainsKey(PdfAcroForm.Keys.NeedAppearances))
                    localForm.Elements[PdfAcroForm.Keys.NeedAppearances] = remoteForm.Elements[PdfAcroForm.Keys.NeedAppearances];
                if (remoteForm.Elements.ContainsKey(PdfAcroForm.Keys.Q))
                    localForm.Elements[PdfAcroForm.Keys.Q] = remoteForm.Elements[PdfAcroForm.Keys.Q];
                if (remoteForm.Elements.ContainsKey(PdfAcroForm.Keys.SigFlags))
                    localForm.Elements[PdfAcroForm.Keys.SigFlags] = remoteForm.Elements[PdfAcroForm.Keys.SigFlags];
                if (remoteForm.Elements.ContainsKey(PdfAcroForm.Keys.XFA))
                    localForm.Elements[PdfAcroForm.Keys.XFA] = ImportClosure(importedObjectTable, Owner, remoteForm.Elements.GetObject(PdfAcroForm.Keys.XFA)!);
            }
            else
            {
                // copy resources from the imported AcroForm to the local form
                var extResources = remoteForm.Elements.GetDictionary(PdfAcroForm.Keys.DR);
                if (extResources != null)
                {
                    var localResources = localForm.Elements.GetDictionary(PdfAcroForm.Keys.DR) ?? new PdfDictionary(Owner);
                    var resourceKeys = new[] { PdfResources.Keys.Font, PdfResources.Keys.XObject };
                    foreach (var resKey in resourceKeys)
                    {
                        var extResDict = extResources.Elements.GetDictionary(resKey);
                        if (extResDict != null)
                        {
                            var localResDict = localResources.Elements.GetDictionary(resKey) ?? new PdfDictionary(Owner);
                            foreach (var key in extResDict.Elements.Keys)
                            {
                                if (!localResDict.Elements.ContainsKey(key))
                                    localResDict.Elements.Add(key, ImportClosure(importedObjectTable, Owner, extResDict.Elements.GetObject(key)!));
                            }
                            if (!localResources.Elements.ContainsKey(resKey))
                                localResources.Elements.Add(resKey, localResDict);
                        }
                    }
                    if (!localForm.Elements.ContainsKey(PdfAcroForm.Keys.DR))
                        localForm.Elements.Add(PdfAcroForm.Keys.DR, localResources);
                }
            }

            for (var f = 0; f < remoteForm.Fields.Elements.Count; f++)
            {
                var remoteField = remoteForm.Fields[f];
                if (fieldFilter != null && !fieldFilter(remoteField))
                    continue;
                ImportAcroField(localForm, remoteField, null, fieldHandler);
            }
        }

        private void ImportAcroField(PdfAcroForm localForm, PdfAcroField remoteField, PdfAcroField? parentField = null,
            Action<PdfAcroField, PdfAcroField>? fieldHandler = null)
        {
            var importedObjectTable = Owner.FormTable.GetImportedObjectTable(remoteField.Owner);
            var annotationsImported = false;

            PdfAcroField importedField = remoteField.GetType().Name switch
            {
                nameof(PdfCheckBoxField) => localForm.AddCheckBoxField(checkBoxField =>
                {
                    var externalCheckBoxField = (PdfCheckBoxField)remoteField;
                    checkBoxField.Name = remoteField.Name;
                    checkBoxField.Checked = externalCheckBoxField.Checked;
                    parentField?.AddChild(checkBoxField);
                }),
                nameof(PdfComboBoxField) => localForm.AddComboBoxField(comboBoxField =>
                {
                    var externalComboBoxField = (PdfComboBoxField)remoteField;
                    comboBoxField.Name = remoteField.Name;
                    comboBoxField.Options = externalComboBoxField.Options;
                    comboBoxField.SelectedIndex = externalComboBoxField.SelectedIndex;
                    if (remoteField.Elements.ContainsKey(PdfChoiceField.Keys.Opt))
                        comboBoxField.Elements[PdfChoiceField.Keys.Opt] = remoteField.Elements[PdfChoiceField.Keys.Opt]!.Clone();
                    parentField?.AddChild(comboBoxField);
                }),
                nameof(PdfListBoxField) => localForm.AddListBoxField(listBoxField =>
                {
                    var externalListBoxField = (PdfListBoxField)remoteField;
                    listBoxField.Name = remoteField.Name;
                    listBoxField.Options = externalListBoxField.Options;
                    listBoxField.SelectedIndices = externalListBoxField.SelectedIndices;
                    if (remoteField.Elements.ContainsKey(PdfChoiceField.Keys.Opt))
                        listBoxField.Elements[PdfChoiceField.Keys.Opt] = remoteField.Elements[PdfChoiceField.Keys.Opt]!.Clone();
                    parentField?.AddChild(listBoxField);
                }),
                nameof(PdfRadioButtonField) => localForm.AddRadioButtonField(radioButtonField =>
                {
                    var extRadioButtonField = (PdfRadioButtonField)remoteField;
                    // must copy annotations here, because SelectedIndex relies on them
                    ImportFieldAnnotations(radioButtonField, remoteField);
                    annotationsImported = true;
                    radioButtonField.Name = remoteField.Name;
                    radioButtonField.SelectedIndex = extRadioButtonField.SelectedIndex;
                    if (remoteField.Elements.ContainsKey(PdfRadioButtonField.Keys.Opt))
                        radioButtonField.Elements[PdfRadioButtonField.Keys.Opt] = remoteField.Elements[PdfRadioButtonField.Keys.Opt]!.Clone();
                    parentField?.AddChild(radioButtonField);
                }),
                nameof(PdfSignatureField) => localForm.AddSignatureField(signatureField =>
                {
                    signatureField.Name = remoteField.Name;
                    if (remoteField.Elements.ContainsKey(PdfSignatureField.Keys.Lock))
                        signatureField.Elements[PdfSignatureField.Keys.Lock] = ImportClosure(importedObjectTable, Owner, remoteField.Elements.GetObject(PdfSignatureField.Keys.Lock)!);
                    if (remoteField.Elements.ContainsKey(PdfSignatureField.Keys.SV))
                        signatureField.Elements[PdfSignatureField.Keys.SV] = ImportClosure(importedObjectTable, Owner, remoteField.Elements.GetObject(PdfSignatureField.Keys.SV)!);
                    parentField?.AddChild(signatureField);
                }),
                nameof(PdfGenericField) => localForm.AddGenericField(genericField =>
                {
                    genericField.Name = remoteField.Name;
                    parentField?.AddChild(genericField);
                }),
                nameof(PdfTextField) => localForm.AddTextField(textField =>
                {
                    var externalTextField = (PdfTextField)remoteField;
                    textField.Name = remoteField.Name;
                    textField.MaxLength = externalTextField.MaxLength;
                    textField.Text = externalTextField.Text;
                    parentField?.AddChild(textField);
                }),
                nameof(PdfPushButtonField) => localForm.AddPushButtonField(pushButton =>
                {
                    pushButton.Name = remoteField.Name;
                    parentField?.AddChild(pushButton);
                }),
                _ => throw new NotImplementedException($"Field type {remoteField.GetType().Name} is not handled"),
            };
            // copy common properties
            if (!string.IsNullOrEmpty(importedField.AlternateName))
                importedField.AlternateName = remoteField.AlternateName;
            if (!string.IsNullOrEmpty(importedField.MappingName))
                importedField.MappingName = remoteField.MappingName;
            if (remoteField.DefaultValue != null && importedField is not PdfPushButtonField)
                importedField.DefaultValue = remoteField.DefaultValue;
            if (remoteField.Elements.ContainsKey(PdfAcroField.Keys.DA))
                importedField.Elements[PdfAcroField.Keys.DA] = remoteField.Elements[PdfAcroField.Keys.DA];
            if (remoteField.Elements.ContainsKey(PdfAcroField.Keys.DS))
                importedField.Elements[PdfAcroField.Keys.DS] = remoteField.Elements[PdfAcroField.Keys.DS];
            if (remoteField.Elements.ContainsKey(PdfAcroField.Keys.RV))
                importedField.Elements[PdfAcroField.Keys.RV] = remoteField.Elements[PdfAcroField.Keys.RV];
            if (remoteField.Elements.ContainsKey(PdfAcroField.Keys.AA))
                importedField.Elements[PdfAcroField.Keys.AA] = ImportClosure(importedObjectTable, Owner, remoteField.Elements.GetObject(PdfAcroField.Keys.AA)!);
            importedField.SetFlags = remoteField.Flags;
            importedField.Font = remoteField.Font;
            importedField.FontSize = remoteField.FontSize;
            importedField.ForeColor = remoteField.ForeColor;
            importedField.TextAlign = remoteField.TextAlign;

            if (!annotationsImported)
                ImportFieldAnnotations(importedField, remoteField);

            fieldHandler?.Invoke(remoteField, importedField);

            if (remoteField.HasChildFields)
            {
                for (var i = 0; i < remoteField.Fields.Elements.Count; i++)
                    ImportAcroField(localForm, remoteField.Fields[i], importedField, fieldHandler);
            }
        }

        private void ImportFieldAnnotations(PdfAcroField localField, PdfAcroField remoteField)
        {
            var importedObjectTable = Owner.FormTable.GetImportedObjectTable(remoteField.Owner);
            foreach (var remoteAnnot in remoteField.Annotations.Elements)
            {
                // skip annotation if it is associated with a page that was not imported
                if (remoteAnnot.Page != null && !importedObjectTable.Contains(remoteAnnot.Page.ObjectID))
                    continue;

                localField.AddAnnotation(annot =>
                {
                    annot.BackColor = remoteAnnot.BackColor;
                    annot.BorderColor = remoteAnnot.BorderColor;
                    annot.Border = new PdfAnnotationBorder
                    {
                        BorderStyle = remoteAnnot.Border.BorderStyle,
                        DashPattern = remoteAnnot.Border.DashPattern,
                        HorizontalRadius = remoteAnnot.Border.HorizontalRadius,
                        VerticalRadius = remoteAnnot.Border.VerticalRadius,
                        Width = remoteAnnot.Border.Width
                    };
                    annot.Color = remoteAnnot.Color;
                    annot.Flags = remoteAnnot.Flags;
                    annot.Opacity = remoteAnnot.Opacity;
                    annot.Rectangle = remoteAnnot.Rectangle;
                    annot.Rotation = remoteAnnot.Rotation;
                    if (remoteAnnot.Elements.ContainsKey(PdfAnnotation.Keys.AP))
                        annot.Elements[PdfAnnotation.Keys.AP] = ImportClosure(importedObjectTable, _document, remoteAnnot.Elements.GetObject(PdfAnnotation.Keys.AP)!);
                    if (remoteAnnot.Elements.ContainsKey(PdfAnnotation.Keys.AS))
                        annot.Elements[PdfAnnotation.Keys.AS] = remoteAnnot.Elements[PdfAnnotation.Keys.AS];
                    if (remoteAnnot.Elements.ContainsKey(PdfAnnotation.Keys.NM))
                        annot.Elements[PdfAnnotation.Keys.NM] = remoteAnnot.Elements[PdfAnnotation.Keys.NM];
                    if (remoteAnnot.Elements.ContainsKey(PdfAnnotation.Keys.Contents))
                        annot.Elements[PdfAnnotation.Keys.Contents] = remoteAnnot.Elements[PdfAnnotation.Keys.Contents];
                    if (remoteAnnot.Elements.ContainsKey(PdfAnnotation.Keys.A))
                        annot.Elements[PdfAnnotation.Keys.A] = ImportClosure(importedObjectTable, Owner, remoteAnnot.Elements.GetObject(PdfAnnotation.Keys.A)!);
                    if (remoteAnnot.Elements.ContainsKey(PdfWidgetAnnotation.Keys.H))
                        annot.Elements[PdfWidgetAnnotation.Keys.H] = remoteAnnot.Elements[PdfWidgetAnnotation.Keys.H];
                    if (remoteAnnot.Elements.ContainsKey(PdfWidgetAnnotation.Keys.MK))
                        annot.Elements[PdfWidgetAnnotation.Keys.MK] = ImportClosure(importedObjectTable, _document, remoteAnnot.Elements.GetObject(PdfWidgetAnnotation.Keys.MK)!);
                    if (remoteAnnot.Page != null && importedObjectTable.Contains(remoteAnnot.Page.ObjectID))
                    {
                        var localPage = importedObjectTable[remoteAnnot.Page.ObjectID]!.Value as PdfPage;
                        // avoid duplicate annotations (page-import already imported annotations)
                        if (localPage != null
                            && importedObjectTable.Contains(remoteAnnot.ObjectID)
                            && importedObjectTable[remoteAnnot.ObjectID].Value is PdfDictionary importedAnnot)
                        {
                            localPage.Annotations.Remove(new PdfGenericAnnotation(importedAnnot));
                        }
                        annot.Page = localPage;
                    }
                });
            }
        }
    }
}
