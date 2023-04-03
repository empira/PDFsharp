// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using PdfSharp.Drawing;

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents the predefined column/bar chart colors.
    /// </summary>
    sealed class ColumnColors
    {
        /// <summary>
        /// Gets the color for column/bar charts from the specified index.
        /// </summary>
        public static XColor Item(int index)
            => XColor.FromArgb((int)SeriesColorValues[index]);

        /// <summary>
        /// Colors for column/bar charts taken from Excel.
        /// </summary>
        static readonly uint[] SeriesColorValues = new uint[]
        {
          0xFF9999FF, 0xFF993366, 0xFFFFFFCC, 0xFFCCFFFF, 0xFF660066, 0xFFFF8080,
          0xFF0066CC, 0xFFCCCCFF, 0xFF000080, 0xFFFF00FF, 0xFFFFFF00, 0xFF00FFFF,
          0xFF800080, 0xFF800000, 0xFF008080, 0xFF0000FF, 0xFF00CCFF, 0xFFCCFFFF,
          0xFFCCFFCC, 0xFFFFFF99, 0xFF99CCFF, 0xFFFF99CC, 0xFFCC99FF, 0xFFFFCC99,
          0xFF3366FF, 0xFF33CCCC, 0xFF99CC00, 0xFFFFCC00, 0xFFFF9900, 0xFFFF6600,
          0xFF666699, 0xFF969696, 0xFF003366, 0xFF339966, 0xFF003300, 0xFF333300,
          0xFF993300, 0xFF993366, 0xFF333399, 0xFF333333, 0xFF000000, 0xFFFFFFFF,
          0xFFFF0000, 0xFF00FF00, 0xFF0000FF, 0xFFFFFF00, 0xFFFF00FF, 0xFF00FFFF,
          0xFF800000, 0xFF008000, 0xFF000080, 0xFF808000, 0xFF800080, 0xFF008080,
          0xFFC0C0C0, 0xFF808080
        };
    }

    /// <summary>
    /// Represents the predefined line chart colors.
    /// </summary>
    static class LineColors
    {
        /// <summary>
        /// Gets the color for line charts from the specified index.
        /// </summary>
        public static XColor Item(int index) 
            => XColor.FromArgb((int)LineColorValues[index]);

        /// <summary>
        /// Colors for line charts taken from Excel.
        /// </summary>
        static readonly uint[] LineColorValues = new uint[]
        {
          0xFF000080, 0xFFFF00FF, 0xFFFFFF00, 0xFF00FFFF, 0xFF800080, 0xFF800000,
          0xFF008080, 0xFF0000FF, 0xFF00CCFF, 0xFFCCFFFF, 0xFFCCFFCC, 0xFFFFFF99,
          0xFF99CCFF, 0xFFFF99CC, 0xFFCC99FF, 0xFFFFCC99, 0xFF3366FF, 0xFF33CCCC,
          0xFF99CC00, 0xFFFFCC00, 0xFFFF9900, 0xFFFF6600, 0xFF666699, 0xFF969696,
          0xFF003366, 0xFF339966, 0xFF003300, 0xFF333300, 0xFF993300, 0xFF993366,
          0xFF333399, 0xFF000000, 0xFFFFFFFF, 0xFFFF0000, 0xFF00FF00, 0xFF0000FF,
          0xFFFFFF00, 0xFFFF00FF, 0xFF00FFFF, 0xFF800000, 0xFF008000, 0xFF000080,
          0xFF808000, 0xFF800080, 0xFF008080, 0xFFC0C0C0, 0xFF808080, 0xFF9999FF,
          0xFF993366, 0xFFFFFFCC, 0xFFCCFFFF, 0xFF660066, 0xFFFF8080, 0xFF0066CC,
          0xFFCCCCFF
        };
    }

    /// <summary>
    /// Represents the predefined pie chart colors.
    /// </summary>
    static class PieColors
    {
        /// <summary>
        /// Gets the color for pie charts from the specified index.
        /// </summary>
        public static XColor Item(int index) 
            => XColor.FromArgb((int)SectorColorValues[index]);

        /// <summary>
        /// Colors for pie charts taken from Excel.
        /// </summary>
        static readonly uint[] SectorColorValues =
        {
          0xFF9999FF, 0xFF993366, 0xFFFFFFCC, 0xFFCCFFFF, 0xFF660066, 0xFFFF8080,
          0xFF0066CC, 0xFFCCCCFF, 0xFF000080, 0xFFFF00FF, 0xFFFFFF00, 0xFF00FFFF,
          0xFF800080, 0xFF800000, 0xFF008080, 0xFF0000FF, 0xFF00CCFF, 0xFFCCFFFF,
          0xFFCCFFCC, 0xFFFFFF99, 0xFF99CCFF, 0xFFFF99CC, 0xFFCC99FF, 0xFFFFCC99,
          0xFF3366FF, 0xFF33CCCC, 0xFF99CC00, 0xFFFFCC00, 0xFFFF9900, 0xFFFF6600,
          0xFF666699, 0xFF969696, 0xFF003366, 0xFF339966, 0xFF003300, 0xFF333300,
          0xFF993300, 0xFF993366, 0xFF333399, 0xFF333333, 0xFF000000, 0xFFFFFFFF,
          0xFFFF0000, 0xFF00FF00, 0xFF0000FF, 0xFFFFFF00, 0xFFFF00FF, 0xFF00FFFF,
          0xFF800000, 0xFF008000, 0xFF000080, 0xFF808000, 0xFF800080, 0xFF008080,
          0xFFC0C0C0, 0xFF808080
        };
    }
}
