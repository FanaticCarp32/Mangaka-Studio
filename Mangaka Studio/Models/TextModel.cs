using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Net.Mime.MediaTypeNames;

namespace Mangaka_Studio.Models
{
    public class TextModel
    {
        public string Text { get; set; } = "";
        public SKPoint Position { get; set; }
        public float Rotate { get; set; } = 0f;
        public float FontSize { get; set; } = 18;
        public SKColor Color { get; set; } = SKColors.Black;
        public SKColor ColorStroke { get; set; } = SKColors.White;
        public bool IsStroke { get; set; } = false;
        public float StrokeWidth { get; set; } = 5;
        public string FontFamily { get; set; } = "Segoe UI";
        public int FontStyleWeight { get; set; } = 400;
        public int FontStyleWidth { get; set; } = 5;
        public int FontStyleSlant { get; set; } = 0;
        [JsonIgnore]
        public SKFontStyleSlant SlantEnum => (SKFontStyleSlant)FontStyleSlant;
        public bool IsSelected { get; set; } = true;

        public SKRect GetBounds()
        {
            using var font = new SKFont(SKTypeface.FromFamilyName(FontFamily, new SKFontStyle(FontStyleWeight, FontStyleWidth, SlantEnum)), FontSize);
            SKRect bounds = new();
            using var paint = new SKPaint
            {
                Color = Color,
                IsAntialias = true
            };
            var lines = Text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            float maxWidth = 0;
            float totalHeight = 0;

            var metrics = font.Metrics;
            float lineHeight = metrics.Descent - metrics.Ascent + metrics.Leading;

            foreach (var line in lines)
            {
                var lineWidth = font.MeasureText(line, paint);
                if (lineWidth > maxWidth)
                    maxWidth = lineWidth;

                totalHeight += lineHeight;
            }

            var left = Position.X;
            var top = Position.Y;

            var right = left + maxWidth;
            var bottom = top + totalHeight;

            float padding = 5f;

            return new SKRect(
                left - padding,
                top - padding,
                right + padding,
                bottom + padding
            );
        }

        public void Draw(SKCanvas canvas, double scale)
        {
            using var font = new SKFont(SKTypeface.FromFamilyName(FontFamily, new SKFontStyle(FontStyleWeight, FontStyleWidth, SlantEnum)), FontSize);
            using var paintFill = new SKPaint { Style = SKPaintStyle.Fill, Color = Color, IsAntialias = true };
            canvas.Save();
            canvas.Translate(Position);
            var bounds = GetBounds();
            bounds.Offset(new SKPoint(-Position.X, -Position.Y));
            canvas.RotateDegrees(Rotate, bounds.MidX, bounds.MidY);

            var metrics = font.Metrics;
            float lineHeight = metrics.Descent - metrics.Ascent + metrics.Leading;

            float y = 0;
            var lines = Text.Split(["\r\n", "\n"], StringSplitOptions.None);
            if (!IsStroke)
            {
                foreach (var line in lines)
                {
                    var point = new SKPoint(0, -metrics.Ascent + y);
                    canvas.DrawText(line, point, font, paintFill);
                    y += lineHeight;
                }
            }
            else
            {
                using var paintStroke = new SKPaint { StrokeWidth = StrokeWidth, Style = SKPaintStyle.Stroke, Color = ColorStroke, IsAntialias = true };
                foreach (var line in lines)
                {
                    var point = new SKPoint(0, -metrics.Ascent + y);
                    canvas.DrawText(line, point, font, paintStroke);
                    canvas.DrawText(line, point, font, paintFill);
                    y += lineHeight;
                }
            }

            if (IsSelected)
            {
                using var paintBound = new SKPaint { Color = SKColors.Blue, Style = SKPaintStyle.Stroke, StrokeWidth = 1 / (float)scale, IsAntialias = true };
                canvas.DrawRect(bounds, paintBound);
                var point = new SKPoint(bounds.Left + (bounds.Right - bounds.Left) / 2, bounds.Top);
                canvas.DrawLine(point, new SKPoint(point.X, point.Y - 10 / (float)scale), paintBound);
                canvas.DrawCircle(PointCircle(bounds, scale), 10 / (float)scale, paintBound);
            }
            canvas.Restore();
        }

        public void DrawCurrentLayer(SKCanvas canvas, double scale)
        {
            using var font = new SKFont(SKTypeface.FromFamilyName(FontFamily, new SKFontStyle(FontStyleWeight, FontStyleWidth, SlantEnum)), FontSize);
            using var paintFill = new SKPaint { Style = SKPaintStyle.Fill, Color = Color, IsAntialias = true };
            canvas.Save();
            canvas.Translate(Position);
            var bounds = GetBounds();
            bounds.Offset(new SKPoint(-Position.X, -Position.Y));
            canvas.RotateDegrees(Rotate, bounds.MidX, bounds.MidY);
            var metrics = font.Metrics;
            float lineHeight = metrics.Descent - metrics.Ascent + metrics.Leading;

            float y = 0;
            var lines = Text.Split(["\r\n", "\n"], StringSplitOptions.None);
            if (!IsStroke)
            {
                foreach (var line in lines)
                {
                    var point = new SKPoint(0, -metrics.Ascent + y);
                    canvas.DrawText(line, point, font, paintFill);
                    y += lineHeight;
                }
            }
            else
            {
                using var paintStroke = new SKPaint { StrokeWidth = StrokeWidth, Style = SKPaintStyle.Stroke, Color = ColorStroke, IsAntialias = true };
                foreach (var line in lines)
                {
                    var point = new SKPoint(0, -metrics.Ascent + y);
                    canvas.DrawText(line, point, font, paintStroke);
                    canvas.DrawText(line, point, font, paintFill);
                    y += lineHeight;
                }
            }
            if (IsSelected)
            {
                using var paintBound = new SKPaint { Color = SKColors.Blue, Style = SKPaintStyle.Stroke, StrokeWidth = 1 / (float)scale, IsAntialias = true };
                canvas.DrawRect(bounds, paintBound);
                var point = new SKPoint(bounds.Left + (bounds.Right - bounds.Left) / 2, bounds.Top);
                canvas.DrawLine(point, new SKPoint(point.X, point.Y - 10 / (float)scale), paintBound);
                canvas.DrawCircle(PointCircle(bounds, scale), 10 / (float)scale, paintBound);
            }
            else
            {
                using var paintBound = new SKPaint { Color = new SKColor(0, 0, 255, 128), Style = SKPaintStyle.Stroke, StrokeWidth = 1 / (float)scale, IsAntialias = true };
                canvas.DrawRect(bounds, paintBound);
            }
            canvas.Restore();
        }

        public void DrawTextBox(SKCanvas canvas, double scale)
        {
            using var font = new SKFont(SKTypeface.FromFamilyName(FontFamily, new SKFontStyle(FontStyleWeight, FontStyleWidth, SlantEnum)), FontSize);
            using var paintFill = new SKPaint { Style = SKPaintStyle.Fill, Color = SKColors.Transparent, IsAntialias = true };
            canvas.Save();
            canvas.Translate(Position);
            var bounds = GetBounds();
            bounds.Offset(new SKPoint(-Position.X, -Position.Y));
            canvas.RotateDegrees(Rotate, bounds.MidX, bounds.MidY);
            var metrics = font.Metrics;
            float lineHeight = metrics.Descent - metrics.Ascent + metrics.Leading;

            float y = 0;
            var lines = Text.Split(["\r\n", "\n"], StringSplitOptions.None);
            if (!IsStroke)
            {
                foreach (var line in lines)
                {
                    var point = new SKPoint(0, -metrics.Ascent + y);
                    canvas.DrawText(line, point, font, paintFill);
                    y += lineHeight;
                }
            }
            else
            {
                using var paintStroke = new SKPaint { StrokeWidth = StrokeWidth, Style = SKPaintStyle.Stroke, Color = SKColors.Transparent, IsAntialias = true };
                foreach (var line in lines)
                {
                    var point = new SKPoint(0, -metrics.Ascent + y);
                    canvas.DrawText(line, point, font, paintStroke);
                    canvas.DrawText(line, point, font, paintFill);
                    y += lineHeight;
                }
            }
            if (IsSelected)
            {
                using var paintBound = new SKPaint { Color = SKColors.Blue, Style = SKPaintStyle.Stroke, StrokeWidth = 1 / (float)scale, IsAntialias = true };
                canvas.DrawRect(bounds, paintBound);
                var point = new SKPoint(bounds.Left + (bounds.Right - bounds.Left) / 2, bounds.Top);
                canvas.DrawLine(point, new SKPoint(point.X, point.Y - 10 / (float)scale), paintBound);
                canvas.DrawCircle(PointCircle(bounds, scale), 10 / (float)scale, paintBound);
            }
            canvas.Restore();
        }

        public SKPoint PointCircle(SKRect bounds, double scale)
        {
            var point = new SKPoint(bounds.Left + (bounds.Right - bounds.Left) / 2, bounds.Top);
            return new SKPoint(point.X, point.Y - 10 / (float)scale * 2);
        }

        public SKRect RotateRect(SKRect rect, float angleDegrees)
        {
            var center = new SKPoint(rect.MidX, rect.MidY);
            var angleRad = angleDegrees * (MathF.PI / 180f);

            // Вращаем 4 угла
            SKPoint[] corners =
            {
                RotatePoint(new SKPoint(rect.Left, rect.Top), center, angleRad),
                RotatePoint(new SKPoint(rect.Right, rect.Top), center, angleRad),
                RotatePoint(new SKPoint(rect.Right, rect.Bottom), center, angleRad),
                RotatePoint(new SKPoint(rect.Left, rect.Bottom), center, angleRad)
            };

            // Находим охватывающий прямоугольник
            float minX = corners.Min(p => p.X);
            float minY = corners.Min(p => p.Y);
            float maxX = corners.Max(p => p.X);
            float maxY = corners.Max(p => p.Y);

            return new SKRect(minX, minY, maxX, maxY);
        }

        private static SKPoint RotatePoint(SKPoint point, SKPoint center, float angle)
        {
            float cos = MathF.Cos(angle);
            float sin = MathF.Sin(angle);
            float dx = point.X - center.X;
            float dy = point.Y - center.Y;
            return new SKPoint(
                center.X + dx * cos - dy * sin,
                center.Y + dx * sin + dy * cos
            );
        }
    }
}
