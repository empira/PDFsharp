// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

namespace PdfSharp.PlugIn
{
    /// <summary>
    /// Provisionally plug-in interface.
    /// Internal use only.
    /// </summary>
    public interface IPdfSharpPlugInV0
    {
        /// <summary>
        /// Gets the GUID of the plug-in.
        /// </summary>
        Guid ID { get; }

        /// <summary>
        /// Gets the name of the plug-in.
        /// </summary>
        string Name { get; }
    }
}
