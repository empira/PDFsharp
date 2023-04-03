// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Internal
{
    /// <summary>
    /// Static locking functions to make PDFsharp thread save.
    /// </summary>
    static class Lock
    {
        public static void EnterGdiPlus()
        {
            //if (_fontFactoryLockCount > 0)
            //    throw new InvalidOperationException("");

            Monitor.Enter(GdiPlus);
            _gdiPlusLockCount++;
        }

        public static void ExitGdiPlus()
        {
            _gdiPlusLockCount--;
            Monitor.Exit(GdiPlus);
        }

        static readonly object GdiPlus = new();
        static int _gdiPlusLockCount;

        public static void EnterFontFactory()
        {
            Monitor.Enter(FontFactory);
            _fontFactoryLockCount++;
        }

        public static void ExitFontFactory()
        {
            _fontFactoryLockCount--;
            Monitor.Exit(FontFactory);
        }

        static readonly object FontFactory = new();
        [ThreadStatic] static int _fontFactoryLockCount;
    }
}
