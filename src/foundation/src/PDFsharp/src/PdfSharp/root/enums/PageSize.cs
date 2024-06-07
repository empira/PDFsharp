// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp
{
    /// <summary>
    /// Identifies the most popular predefined page sizes.
    /// </summary>
    public enum PageSize
    {
        /// <summary>
        /// The width or height of the page are set manually and override the PageSize property.
        /// </summary>
        Undefined = 0,

        // ISO formats
        // see https://www.engineeringtoolbox.com/drawings-paper-sheets-sizes-d_349.html

        /// <summary>
        /// Identifies a paper sheet size of 841 mm by 1189 mm or 33.11 inches by 46.81 inches.
        /// </summary>
        A0 = 1,

        /// <summary>
        /// Identifies a paper sheet size of 594 mm by 841 mm or 23.39 inches by 33.1 inches.
        /// </summary>
        A1 = 2,

        /// <summary>
        /// Identifies a paper sheet size of 420 mm by 594 mm or 16.54 inches by 23.29 inches.
        /// </summary>
        A2 = 3,

        /// <summary>
        /// Identifies a paper sheet size of 297 mm by 420 mm or 11.69 inches by 16.54 inches.
        /// </summary>
        A3 = 4,

        /// <summary>
        /// Identifies a paper sheet size of 210 mm by 297 mm or 8.27 inches by 11.69 inches.
        /// </summary>
        A4 = 5,

        /// <summary>
        /// Identifies a paper sheet size of 148 mm by 210 mm or 5.83 inches by 8.27 inches.
        /// </summary>
        A5 = 6,

        /// <summary>
        /// Identifies a paper sheet size of 860 mm by 1220 mm.
        /// </summary>
        RA0 = 7,

        /// <summary>
        /// Identifies a paper sheet size of 610 mm by 860 mm.
        /// </summary>
        RA1 = 8,

        /// <summary>
        /// Identifies a paper sheet size of 430 mm by 610 mm.
        /// </summary>
        RA2 = 9,

        /// <summary>
        /// Identifies a paper sheet size of 305 mm by 430 mm.
        /// </summary>
        RA3 = 10,

        /// <summary>
        /// Identifies a paper sheet size of 215 mm by 305 mm.
        /// </summary>
        RA4 = 11,

        /// <summary>
        /// Identifies a paper sheet size of 153 mm by 215 mm.
        /// </summary>
        RA5 = 12,

        /// <summary>
        /// Identifies a paper sheet size of 1000 mm by 1414 mm or 39.37 inches by 55.67 inches.
        /// </summary>
        B0 = 13,

        /// <summary>
        /// Identifies a paper sheet size of 707 mm by 1000 mm or 27.83 inches by 39.37 inches.
        /// </summary>
        B1 = 14,

        /// <summary>
        /// Identifies a paper sheet size of 500 mm by 707 mm or 19.68 inches by 27.83 inches.
        /// </summary>
        B2 = 15,

        /// <summary>
        /// Identifies a paper sheet size of 353 mm by 500 mm or 13.90 inches by 19.68 inches.
        /// </summary>
        B3 = 16,

        /// <summary>
        /// Identifies a paper sheet size of 250 mm by 353 mm or 9.84 inches by 13.90 inches.
        /// </summary>
        B4 = 17,

        /// <summary>
        /// Identifies a paper sheet size of 176 mm by 250 mm or 6.93 inches by 9.84 inches.
        /// </summary>
        B5 = 18,

#if true_
        /// <summary>
        /// Identifies a paper sheet size of 917 mm by 1297 mm or 36.00 inches by 51.20 inches.
        /// </summary>
        C0 = 19,

        /// <summary>
        /// Identifies a paper sheet size of 648 mm by 917 mm or 25.60 inches by 36.00 inches.
        /// </summary>
        C1 = 20,

        /// <summary>
        /// Identifies a paper sheet size of 458 mm by 648 mm or 18.00 inches by 25.60 inches.
        /// </summary>
        C2 = 21,

        /// <summary>
        /// Identifies a paper sheet size of 324 mm by 458 mm or 12.80 inches by 18.00 inches.
        /// </summary>
        C3 = 22,

        /// <summary>
        /// Identifies a paper sheet size of 229 mm by 324 mm or 9.00 inches by 12.80 inches.
        /// </summary>
        C4 = 23,

        /// <summary>
        /// Identifies a paper sheet size of 162 mm by 229 mm or 6.40 inches by 9.0 inches.
        /// </summary>
        C5 = 24,
#endif

        // Current U.S. loose paper sizes 

        /// <summary>
        /// Identifies a paper sheet size of 10 inches by 8 inches or 254 mm by 203 mm.
        /// </summary>
        Quarto = 100,

        /// <summary>
        /// Identifies a paper sheet size of 13 inches by 8 inches or 330 mm by 203 mm.
        /// </summary>
        Foolscap = 101,

        /// <summary>
        ///  Identifies a paper sheet size of 10.5 inches by 7.25 inches or 267 mm by 184 mm.
        /// </summary>
        Executive = 102,

        /// <summary>
        /// Identifies a paper sheet size of 10.5 inches by 8 inches or 267 mm by 203 mm.
        /// </summary>
        GovernmentLetter = 103,

        /// <summary>
        /// Identifies a paper sheet size of 11 inches by 8.5 inches or 279 mm by 216 mm.
        /// </summary>
        Letter = 104,

        /// <summary>
        /// Identifies a paper sheet size of 14 inches by 8.5 inches or 356 mm by 216 mm.
        /// </summary>
        Legal = 105,

        /// <summary>
        /// Identifies a paper sheet size of 17 inches by 11 inches or 432 mm by 279 mm.
        /// </summary>
        Ledger = 106,

        /// <summary>
        /// Identifies a paper sheet size of 17 inches by 11 inches or 432 mm by 279 mm.
        /// </summary>
        Tabloid = 107,

        /// <summary>
        /// Identifies a paper sheet size of 19.25 inches by 15.5 inches or 489 mm by 394 mm.
        /// </summary>
        Post = 108,

        /// <summary>
        /// Identifies a paper sheet size of 20 inches by 15 inches or 508 mm by 381 mm.
        /// </summary>
        Crown = 109,

        /// <summary>
        /// Identifies a paper sheet size of 21 inches by 16.5 inches or 533 mm by 419 mm.
        /// </summary>
        LargePost = 110,

        /// <summary>
        /// Identifies a paper sheet size of 22.5 inches by 17.5 inches or 572 mm by 445 mm.
        /// </summary>
        Demy = 111,

        /// <summary>
        /// Identifies a paper sheet size of 23 inches by 18 inches or 584 mm by 457 mm.
        /// </summary>
        Medium = 112,

        /// <summary>
        /// Identifies a paper sheet size of 25 inches by 20 inches or 635 mm by 508 mm.
        /// </summary>
        Royal = 113,

        /// <summary>
        /// Identifies a paper sheet size of 28 inches by 23 inches or 711 mm by 584 mm.
        /// </summary>
        Elephant = 114,

        /// <summary>
        /// Identifies a paper sheet size of 35 inches by 23.5 inches or 889 mm by 597 mm.
        /// </summary>
        DoubleDemy = 115,

        /// <summary>
        /// Identifies a paper sheet size of 45 inches by 35 inches or 1143 by 889 mm.
        /// </summary>
        QuadDemy = 116,

        /// <summary>
        /// Identifies a paper sheet size of 8.5 inches by 5.5 inches or 216 mm by 396 mm.
        /// </summary>
        STMT = 117,

        /// <summary>
        /// Identifies a paper sheet size of 8.5 inches by 13 inches or 216 mm by 330 mm.
        /// </summary>
        Folio = 120,

        /// <summary>
        /// Identifies a paper sheet size of 5.5 inches by 8.5 inches or 396 mm by 216 mm.
        /// </summary>
        Statement = 121,

        /// <summary>
        /// Identifies a paper sheet size of 10 inches by 14 inches.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        Size10x14 = 122,

        //A 11 × 8.5 279 × 216
        //B 17 × 11 432 × 279
        //C 22 × 17 559 × 432
        //D 34 × 22 864 × 559
        //E 44 × 34 1118 × 864
    }
}