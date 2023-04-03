// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]

// TODO We should add a WPF Preview panel
//#if WPF
//[assembly: XmlnsDefinition("http://schemas.empira.com/pdfsharp/2010/xaml/presentation", "PdfSharp.Windows")]
//#endif

// Temporary HACK for PDFsharp Universal Accessibility.
[assembly: InternalsVisibleTo("PdfSharp.UA")]
[assembly: InternalsVisibleTo("PdfSharp.UA-wpf")]