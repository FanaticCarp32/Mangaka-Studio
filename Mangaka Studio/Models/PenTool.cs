using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Input;
using SkiaSharp;
using System.Windows.Ink;
using SkiaSharp.Views.WPF;
using Mangaka_Studio.ViewModels;
using System.Windows.Documents;

namespace Mangaka_Studio.Models
{
    class PenTool : DrawingTools
    {
        public override ToolsSettingsViewModel Settings { get; }

        private bool _isDrawing = false;
        private SKSurface surface;

        public PenTool(ToolsSettingsViewModel penSettings)
        {
            Settings = penSettings;
        }

        public override void OnMouseDown(CanvasViewModel canvasViewModel, SKPoint pos, ColorPickerViewModel colorPickerViewModel, LayerViewModel layerViewModel)
        {
            _isDrawing = true;
            surface = SKSurface.Create(layerViewModel.SelectLayer.Image.Info);
            surface.Canvas.Clear(SKColors.Transparent);
            canvasViewModel.EraserCursor = pos;
            ApplyPen(canvasViewModel, pos, colorPickerViewModel, layerViewModel);
        }

        public override void OnMouseMove(CanvasViewModel canvasViewModel, SKPoint pos, ColorPickerViewModel colorPickerViewModel, LayerViewModel layerViewModel)
        {
            if (_isDrawing)
            {
                canvasViewModel.EraserCursor = pos;
                ApplyPen(canvasViewModel, pos, colorPickerViewModel, layerViewModel);
            }
        }

        public override void OnMouseUp(CanvasViewModel canvasViewModel, LayerViewModel layerViewModel)
        {
            if (_isDrawing)
            {
                var surface1 = SKSurface.Create(layerViewModel.SelectLayer.Image.Info);
                surface1.Canvas.DrawImage(layerViewModel.SelectLayer.Image, 0, 0);
                surface1.Canvas.DrawImage(surface.Snapshot(), 0, 0);
                layerViewModel.SelectLayer.Image = surface1.Snapshot();
                surface1.Dispose();
                layerViewModel.tempSurface.Canvas.Clear();
                layerViewModel.NeedsRedraw = true;
                canvasViewModel.EraserCursor = null;
                canvasViewModel.LastErasePoint = null;
                _isDrawing = false;
            }
        }

        private void ApplyPen(CanvasViewModel canvasViewModel, SKPoint pos, ColorPickerViewModel colorPickerViewModel, LayerViewModel layerViewModel)
        {
            if (layerViewModel.SelectLayer == null) return;

            /*var penSettings = Settings as PenToolSettingsViewModel;
            var brushBitmap = CreateBrushBitmap((int)penSettings.StrokeWidth, penSettings.StrokeColor);
            float brushSpacing = penSettings.StrokeWidth * 0.3f; // шаг между отпечатками кисти

            SKPoint last = canvasViewModel.LastErasePoint ?? pos;
            SKPoint current = pos;

            float distance = SKPoint.Distance(last, current);
            int steps = Math.Max(1, (int)(distance / brushSpacing)); // минимум 1 шаг

            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                float x = last.X + (current.X - last.X) * t - brushBitmap.Width / 2f;
                float y = last.Y + (current.Y - last.Y) * t - brushBitmap.Height / 2f;

                layerViewModel.SelectLayer.Surface.Canvas.DrawBitmap(brushBitmap, new SKPoint(x, y));
            }

            canvasViewModel.LastErasePoint = pos;*/

            var penSettings = Settings as PenToolSettingsViewModel;
            var trans = penSettings.Transparent * 255 / 100;
            var color = penSettings.StrokeColor;
            var color1 = penSettings.StrokeColor1;

            using (var paint = new SKPaint
            {
                Color = new SKColor(color.Red, color.Green, color.Blue, (byte)trans),
                StrokeWidth = penSettings.StrokeWidth,
                Style = SKPaintStyle.Stroke,
                StrokeCap = SKStrokeCap.Round,
                IsAntialias = penSettings.IsSmooth,
                BlendMode = SKBlendMode.Src
            })
            {
                if (!colorPickerViewModel.SwitchColor)
                {
                    paint.Color = new SKColor(color1.Red, color1.Green, color1.Blue, (byte)trans);
                }
                if (canvasViewModel.LastErasePoint.HasValue)
                {
                    surface.Canvas.DrawLine(canvasViewModel.LastErasePoint.Value, pos, paint);
                }
                else
                {
                    surface.Canvas.DrawPoint(pos, paint);
                }
            }
            layerViewModel.tempSurface = surface;
            canvasViewModel.LastErasePoint = pos;
            //layerViewModel.SelectLayer.Surface.Canvas.Flush();
        }

        private SKBitmap CreateBrushBitmap(int size, SKColor baseColor)
        {
            var bitmap = new SKBitmap(size, size);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.Transparent);

            var center = new SKPoint(size / 2f, size / 2f);
            var radius = size / 2f;

            using var paint = new SKPaint
            {
                IsAntialias = true,
                Color = new SKColor(baseColor.Red, baseColor.Green, baseColor.Blue, baseColor.Alpha),
                BlendMode = SKBlendMode.Src
                //Shader = SKShader.CreateRadialGradient(
                //    center,
                //    radius,
                //    new[] {
                //new SKColor(baseColor.Red, baseColor.Green, baseColor.Blue, baseColor.Alpha),
                //new SKColor(baseColor.Red, baseColor.Green, baseColor.Blue, 0)
                //    },
                //    new float[] { 0f, 1f },
                //    SKShaderTileMode.Clamp
                //)
            };

            canvas.DrawCircle(center, radius, paint);
            return bitmap;
        }
    }
}
