// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

namespace PdfSharp.Charting.Renderers
{
    /// <summary>
    /// Represents the base class for all Y axis renderer.
    /// </summary>
    abstract class YAxisRenderer : AxisRenderer
    {
        /// <summary>
        /// Initializes a new instance of the YAxisRenderer class with the specified renderer parameters.
        /// </summary>
        internal YAxisRenderer(RendererParameters parms) : base(parms)
        { }

        /// <summary>
        /// Calculates optimal minimum/maximum scale and minor/major tick based on yMin and yMax.
        /// </summary>
        protected void FineTuneYAxis(AxisRendererInfo rendererInfo, double yMin, double yMax)
        {
            if (yMin.Equals(Double.MaxValue) && yMax.Equals(Double.MinValue))
            {
                // No series data given.
                yMin = 0.0f;
                yMax = 0.9f;
            }

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (yMin == yMax)
            {
                if (yMin == 0)
                    yMax = 0.9f;
                else if (yMin < 0)
                    yMax = 0;
                else if (yMin > 0)
                    yMax = yMin + 1;
            }

            // If the ratio between yMax to yMin is more than 1.2, the smallest number will be set too zero.
            // It’s Excel’s behavior.
            if (yMin != 0)
            {
                if (yMin < 0 && yMax < 0)
                {
                    if (yMin / yMax >= 1.2)
                        yMax = 0;
                }
                else if (yMax / yMin >= 1.2)
                    yMin = 0;
            }

            double deltaYRaw = yMax - yMin;

            int digits = (int)(Math.Log(deltaYRaw, 10) + 1);
            double normed = deltaYRaw / Math.Pow(10, digits) * 10;

            double normedStepWidth = 1;
            if (normed < 2)
                normedStepWidth = 0.2f;
            else if (normed < 5)
                normedStepWidth = 0.5f;

            var yari = rendererInfo;
            double stepWidth = normedStepWidth * Math.Pow(10.0, digits - 1.0);
            if (yari.Axis == null || Double.IsNaN(yari.Axis.MajorTick))
                yari.MajorTick = stepWidth;
            else
                yari.MajorTick = yari.Axis.MajorTick;

            double roundFactor = stepWidth * 0.5;
            if (yari.Axis == null || Double.IsNaN(yari.Axis.MinimumScale))
            {
                double signumMin = (yMin != 0) ? yMin / Math.Abs(yMin) : 0;
                yari.MinimumScale = (int)(Math.Abs((yMin - roundFactor) / stepWidth) - (1 * signumMin)) * stepWidth * signumMin;
            }
            else
                yari.MinimumScale = yari.Axis.MinimumScale;

            if (yari.Axis == null || Double.IsNaN(yari.Axis.MaximumScale))
            {
                double signumMax = (yMax != 0) ? yMax / Math.Abs(yMax) : 0;
                yari.MaximumScale = (int)(Math.Abs((yMax + roundFactor) / stepWidth) + (1 * signumMax)) * stepWidth * signumMax;
            }
            else
                yari.MaximumScale = yari.Axis.MaximumScale;

            if (yari.Axis == null || Double.IsNaN(yari.Axis.MinorTick))
                yari.MinorTick = yari.MajorTick / 5;
            else
                yari.MinorTick = yari.Axis.MinorTick;
        }

        /// <summary>
        /// Returns the default tick labels format string.
        /// </summary>
        protected override string GetDefaultTickLabelsFormat() => "0.0";
    }
}
