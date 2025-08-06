// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System;
using MigraDoc.DocumentObjectModel;
using System.Globalization;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using MigraDoc.DocumentObjectModel.Fields;
using MigraDoc.Logging;

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

        string GetEffectiveFormat(DateField dateField, out DateTimeFormatInfo dtfInfo)
        {
            var culture = dateField.Document!.EffectiveCulture;
            dtfInfo = culture.DateTimeFormat;

            var format = dateField.Format;
            if (!String.IsNullOrEmpty(format))
                return format;

            return dtfInfo.ShortDatePattern + " " + dtfInfo.LongTimePattern;
        }

        /// <summary>
        /// Translates the date field format to RTF.
        /// </summary>
        void TranslateFormat()
        {
            // The format is translated using the document’s actual culture.
            var format = GetEffectiveFormat(_dateField, out var dtfInfo);

            if (format.Length == 1)
            {
                switch (format)
                {
                    case "d":
                        format = dtfInfo.ShortDatePattern;
                        break;

                    case "D":
                        format = dtfInfo.LongDatePattern;
                        break;

                    case "T":
                        format = dtfInfo.LongTimePattern;
                        break;

                    case "t":
                        format = dtfInfo.ShortTimePattern;
                        break;

                    case "f":
                        format = dtfInfo.LongDatePattern + " " + dtfInfo.ShortTimePattern;
                        break;

                    case "F":
                        format = dtfInfo.FullDateTimePattern;
                        break;

                    case "G":
                        format = dtfInfo.ShortDatePattern + " " + dtfInfo.LongTimePattern;
                        break;

                    case "g":
                        format = dtfInfo.ShortDatePattern + " " + dtfInfo.ShortTimePattern;
                        break;

                    case "M":
                    case "m":
                        format = dtfInfo.MonthDayPattern;
                        break;

                    case "R":
                    case "r":
                        format = dtfInfo.RFC1123Pattern;
                        break;

                    case "s":
                        format = dtfInfo.SortableDateTimePattern;
                        break;

                    //TODO_OLD: Output universal time for u and U.
                    case "u":
                        format = dtfInfo.UniversalSortableDateTimePattern;
                        break;

                    case "U":
                        format = dtfInfo.FullDateTimePattern;
                        break;

                    case "Y":
                    case "y":
                        format = dtfInfo.YearMonthPattern;
                        break;

                    default:
                        break;
                }
            }
            bool isEscaped = false;
            bool isQuoted = false;
            bool isSingleQuoted = false;
            string rtfFrmt2 = "\\@ \"";
            foreach (char c in format)
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
                            //Doesn’t work in Word format strings.
                            MigraDocLogHost.RtfRenderingLogger.LogWarning(MdRtfMsgs.CharacterNotAllowedInDateFormat(c).Message);
                            //Debug.WriteLine(Messages2.CharacterNotAllowedInDateFormat(c), "warning");
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
        /// Translates an unescaped character of a DateField’s custom format to RTF.
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
            return DateTime.Now.ToString(GetEffectiveFormat(_dateField, out _));
        }

        readonly DateField _dateField;
    }
}
