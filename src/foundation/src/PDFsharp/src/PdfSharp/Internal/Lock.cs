// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using Microsoft.Extensions.Logging;
using PdfSharp.Logging;

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

            Monitor.Enter(GdiPlusLock);
            //Interlocked.Increment(ref _gdiPlusLockCount);
            _gdiPlusLockCount++;  // Is atomic because we have the lock.
            
            if (IsFontFactoryLockedByCurrentThread)
            {
                // This should not happen by design.
                PdfSharpLogHost.Logger.LogCritical("Entered GDI+ lock while FontFactory lock is also taken.");
            }
        }

        public static void ExitGdiPlus()
        {
            Interlocked.Decrement(ref _gdiPlusLockCount);
            Monitor.Exit(GdiPlusLock);
        }

        public static bool IsGdiPlusLockedByCurrentThread => Monitor.IsEntered(GdiPlusLock);
        
        public static bool IsGdiPlusLookTaken() => _gdiPlusLockCount > 0;


        static readonly object GdiPlusLock = new();
        static int _gdiPlusLockCount;
        
        // ------------------------------------------------------------

        public static void EnterFontFactory()
        {
            Monitor.Enter(FontFactoryLock);
            _fontFactoryLockCount++;  // Is atomic because we have the lock.

            if (IsGdiPlusLockedByCurrentThread)
            {
                // This should not happen by design.
                PdfSharpLogHost.Logger.LogCritical("Entered FontFactory lock while GDI+ lock is also taken.");
            }
        }

        public static void ExitFontFactory()
        {
            _fontFactoryLockCount--;
            Monitor.Exit(FontFactoryLock);
        }

        public static bool IsFontFactoryLockedByCurrentThread => Monitor.IsEntered(FontFactoryLock);

        public static bool IsFontFactoryLookTaken() => _fontFactoryLockCount > 0;
        
        static readonly object FontFactoryLock = new();
        static int _fontFactoryLockCount;
    }
}
