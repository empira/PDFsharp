using MigraDoc.DocumentObjectModel;

namespace MigraDoc.Tests.Helper
{
    static class TestHelper
    {
        // Draws vertical lines at the desired positions by add a table with borders at that positions.
        public static void DrawHorizontalPosition(Section section, params Unit[] positions)
        {
            var table = section.AddTable();
            table.LeftPadding = Unit.Zero;
            table.Borders.Left.Width = Unit.FromPoint(1);
            table.Borders.Right.Width = Unit.FromPoint(1);


            var currentPosition = Unit.Zero;

            foreach (var position in positions.OrderBy(x => x.Millimeter))
            {
                var columnWidth = position - currentPosition;
                table.AddColumn(columnWidth);

                currentPosition += columnWidth;
            }
            
            table.AddRow();
        }
    }
}
