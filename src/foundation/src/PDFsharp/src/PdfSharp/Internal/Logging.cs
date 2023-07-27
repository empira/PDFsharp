// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

using Microsoft.Extensions.Logging;

namespace PdfSharp.Internal
{
    /// <summary>
    /// NYI DOC
    /// </summary>
    public static class AppLogEvents
    {
        /// <summary>
        /// The dummy event.
        /// </summary>
        public static EventId Dummy = new(0815, "Dummy event id");

        // TODO Not yet used.

        //public static EventId FontCreated = new(1000, "Font created");
        //public static EventId FontFound = new(1001, "Font found");
        //public static EventId FontNotFound = new(1002, "Font not found");

#if DEBUG_

        public static EventId Create = new(1000, "Created");
        public static EventId Read = new(1001, "Read");
        public static EventId Update = new(1002, "Updated");
        public static EventId Delete = new(1003, "Deleted");

        // These are also valid EventId instances, as there's
        // an implicit conversion from int to an EventId
        public const int Details = 3000;
        public const int Error = 3001;
        public
        public static EventId ReadNotFound = 4000;
        public static EventId UpdateNotFound = 4001;

#endif
    }
}
