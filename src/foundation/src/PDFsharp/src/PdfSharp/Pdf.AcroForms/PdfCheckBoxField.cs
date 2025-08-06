// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Pdf.Annotations;
using PdfSharp.Pdf.Advanced;

namespace PdfSharp.Pdf.AcroForms
{
    /// <summary>
    /// Represents the check box field.
    /// </summary>
    public sealed class PdfCheckBoxField : PdfButtonField
    {
        /// <summary>
        /// Initializes a new instance of PdfCheckBoxField.
        /// </summary>
        internal PdfCheckBoxField(PdfDocument document)
            : base(document)
        {
            _document = document;
        }

        internal PdfCheckBoxField(PdfDictionary dict)
            : base(dict)
        { }

#if true_
        /// <summary>
        /// Indicates whether the field is checked.
        /// </summary>
        public bool Checked  //R080317 // TODO_OLD
        {
            get
            {
                if (!HasKids)
                {
                    string value = Elements.GetString(Keys.V);
                    //return !String.IsNullOrEmpty(value) && value != UncheckedValue;
                    return !String.IsNullOrEmpty(value) && value == CheckedName;
                }

                if (Fields.Elements.Items.Length == 2)
                {
                    string value = ((PdfDictionary)(((PdfReference)(Fields.Elements.Items[0])).Value)).Elements.GetString(Keys.V);
                    //bool bReturn = value.Length != 0 && value != UncheckedValue; //R081114 (3Std.!!) auch auf Nein prüfen; //TODO_OLD woher kommt der Wert?
                    bool bReturn = value.Length != 0 && value == CheckedName;
                    return bReturn;
                }

                // NYI: Return false in any other case. 
                return false;
            }

            set
            {
                if (!HasKids)
                {
                    //string name = value ? GetNonOffValue() : "/Off";
                    string name = value ? CheckedName : UncheckedName;
                    Elements.SetName(Keys.V, name);
                    Elements.SetName(PdfAnnotation.Keys.AS, name);
                }
                else
                {
                    // Here we have to handle fields that exist twice with the same name.
                    // Checked must be set for both fields, using /Off for one field and skipping /Off for the other,
                    // to have only one field with a check mark.
                    // Finding this took me two working days.
                    if (Fields.Elements.Items.Length == 2)
                    {
                        if (value)
                        {
                            //Element 0 behandeln -> auf checked setzen
                            string name1 = "";
                            PdfDictionary o = ((PdfDictionary)(((PdfReference)(Fields.Elements.Items[0])).Value)).Elements["/AP"] as PdfDictionary;
                            if (o != null)
                            {
                                PdfDictionary n = o.Elements["/N"] as PdfDictionary;
                                if (n != null)
                                {
                                    foreach (string name in n.Elements.Keys)
                                    {
                                        //if (name != UncheckedValue)
                                        if (name == CheckedName)
                                        {
                                            name1 = name;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (name1.Length != 0)
                            {
                                ((PdfDictionary)(((PdfReference)(Fields.Elements.Items[0])).Value)).Elements.SetName(Keys.V, name1);
                                ((PdfDictionary)(((PdfReference)(Fields.Elements.Items[0])).Value)).Elements.SetName(PdfAnnotation.Keys.AS, name1);
                            }

                            //Element 1 behandeln -> auf unchecked setzen
                            o = ((PdfDictionary)(((PdfReference)(Fields.Elements.Items[1])).Value)).Elements["/AP"] as PdfDictionary;
                            if (o != null)
                            {
                                PdfDictionary n = o.Elements["/N"] as PdfDictionary;
                                if (n != null)
                                {
                                    foreach (string name in n.Elements.Keys)
                                    {
                                        if (name == UncheckedName)
                                        {
                                            name1 = name;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (!String.IsNullOrEmpty(name1))
                            {
                                ((PdfDictionary)(((PdfReference)(Fields.Elements.Items[1])).Value)).Elements.SetName(Keys.V, name1);
                                ((PdfDictionary)(((PdfReference)(Fields.Elements.Items[1])).Value)).Elements.SetName(PdfAnnotation.Keys.AS, name1);
                            }
                        }
                        else
                        {
                            //Element 0 behandeln -> auf unchecked setzen
                            string name1 = "";
                            PdfDictionary o = ((PdfDictionary)(((PdfReference)(Fields.Elements.Items[1])).Value)).Elements["/AP"] as PdfDictionary;
                            if (o != null)
                            {
                                PdfDictionary n = o.Elements["/N"] as PdfDictionary;
                                if (n != null)
                                {
                                    foreach (string name in n.Elements.Keys)
                                    {
                                        //if (name != UncheckedValue)
                                        if (name == CheckedName)
                                        {
                                            name1 = name;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (name1.Length != 0)
                            {
                                ((PdfDictionary)(((PdfReference)(Fields.Elements.Items[1])).Value)).Elements.SetName(Keys.V, name1);
                                ((PdfDictionary)(((PdfReference)(Fields.Elements.Items[1])).Value)).Elements.SetName(PdfAnnotation.Keys.AS, name1);
                            }

                            //Element 1 behandeln -> auf checked setzen
                            o = ((PdfDictionary)(((PdfReference)(Fields.Elements.Items[0])).Value)).Elements["/AP"] as PdfDictionary;
                            if (o != null)
                            {
                                PdfDictionary n = o.Elements["/N"] as PdfDictionary;
                                if (n != null)
                                {
                                    foreach (string name in n.Elements.Keys)
                                    {
                                        if (name == UncheckedName)
                                        {
                                            name1 = name;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (name1.Length != 0)
                            {
                                ((PdfDictionary)(((PdfReference)(Fields.Elements.Items[0])).Value)).Elements.SetName(Keys.V, name1);
                                ((PdfDictionary)(((PdfReference)(Fields.Elements.Items[0])).Value)).Elements.SetName(PdfAnnotation.Keys.AS, name1);
                            }
                        }
                    }
                }
            }
        }

#else
        /// <summary>
        /// Indicates whether the field is checked.
        /// </summary>
        public bool Checked
        {
            get
            {
                if (!HasKids) //R080317
                {
                    string value = Elements.GetString(PdfAcroField.Keys.V);
                    return value.Length != 0 && value != "/Off";
                }
                else //R080317
                {
                    if (Fields.Elements.Items.Length == 2)
                    {
                        string value = ((PdfDictionary)(((PdfReference)(Fields.Elements.Items[0])).Value)).Elements.GetString(PdfAcroField.Keys.V);
                        bool bReturn = value.Length != 0 && value != "/Off" && value != "/Nein"; //R081114 (3Std.!!) auch auf Nein prüfen; //TODO_OLD woher kommt der Wert?
                        return bReturn;
                    }
                    else
                        return false;
                }
            }
            set
            {
                if (!HasKids)
                {
                    string name = value ? GetNonOffValue() : "/Off";
                    Elements.SetName(PdfAcroField.Keys.V, name);
                    Elements.SetName(PdfAnnotation.Keys.AS, name);
                }
                else
                {
                    // Here we have to handle fields that exist twice with the same name.
                    // Checked must be set for both fields, using /Off for one field and skipping /Off for the other,
                    // to have only one field with a check mark.
                    // Finding this took me two working days.
                    if (Fields.Elements.Items.Length == 2)
                    {
                        if (value)
                        {
                            //Element 0 behandeln -> auf checked setzen
                            string name1 = "";
                            if (((PdfDictionary)(((PdfReference)(Fields.Elements.Items[0])).Value)).Elements["/AP"] is PdfDictionary obj1)
                            {
                                if (obj1.Elements["/N"] is PdfDictionary n)
                                {
                                    foreach (string name in n.Elements.Keys)
                                    {
                                        if (name != "/Off")
                                        {
                                            name1 = name;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (name1.Length != 0)
                            {
                                ((PdfDictionary)(((PdfReference)(Fields.Elements.Items[0])).Value)).Elements.SetName(PdfAcroField.Keys.V, name1);
                                ((PdfDictionary)(((PdfReference)(Fields.Elements.Items[0])).Value)).Elements.SetName(PdfAnnotation.Keys.AS, name1);
                            }

                            //Element 1 behandeln -> auf unchecked setzen
                            var obj2 = ((PdfDictionary?)((PdfReference?)Fields.Elements.Items[1])?.Value)?.Elements["/AP"] as PdfDictionary;
                            if (obj2 != null)
                            {
                                var n = obj2.Elements["/N"] as PdfDictionary;
                                if (n != null)
                                {
                                    foreach (string name in n.Elements.Keys)
                                    {
                                        if (name == "/Off")
                                        {
                                            name1 = name;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (name1.Length != 0)
                            {
                                ((PdfDictionary)(((PdfReference)(Fields.Elements.Items[1])).Value)).Elements.SetName(PdfAcroField.Keys.V, name1);
                                ((PdfDictionary)(((PdfReference)(Fields.Elements.Items[1])).Value)).Elements.SetName(PdfAnnotation.Keys.AS, name1);
                            }

                        }
                        else
                        {
                            //Element 0 behandeln -> auf unchecked setzen
                            string name1 = "";
                            PdfDictionary? o = ((PdfDictionary)((PdfReference)(Fields.Elements.Items[1])).Value).Elements["/AP"] as PdfDictionary;
                            if (o != null)
                            {
                                if (o.Elements["/N"] is PdfDictionary n)
                                {
                                    foreach (string name in n.Elements.Keys)
                                    {
                                        if (name != "/Off")
                                        {
                                            name1 = name;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (name1.Length != 0)
                            {
                                ((PdfDictionary)(((PdfReference)(Fields.Elements.Items[1])).Value)).Elements.SetName(PdfAcroField.Keys.V, name1);
                                ((PdfDictionary)(((PdfReference)(Fields.Elements.Items[1])).Value)).Elements.SetName(PdfAnnotation.Keys.AS, name1);
                            }

                            //Element 1 behandeln -> auf checked setzen
                            o = ((PdfDictionary)(((PdfReference)(Fields.Elements.Items[0])).Value)).Elements["/AP"] as PdfDictionary;
                            if (o != null)
                            {
                                if (o.Elements["/N"] is PdfDictionary n)
                                {
                                    foreach (string name in n.Elements.Keys)
                                    {
                                        if (name == "/Off")
                                        {
                                            name1 = name;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (name1.Length != 0)
                            {
                                ((PdfDictionary)(((PdfReference)(Fields.Elements.Items[0])).Value)).Elements.SetName(PdfAcroField.Keys.V, name1);
                                ((PdfDictionary)(((PdfReference)(Fields.Elements.Items[0])).Value)).Elements.SetName(PdfAnnotation.Keys.AS, name1);
                            }
                        }
                    }
                }
            }
        }
#endif

        /// <summary>
        /// Gets or sets the name of the dictionary that represents the Checked state.
        /// </summary>
        /// The default value is "/Yes".
        public string CheckedName
        {
            get => _checkedName;
            set => _checkedName = value;
        }

        string _checkedName = "/Yes";

        /// <summary>
        /// Gets or sets the name of the dictionary that represents the Unchecked state.
        /// The default value is "/Off".
        /// </summary>
        public string UncheckedName
        {
            get => _uncheckedName;
            set => _uncheckedName = value;
        }

        string _uncheckedName = "/Off";

        /// <summary>
        /// Predefined keys of this dictionary. 
        /// The description comes from PDF 1.4 Reference.
        /// </summary>
        public new class Keys : PdfButtonField.Keys
        {
            /// <summary>
            /// (Optional; inheritable; PDF 1.4) A text string to be used in place of the V entry for the
            /// value of the field.
            /// </summary>
            [KeyInfo(KeyType.TextString | KeyType.Optional)]
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
