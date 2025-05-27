using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.IO;
using System.Windows;

namespace Mangaka_Studio.Models
{
    public class TemplateModel
    {
        private SKBitmap bitmap;
        //public float Rotate { get; set; } = 0f;
        public SKRect Bounds { get; set; }
        public bool IsSelected { get; set; } = false;

        private string path;
        public string Path
        {
            get => path;
            set
            {
                if (path != value)
                {
                    path = value;
                    LoadBitmap(path);
                }
            }
        }

        private void LoadBitmap(string path)
        {
            bitmap?.Dispose();
            bitmap = null;

            if (string.IsNullOrEmpty(path))
                return;

            try
            {
                var uri = new Uri(path, UriKind.RelativeOrAbsolute);
                Stream stream = null;

                if (uri.IsAbsoluteUri && uri.Scheme == "pack")
                {
                    var resourceStream = Application.GetResourceStream(uri);
                    if (resourceStream == null) return;
                    stream = resourceStream.Stream;
                }
                else if (File.Exists(path))
                {
                    stream = File.OpenRead(path);
                }
                else
                {
                    return;
                }

                using (stream)
                using (var managedStream = new SKManagedStream(stream))
                {
                    bitmap = SKBitmap.Decode(managedStream);
                }
            }
            catch
            {
                // Обработка ошибок загрузки
                bitmap = null;
            }
        }


        public void Draw(SKCanvas canvas, double scale)
        {
            if (bitmap == null)
                return;

            canvas.Save();

            var rect = Bounds;
            var destRect = new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom);
            canvas.DrawBitmap(bitmap, destRect);
            
            canvas.Restore();
        }
        public void DrawCurrentLayer(SKCanvas canvas, double scale)
        {
            if (bitmap == null)
                return;

            canvas.Save();

            var rect = Bounds;
            var destRect = new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom);
            canvas.DrawBitmap(bitmap, destRect);
            using var paintBound = new SKPaint
            {
                Color = new SKColor(0, 0, 255, 128),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1f / (float)scale,
                IsAntialias = true
            };

            canvas.DrawRect(destRect, paintBound);
            canvas.Restore();
        }
    }
}
