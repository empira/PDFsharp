// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Collections.Generic;

namespace PdfSharp.Pdf.Advanced
{
    /// <summary>
    /// Contains all used ExtGState objects of a document.
    /// </summary>
    public sealed class PdfExtGStateTable : PdfResourceTable
    {
        /// <summary>
        /// Initializes a new instance of this class, which is a singleton for each document.
        /// </summary>
        public PdfExtGStateTable(PdfDocument document)
            : base(document)
        { }


        /// <summary>
        /// Gets a PdfExtGState with the key 'CA' set to the specified alpha value.
        /// </summary>
        public PdfExtGState GetExtGStateStroke(double alpha, bool overprint)
        {
            string key = PdfExtGState.MakeKey(alpha, overprint);
            if (!_strokeAlphaValues.TryGetValue(key, out var extGState))
            {
                extGState = new PdfExtGState(Owner)
                {
                    //extGState.Elements[PdfExtGState.Keys.CA] = new PdfReal(alpha);
                    StrokeAlpha = alpha
                };
                if (overprint)
                {
                    extGState.StrokeOverprint = true;
                    extGState.Elements.SetInteger(PdfExtGState.Keys.OPM, 1);
                }
                _strokeAlphaValues[key] = extGState;
            }
            return extGState;
        }

        /// <summary>
        /// Gets a PdfExtGState with the key 'ca' set to the specified alpha value.
        /// </summary>
        public PdfExtGState GetExtGStateNonStroke(double alpha, bool overprint)
        {
            string key = PdfExtGState.MakeKey(alpha, overprint);
            if (!_nonStrokeStates.TryGetValue(key, out var extGState))
            {
                extGState = new PdfExtGState(Owner);
                //extGState.Elements[PdfExtGState.Keys.ca] = new PdfReal(alpha);
                extGState.NonStrokeAlpha = alpha;
                if (overprint)
                {
                    extGState.NonStrokeOverprint = true;
                    extGState.Elements.SetInteger(PdfExtGState.Keys.OPM, 1);
                }

                _nonStrokeStates[key] = extGState;
            }
            return extGState;
        }

        readonly Dictionary<string, PdfExtGState> _strokeAlphaValues = new Dictionary<string, PdfExtGState>();
        readonly Dictionary<string, PdfExtGState> _nonStrokeStates = new Dictionary<string, PdfExtGState>();
    }
}