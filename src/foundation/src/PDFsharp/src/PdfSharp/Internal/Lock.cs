// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Internal
{
    /// <summary>
    /// Static locking functions to make PDFsharp thread save.
    /// POSSIBLE BUG: Having more than one lock can lead to a deadlock.
    /// </summary>
    static class Lock
    {
        public static void EnterGdiPlus()
        {
            //if (_fontFactoryLockCount > 0)
            //    throw new InvalidOperationException("");

            Monitor.Enter(GdiPlusLock);
            _gdiPlusLockCount++;
        }

        public static void ExitGdiPlus()
        {
            _gdiPlusLockCount--;
            Monitor.Exit(GdiPlusLock);
        }

        static readonly object GdiPlusLock = new();
        static int _gdiPlusLockCount;
        
        // ------------------------------------------------------------

        public static void EnterFontFactory()
        {
            Monitor.Enter(FontFactoryLock);
            _fontFactoryLockCount++;
        }

        public static void ExitFontFactory()
        {
            _fontFactoryLockCount--;
            Monitor.Exit(FontFactoryLock);
        }

        public static bool IsFontFactoryLookTaken() => _fontFactoryLockCount > 0;
        
        static readonly object FontFactoryLock = new();
        //[ThreadStatic] // StL: ??? - makes no sense
        static int _fontFactoryLockCount;
    }
}
