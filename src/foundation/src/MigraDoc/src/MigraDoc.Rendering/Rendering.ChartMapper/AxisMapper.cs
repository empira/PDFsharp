// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;
using PdfSharp.Charting;

namespace MigraDoc.Rendering.ChartMapper
{
    /// <summary>
    /// The AxisMapper class.
    /// </summary>
    public class AxisMapper
    {
        void MapObject(Axis axis, DocumentObjectModel.Shapes.Charts.Axis domAxis)
        {
            if (!domAxis.Values.TickLabels?.Values.Format.IsValueNullOrEmpty() ?? false)
                axis.TickLabels.Format = domAxis.TickLabels.Format;
            //if (!domAxis.IsNull("TickLabels.Format"))
            //    axis.TickLabels.Format = domAxis.TickLabels.Format;

            if (!domAxis.Values.TickLabels?.Values.Style.IsValueNullOrEmpty() ?? false)
            {
                Debug.Assert(domAxis.TickLabels.Document != null, "domAxis.TickLabels.Document != null");
                FontMapper.Map(axis.TickLabels.Font, domAxis.TickLabels.Document, domAxis.TickLabels.Style);
            }

            if (!domAxis.Values.TickLabels?.Values.Font.IsValueNullOrEmpty() ?? false)
                FontMapper.Map(axis.TickLabels.Font, domAxis.TickLabels.Font);

            if (domAxis.Values.MajorTickMark is not null)
                axis.MajorTickMark = (TickMarkType)domAxis.MajorTickMark;
            if (domAxis.Values.MinorTickMark is not null)
                axis.MinorTickMark = (TickMarkType)domAxis.MinorTickMark;

            if (domAxis.Values.MajorTick is not null)
                axis.MajorTick = domAxis.MajorTick;
            if (domAxis.Values.MinorTick is not null)
                axis.MinorTick = domAxis.MinorTick;

            if (!domAxis.Values.Title.IsValueNullOrEmpty())
            {
                // domAxis.Title is not null or empty here.
                axis.Title.Caption = domAxis.Title.Caption;
                if (/*!domAxis.Values.Title.IsValueNullOrEmpty() &&*/ !domAxis.Title.Values.Style.IsValueNullOrEmpty())
                {
                    Debug.Assert(domAxis.Title.Document != null, "domAxis.Title.Document != null");
                    FontMapper.Map(axis.Title.Font, domAxis.Title.Document, domAxis.Title.Style);
                }

                if (/*!domAxis.Values.Title.IsValueNullOrEmpty() &&*/ !domAxis.Title.Values.Font.IsValueNullOrEmpty())
                    FontMapper.Map(axis.Title.Font, domAxis.Title.Font);
                axis.Title.Orientation = domAxis.Title.Orientation.Value;
                axis.Title.Alignment = (HorizontalAlignment)domAxis.Title.Alignment;
                axis.Title.VerticalAlignment = (VerticalAlignment)domAxis.Title.VerticalAlignment;
            }

            axis.HasMajorGridlines = domAxis.HasMajorGridlines;
            axis.HasMinorGridlines = domAxis.HasMinorGridlines;

            if (!domAxis.Values.MajorGridlines.IsValueNullOrEmpty() && !domAxis.MajorGridlines.Values.LineFormat.IsValueNullOrEmpty())
                LineFormatMapper.Map(axis.MajorGridlines.LineFormat, domAxis.MajorGridlines.LineFormat);
            if (!domAxis.Values.MinorGridlines.IsValueNullOrEmpty() && !domAxis.MinorGridlines.Values.LineFormat.IsValueNullOrEmpty())
                LineFormatMapper.Map(axis.MinorGridlines.LineFormat, domAxis.MinorGridlines.LineFormat);

            if (domAxis.Values.MaximumScale is not null)
                axis.MaximumScale = domAxis.MaximumScale;
            if (domAxis.Values.MinimumScale is not null)
                axis.MinimumScale = domAxis.MinimumScale;

            if (!domAxis.Values.LineFormat.IsValueNullOrEmpty())
                LineFormatMapper.Map(axis.LineFormat, domAxis.LineFormat);
        }

        internal static void Map(Axis axis, DocumentObjectModel.Shapes.Charts.Axis domAxis)
        {
            var mapper = new AxisMapper();
            mapper.MapObject(axis, domAxis);
        }
    }
}
