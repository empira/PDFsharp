
using System.Reflection;
//#if NET/FX_CORE
//using System.Threading.Tasks;
//#endif
#if WPF
using System.Windows;
using System.Windows.Resources;
#endif
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace PdfSharp.Quality
{
    /// <summary>
    /// Static helper functions for fonts.
    /// </summary>
    public static class AssetsHelper
    {
        /// <summary>
        /// Returns the specified font from an embedded resource.
        /// </summary>
        public static void EnsureAssets()
        {
            //
        }
    }
}
