
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
    public static class FontHelper
    {
        /// <summary>
        /// Returns the specified font from an embedded resource.
        /// </summary>
        public static byte[] LoadFontData(string name)
        {
            var assembly = typeof(PdfSharp.Quality.FontHelper).GetTypeInfo().Assembly;

            return LoadFontData(name, assembly);
        }

        /// <summary>
        /// Returns the specified font from an embedded resource.
        /// </summary>
        public static byte[] LoadFontData(string name, Assembly assembly)
        {
            using var stream = assembly.GetManifestResourceStream(name)
                               ?? throw new ArgumentException($"No resource stream '{name}' exists.");

            int count = (int)stream.Length;
            var data = new byte[count];
            if (stream.Read(data, 0, count) != count)
                throw new InvalidOperationException("Error while reading resource string.");
            return data;
        }

#if WPF
        /// <summary>
        /// Returns the specified font from a WPF Pack URI resource.
        /// </summary>
        public static byte[] LoadFontDataPack(Uri uri)
        {
            var streamInfo = System.Windows.Application.GetResourceStream(uri);
            if (streamInfo == null)
                throw new ArgumentException("No resource with name " + uri);

            // We own the stream, so we have to close it.
            using (var stream = streamInfo.Stream)
            {
                int count = (int)stream.Length;
                byte[] data = new byte[count];
                stream.Read(data, 0, count);
                return data;
            }
        }
#endif
    }
}