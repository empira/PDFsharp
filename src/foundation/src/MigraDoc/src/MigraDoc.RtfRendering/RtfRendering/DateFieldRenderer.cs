// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System;
using MigraDoc.DocumentObjectModel;
using System.Globalization;
using System.Diagnostics;
using MigraDoc.DocumentObjectModel.Fields;
using MigraDoc.RtfRendering.Resources;

namespace MigraDoc.RtfRendering
{
    /// <summary>
    /// Renders a date field to RTF.
    /// </summary>
    class DateFieldRenderer : FieldRenderer
    {
        internal DateFieldRenderer(DocumentObject domObj, RtfDocumentRenderer docRenderer)
            : base(domObj, docRenderer)
        {
            _dateField = (DateField)domObj;
        }

        /// <summary>
        /// Renders a date field to RTF.
        /// </summary>
        internal override void Render()
        {
            StartField();
            _rtfWriter.WriteText("DATE ");
            TranslateFormat();
            EndField();
        }

        /// <summary>
        /// Translates the date field format to RTF.
        /// </summary>
        void TranslateFormat()
        {
            string domFrmt = _dateField.Format;
            string rtfFrmt = domFrmt;

            //The format is translated using the current culture.
            DateTimeFormatInfo dtfInfo = CultureInfo.CurrentCulture.DateTimeFormat;
            if (domFrmt == "")
                rtfFrmt = dtfInfo.ShortDatePattern + " " + dtfInfo.LongTimePattern;

            else if (domFrmt.Length == 1)
            {
                switch (domFrmt)
                {
                    case "d":
                        rtfFrmt = dtfInfo.ShortDatePattern;
                        break;

                    case "D":
                        rtfFrmt = dtfInfo.LongDatePattern;
                        break;

                    case "T":
                        rtfFrmt = dtfInfo.LongTimePattern;
                        break;

                    case "t":
                        rtfFrmt = dtfInfo.ShortTimePattern;
                        break;

                    case "f":
                        rtfFrmt = dtfInfo.LongDatePattern + " " + dtfInfo.ShortTimePattern;
                        break;

                    case "F":
                        rtfFrmt = dtfInfo.FullDateTimePattern;
                        break;

                    case "G":
                        rtfFrmt = dtfInfo.ShortDatePattern + " " + dtfInfo.LongTimePattern;
                        break;

                    case "g":
                        rtfFrmt = dtfInfo.ShortDatePattern + " " + dtfInfo.ShortTimePattern;
                        break;

                    case "M":
                    case "m":
                        rtfFrmt = dtfInfo.MonthDayPattern;
                        break;

                    case "R":
                    case "r":
                        rtfFrmt = dtfInfo.RFC1123Pattern;
                        break;

                    case "s":
                        rtfFrmt = dtfInfo.SortableDateTimePattern;
                        break;

                    //TODO: Output universal time for u und U.
                    case "u":
                        rtfFrmt = dtfInfo.UniversalSortableDateTimePattern;
                        break;

                    case "U":
                        rtfFrmt = dtfInfo.FullDateTimePattern;
                        break;

                    case "Y":
                    case "y":
                        rtfFrmt = dtfInfo.YearMonthPattern;
                        break;

                    default:
                        break;
                }
            }
            bool isEscaped = false;
            bool isQuoted = false;
            bool isSingleQuoted = false;
            string rtfFrmt2 = "\\@ \"";
            foreach (char c in rtfFrmt)
            {
                switch (c)
                {
                    case '\\':
                        if (isEscaped)
                            rtfFrmt2 += "'" + '\\' + "'";

                        isEscaped = !isEscaped;
                        break;

                    case '\'':
                        if (isEscaped)
                        {
                            //Doesn't work in word format strings.
                            Debug.WriteLine(Messages2.CharacterNotAllowedInDateFormat(c), "warning");
                            isEscaped = false;
                        }
                        else if (!isSingleQuoted && !isQuoted)
                        {
                            isSingleQuoted = true;
                            rtfFrmt2 += c;
                        }
                        else if (isQuoted)
                        {
                            rtfFrmt2 += @"\'";
                        }
                        else if (isSingleQuoted)
                        {
                            isSingleQuoted = false;
                            rtfFrmt2 += c;
                        }
                        break;

                    case '"':
                        if (isEscaped)
                        {
                            rtfFrmt2 += c;
                            isEscaped = false;
                        }
                        else if (!isQuoted && !isSingleQuoted)
                        {
                            isQuoted = true;
                            rtfFrmt2 += '\'';
                        }
                        else if (isQuoted)
                        {
                            isQuoted = false;
                            rtfFrmt2 += '\'';
                        }
                        else if (isSingleQuoted)
                        {
                            rtfFrmt2 += "\\\"";
                        }
                        break;

                    case '/':
                        if (isEscaped || isQuoted || isSingleQuoted)
                        {
                            isEscaped = false;
                            rtfFrmt2 += c;
                        }
                        else
                        {
                            rtfFrmt2 += dtfInfo.DateSeparator;
                        }
                        break;

                    case ':':
                        if (isEscaped || isQuoted || isSingleQuoted)
                        {
                            isEscaped = false;
                            rtfFrmt2 += c;
                        }
                        else
                        {
                            rtfFrmt2 += dtfInfo.TimeSeparator;
                        }
                        break;

                    default:
                        if (isEscaped)
                            rtfFrmt2 += "'" + c + "'";
                        else if (!isQuoted && !isSingleQuoted)
                            rtfFrmt2 += TranslateCustomFormatChar(c);
                        else
                            rtfFrmt2 += c;

                        isEscaped = false;
                        break;
                }
            }
            _rtfWriter.WriteText(rtfFrmt2 + @""" \* MERGEFORMAT");
        }

        /// <summary>
        /// Translates an unescaped character of a DateField's custom format to RTF.
        /// </summary>
        string TranslateCustomFormatChar(char ch)
        {
            switch (ch)
            {
                case 'y':
                case 'M':
                case 'd':
                case 'H':
                case 'h':
                case 'm':
                case 's':
                    return ch.ToString();

                default:
                    return "'" + ch + "'";
            }
        }

        /// <summary>
        /// Gets the current date in the correct format.
        /// </summary>
        protected override string GetFieldResult()
        {
            return DateTime.Now.ToString(_dateField.Format);
        }

        readonly DateField _dateField;
    }
}
