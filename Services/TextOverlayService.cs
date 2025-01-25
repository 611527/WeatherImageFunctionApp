using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace WeatherImageFunctionApp.Services
{
    public class TextOverlayService
    {
        public Stream ApplyTextOverlay(Bitmap bitmap, string text)
        {
            using (var graphics = Graphics.FromImage(bitmap))
            {
                var font = new Font("Arial", 24, FontStyle.Bold);
                var textColor = Brushes.White;
                var textBackground = Brushes.Black;
                var textLocation = new PointF(10, 10);

                var textSize = graphics.MeasureString(text, font);
                graphics.FillRectangle(textBackground, textLocation.X - 5, textLocation.Y - 5, textSize.Width + 10, textSize.Height + 10);
                graphics.DrawString(text, font, textColor, textLocation);
            }

            var outputStream = new MemoryStream();
            bitmap.Save(outputStream, ImageFormat.Png);
            outputStream.Position = 0;
            return outputStream;
        }
    }
}
