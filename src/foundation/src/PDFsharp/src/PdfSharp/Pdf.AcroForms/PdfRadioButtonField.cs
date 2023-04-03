// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.AcroForms
{
    /// <summary>
    /// Represents the radio button field.
    /// </summary>
    public sealed class PdfRadioButtonField : PdfButtonField
    {
        /// <summary>
        /// Initializes a new instance of PdfRadioButtonField.
        /// </summary>
        internal PdfRadioButtonField(PdfDocument document)
            : base(document)
        {
            _document = document;
        }

        internal PdfRadioButtonField(PdfDictionary dict)
            : base(dict)
        { }

        /// <summary>
        /// Gets or sets the index of the selected radio button in a radio button group.
        /// </summary>
        public int SelectedIndex
        {
            get
            {
                string value = Elements.GetString(PdfAcroField.Keys.V);
                return IndexInOptStrings(value);
            }
            set
            {
                var opt = Elements[Keys.Opt] as PdfArray;

                if (opt == null)
                    opt = Elements[PdfAcroField.Keys.Kids] as PdfArray;

                if (opt != null)
                {
                    int count = opt.Elements.Count;
                    if (value < 0 || value >= count)
                        throw new ArgumentOutOfRangeException(nameof(value));
                    Elements.SetName(PdfAcroField.Keys.V, opt.Elements[value].ToString() ?? NRT.ThrowOnNull<string>());
                }
            }
        }

        int IndexInOptStrings(string value)
        {
            if (Elements[Keys.Opt] is PdfArray opt)
            {
                int count = opt.Elements.Count;
                for (int idx = 0; idx < count; idx++)
                {
                    var item = opt.Elements[idx];
                    if (item is PdfString)
                    {
                        if (item.ToString() == value)
                            return idx;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// Predefined keys of this dictionary. 
        /// The description comes from PDF 1.4 Reference.
        /// </summary>
        public new class Keys : PdfButtonField.Keys
        {
            /// <summary>
            /// (Optional; inheritable; PDF 1.4) An array of text strings to be used in
            /// place of the V entries for the values of the widget annotations representing
            /// the individual radio buttons. Each element in the array represents
            /// the export value of the corresponding widget annotation in the
            /// Kids array of the radio button field.
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Optional)]
            public const string Opt = "/Opt";

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
