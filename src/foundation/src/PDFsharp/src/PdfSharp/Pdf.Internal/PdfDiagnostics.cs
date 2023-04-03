// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Pdf.Internal
{
    class PdfDiagnostics
    {
        public static bool TraceCompressedObjects
        {
            get => _traceCompressedObjects;
            set => _traceCompressedObjects = value;
        }
        static bool _traceCompressedObjects = true;

        public static bool TraceXrefStreams
        {
            get => _traceXrefStreams && TraceCompressedObjects;
            set => _traceXrefStreams = value;
        }
        static bool _traceXrefStreams = true;

        public static bool TraceObjectStreams
        {
            get => _traceObjectStreams && TraceCompressedObjects;
            set => _traceObjectStreams = value;
        }
        static bool _traceObjectStreams = true;
    }
}
