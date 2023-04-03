// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using PdfSharp.Pdf.Annotations;

namespace PdfSharp.Pdf.AcroForms
{
    /// <summary>
    /// Represents the base class for all button fields.
    /// </summary>
    public abstract class PdfButtonField : PdfAcroField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfButtonField"/> class.
        /// </summary>
        protected PdfButtonField(PdfDocument document)
            : base(document)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfButtonField"/> class.
        /// </summary>
        protected PdfButtonField(PdfDictionary dict)
            : base(dict)
        { }

        /// <summary>
        /// Gets the name which represents the opposite of /Off.
        /// </summary>
        protected string GetNonOffValue()
        {
            // Try to get the information from the appearance dictionary.
            // Just return the first key that is not /Off.
            // I'm not sure what is the right solution to get this value.
            var ap = Elements[PdfAnnotation.Keys.AP] as PdfDictionary;
            if (ap != null)
            {
                var n = ap.Elements["/N"] as PdfDictionary;
                if (n != null)
                {
                    foreach (string name in n.Elements.Keys)
                        if (name != "/Off")
                            return name;
                }
            }
            return null!;
        }

        internal override void GetDescendantNames(ref List<string> names, string? partialName)
        {
            string t = Elements.GetString(PdfAcroField.Keys.T);
            // HACK: ??? 
            if (t == "")
                t = "???";
            Debug.Assert(t != "");
            if (t.Length > 0)
            {
                if (!String.IsNullOrEmpty(partialName))
                    names.Add(partialName + "." + t);
                else
                    names.Add(t);
            }
        }

        /// <summary>
        /// Predefined keys of this dictionary. 
        /// The description comes from PDF 1.4 Reference.
        /// </summary>
        public new class Keys : PdfAcroField.Keys
        {
            // Pushbuttons have no additional entries.
        }
    }
}
