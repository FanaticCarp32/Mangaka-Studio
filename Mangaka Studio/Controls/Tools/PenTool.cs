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
using System.Text.Json.Serialization;
using System.Diagnostics;
using Mangaka_Studio.Interfaces;

namespace Mangaka_Studio.Controls.Tools
{
    public class PenTool : DrawingTools
    {
        public override ToolsSettingsViewModel Settings { get; set; }

        internal bool _isDrawing = false;
        internal Queue<SKPoint> pointBuffer = new Queue<SKPoint>();

        [JsonConstructor]
        public PenTool(ToolsSettingsViewModel penSettings)
        {
            Settings = penSettings;
        }

        public override void OnMouseDown(ICanvasContext canvasViewModel, SKPoint pos, IColorPickerContext colorPickerViewModel, IFrameContext frameViewModel, ITextTemplatesContext textTemplatesViewModel)
        {
            _isDrawing = true;
            frameViewModel.SelectFrame.LayerVM.IsModified = true;
            pointBuffer.Enqueue(pos);
            ApplyPen(canvasViewModel, pos, colorPickerViewModel, frameViewModel);
        }

        public override void OnMouseMove(ICanvasContext canvasViewModel, SKPoint pos, IColorPickerContext colorPickerViewModel, IFrameContext frameViewModel, ITextTemplatesContext textTemplatesViewModel)
        {
            if (_isDrawing)
            {
                ApplyPen(canvasViewModel, pos, colorPickerViewModel, frameViewModel);
            }
        }

        public override void OnMouseUp(ICanvasContext canvasViewModel, IFrameContext frameViewModel)
        {
            if (_isDrawing)
            {
                frameViewModel.SelectFrame.LayerVM.baseSurface.Canvas.DrawSurface(frameViewModel.SelectFrame.LayerVM.tempSurface, 0, 0);
                frameViewModel.SelectFrame.LayerVM.tempSurface.Canvas.Clear();
                frameViewModel.SelectFrame.LayerVM.SelectLayer.Image = frameViewModel.SelectFrame.LayerVM.baseSurface.Snapshot();
                canvasViewModel.LastErasePoint = null;
                pointBuffer.Clear();
                _isDrawing = false;
                canvasViewModel.EraserCursor = null;
            }
        }

        private void ApplyPen(ICanvasContext canvasViewModel, SKPoint pos, IColorPickerContext colorPickerViewModel, IFrameContext frameViewModel)
        {
            if (frameViewModel.SelectFrame.LayerVM.SelectLayer == null || !frameViewModel.SelectFrame.LayerVM.SelectLayer.IsVisible) return;

            var penSettings = Settings as PenToolSettingsViewModel;
            var trans = penSettings.Transparent * 255 / 100;
            var color = penSettings.StrokeColor;
            var color1 = penSettings.StrokeColor1;

            using (var paint = new SKPaint
            {
                Color = new SKColor(color.Red, color.Green, color.Blue, (byte)trans),
                StrokeWidth = Math.Max(penSettings.StrokeWidth * canvasViewModel.Pressure, 1f),
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
                AddBezierSmoothedPoint(pos, frameViewModel.SelectFrame.LayerVM.tempSurface.Canvas, paint);
            }
            canvasViewModel.LastErasePoint = pos;
            canvasViewModel.EraserCursor = pos;
        }

        internal void AddBezierSmoothedPoint(SKPoint point, SKCanvas canvas, SKPaint paint)
        {
            pointBuffer.Enqueue(point);
            if (pointBuffer.Count < 4)
            {
                canvas.DrawPoint(point, paint);
                return;
            }
            var pts = pointBuffer.ToArray();
            var p0 = pts[0];
            var p1 = pts[1];
            var p2 = pts[2];
            var p3 = pts[3];

            var cp1 = new SKPoint((p0.X + p1.X) / 2, (p0.Y + p1.Y) / 2);
            var cp2 = new SKPoint((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
            var cp3 = new SKPoint((p2.X + p3.X) / 2, (p2.Y + p3.Y) / 2);
            using var path = new SKPath();
            path.MoveTo(cp1);
            path.CubicTo(p1, p2, cp3);
            canvas.DrawPath(path, paint);
            pointBuffer.Dequeue();
        }
    }
}
