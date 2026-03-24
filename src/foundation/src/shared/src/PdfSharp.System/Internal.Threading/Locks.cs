// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

#pragma warning disable CS1591 // ...because this is internal stuff.

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using PdfSharp.Logging;

namespace PdfSharp.Internal.Threading
{
    /// <summary>
    /// Static locking functions to make PDFsharp thread save.
    /// </summary>
    public static class Locks  // Renamed from Lock because of the new Lock class in .NET 9.
    {
        public static void EnterGdiPlus()
        {
            Monitor.Enter(GdiPlusLock);
            //Interlocked.Increment(ref _gdiPlusLockCount);
            _gdiPlusLockCount++;  // Is atomic because we have the lock.

            if (IsFontManagementLockedByCurrentThread)
            {
                // This should not happen by design.
                LogHost.Logger.LogCritical("Entered GDI+ lock while font management lock is also taken.");
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

        public static void EnterFontManagement()
        {
            Monitor.Enter(FontManagementLock);
            _fontManagementLockCount++;  // Is atomic because we have the lock.

            if (IsGdiPlusLockedByCurrentThread)
            {
                // This should not happen by design.
                LogHost.Logger.LogCritical("Entered font management lock while GDI+ lock is also taken.");
            }
        }

        public static void ExitFontManagement()
        {
            _fontManagementLockCount--;
            //Debug.Assert(_fontManagementLockCount == 0);
            Monitor.Exit(FontManagementLock);
        }

        public static bool IsFontManagementLockedByCurrentThread => Monitor.IsEntered(FontManagementLock);

        public static bool IsFontManagementLookTaken() => _fontManagementLockCount > 0;

        static readonly object FontManagementLock = new();
        static int _fontManagementLockCount;
    }
}
