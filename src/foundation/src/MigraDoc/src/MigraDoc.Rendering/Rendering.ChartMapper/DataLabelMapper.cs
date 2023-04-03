// MigraDoc - Creating Documents on the Fly
// See the LICENSE file in the solution root for more information.

using System.Diagnostics;
using MigraDoc.DocumentObjectModel;
using PdfSharp.Charting;

namespace MigraDoc.Rendering.ChartMapper
{
    class DataLabelMapper
    {
        void MapObject(DataLabel dataLabel, DocumentObjectModel.Shapes.Charts.DataLabel domDataLabel)
        {
            if (!domDataLabel.Values.Style.IsValueNullOrEmpty())
            {
                Debug.Assert(domDataLabel.Document != null, "domDataLabel.Document != null");
                FontMapper.Map(dataLabel.Font, domDataLabel.Document, domDataLabel.Style);
            }

            if (!domDataLabel.Values.Font.IsValueNullOrEmpty())
                FontMapper.Map(dataLabel.Font, domDataLabel.Font);
            dataLabel.Format = domDataLabel.Format;
            if (domDataLabel.Values.Position is not null)
                dataLabel.Position = (DataLabelPosition)domDataLabel.Position;
            if (domDataLabel.Values.Type is not null)
                dataLabel.Type = (DataLabelType)domDataLabel.Type;
        }

        internal static void Map(DataLabel dataLabel, DocumentObjectModel.Shapes.Charts.DataLabel domDataLabel)
        {
            var mapper = new DataLabelMapper();
            mapper.MapObject(dataLabel, domDataLabel);
        }
    }
}
