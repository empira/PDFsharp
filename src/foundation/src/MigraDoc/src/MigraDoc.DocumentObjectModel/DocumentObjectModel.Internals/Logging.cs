// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using Microsoft.Extensions.Logging;

#pragma warning disable 1591 // Because this is preview code.

namespace MigraDoc.DocumentObjectModel.Internals
{
    public static class AppLogEventIds
    {
        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once IdentifierTypo
        public const int MDDOM = 11001;

    }
    public static class AppLogEvents
    {
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once IdentifierTypo
        public static EventId MDDOM = new(AppLogEventIds.MDDOM, "NDDOM");

        // TODO_OLD Not yet used.

        public static EventId FontCreated = new(AppLogEventIds.MDDOM + 0, "Font created");
        public static EventId FontFound = new(AppLogEventIds.MDDOM + 1, "Font found");
        public static EventId FontNotFound = new(AppLogEventIds.MDDOM + 2, "Font not found");

#if DEBUG_

        public static EventId Create = new(1000, "Created");
        public static EventId Read = new(1001, "Read");
        public static EventId Update = new(1002, "Updated");
        public static EventId Delete = new(1003, "Deleted");

        // These are also valid EventId instances, as there’s
        // an implicit conversion from int to an EventId
        public const int Details = 3000;
        public const int Error = 3001;
        public
        public static EventId ReadNotFound = 4000;
        public static EventId UpdateNotFound = 4001;

#endif
    }
}
