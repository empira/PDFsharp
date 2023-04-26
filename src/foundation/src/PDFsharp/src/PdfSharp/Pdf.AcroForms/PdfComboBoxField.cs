// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.AcroForms
{
    /// <summary>
    /// Represents the combo box field.
    /// </summary>
    public sealed class PdfComboBoxField : PdfChoiceField
    {
        /// <summary>
        /// Initializes a new instance of PdfComboBoxField.
        /// </summary>
        internal PdfComboBoxField(PdfDocument document)
            : base(document)
        { }

        internal PdfComboBoxField(PdfDictionary dict)
            : base(dict)
        { }

        /// <summary>
        /// Gets or sets the index of the selected item.
        /// </summary>
        public int SelectedIndex
        {
            get
            {
                string value = Elements.GetString(PdfAcroField.Keys.V);
                return IndexInOptArray(value);
            }
            set
            {
                if (value != -1)
                {
                    string key = ValueInOptArray(value);
                    Elements.SetString(PdfAcroField.Keys.V, key);
                    Elements.SetInteger("/I", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the field.
        /// </summary>
        public override PdfItem? Value
        {
            get => Elements[PdfAcroField.Keys.V]!;
            set
            {
                if (ReadOnly)
                    throw new InvalidOperationException("The field is read only.");
                if (value is PdfString or PdfName)
                {
                    Elements[PdfAcroField.Keys.V] = value;
                    SelectedIndex = SelectedIndex;
                    if (SelectedIndex == -1)
                    {
                        try
                        {
                            ((PdfArray)((PdfItem[])Elements.Values)[2]).Elements.Add(Value!);  // NRT Value
                            SelectedIndex = SelectedIndex;
                        }
                        // ReSharper disable once EmptyGeneralCatchClause
                        catch
                        { }
                    }
                }
                else
                    throw new NotImplementedException("Values other than string cannot be set.");
            }
        }

        /// <summary>
        /// Predefined keys of this dictionary. 
        /// The description comes from PDF 1.4 Reference.
        /// </summary>
        public new class Keys : PdfAcroField.Keys
        {
            // Combo boxes have no additional entries.

            internal static DictionaryMeta Meta => Keys._meta ??= CreateMeta(typeof(Keys));

            static DictionaryMeta? _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta => Keys.Meta;
    }
}
