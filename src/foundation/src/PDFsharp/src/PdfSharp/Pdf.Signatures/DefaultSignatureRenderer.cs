using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfSharp.Pdf.Signatures
{
    internal class DefaultSignatureRenderer : ISignatureRenderer
    {
        public void Render(XGraphics gfx, XRect rect, PdfSignatureOptions options)
        {
            // if an image was provided, render only that
            if (options.Image != null)
            {
                gfx.DrawImage(options.Image, 0, 0, rect.Width, rect.Height);
                return;
            }

            var sb = new StringBuilder();
            if (options.Signer != null)
            {
                sb.AppendFormat("Signed by {0}\n", options.Signer);
            }    
            if (options.Location != null)
            {
                sb.AppendFormat("Location: {0}\n", options.Location);
            }
            if (options.Reason != null)
            {
                sb.AppendFormat("Reason: {0}\n", options.Reason);
            }
            sb.AppendFormat(CultureInfo.CurrentCulture, "Date: {0}", DateTime.Now);

            XFont font = new XFont("Verdana", 7, XFontStyleEx.Regular);

            XTextFormatter txtFormat = new XTextFormatter(gfx);

            txtFormat.DrawString(sb.ToString(),
                font,
                new XSolidBrush(XColor.FromKnownColor(XKnownColor.Black)),
                new XRect(0, 0, rect.Width, rect.Height),
                XStringFormats.TopLeft);
        }
    }
}
