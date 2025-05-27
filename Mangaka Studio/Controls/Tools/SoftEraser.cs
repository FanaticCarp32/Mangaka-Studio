using Mangaka_Studio.Interfaces;
using Mangaka_Studio.ViewModels;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Mangaka_Studio.Controls.Tools
{
    public class SoftEraser : EraserTool
    {
        public override ToolsSettingsViewModel Settings { get; set; }
        private bool isErasing = false;
        private Queue<SKPoint> pointBuffer = new Queue<SKPoint>();

        public SoftEraser(ToolsSettingsViewModel settings)
        {
            Settings = settings;
        }

        public override void OnMouseDown(ICanvasContext canvasViewModel, SKPoint pos, IColorPickerContext colorPickerViewModel, IFrameContext frameViewModel, ITextTemplatesContext textTemplatesViewModel)
        {
            frameViewModel.SelectFrame.LayerVM.IsModified = true;
            //layerViewModel.tempSurface.Canvas.DrawSurface(layerViewModel.baseSurface, 0, 0);
            //layerViewModel.baseSurface.Canvas.Clear();
            pointBuffer.Enqueue(pos);
            isErasing = true;
            canvasViewModel.LastErasePoint = pos;
            ApplyEraser(canvasViewModel, pos, frameViewModel);
        }

        public override void OnMouseMove(ICanvasContext canvasViewModel, SKPoint pos, IColorPickerContext colorPickerViewModel, IFrameContext frameViewModel, ITextTemplatesContext textTemplatesViewModel)
        {
            if (isErasing)
            {
                ApplyEraser(canvasViewModel, pos, frameViewModel);
                canvasViewModel.LastErasePoint = pos;
            }
        }

        public override void OnMouseUp(ICanvasContext canvasViewModel, IFrameContext frameViewModel)
        {
            if (isErasing)
            {
                isErasing = false;
                frameViewModel.SelectFrame.LayerVM.baseSurface.Canvas.DrawSurface(frameViewModel.SelectFrame.LayerVM.tempSurface, 0, 0);
                frameViewModel.SelectFrame.LayerVM.tempSurface.Canvas.Clear();
                frameViewModel.SelectFrame.LayerVM.SelectLayer.Image = frameViewModel.SelectFrame.LayerVM.baseSurface.Snapshot();
                pointBuffer.Clear();
                canvasViewModel.LastErasePoint = null;
                canvasViewModel.EraserCursor = null;
            }
        }

        public override void ApplyEraser(ICanvasContext canvasViewModel, SKPoint erasePoint, IFrameContext frameViewModel)
        {
            if (frameViewModel.SelectFrame.LayerVM.SelectLayer == null || !frameViewModel.SelectFrame.LayerVM.SelectLayer.IsVisible)
                return;
            var eraseSettings = Settings as EraserSoftToolSettingsViewModel;
            using (var paint = new SKPaint
            {
                Color = new SKColor(0, 0, 0, eraseSettings.Transparent), // Прозрачность
                StrokeWidth = Math.Max(eraseSettings.StrokeWidth * canvasViewModel.Pressure, 1f),
                BlendMode = SKBlendMode.DstOut, // Стирание альфа-композитингом
                Style = SKPaintStyle.Stroke,
                StrokeCap = SKStrokeCap.Round,
                IsAntialias = eraseSettings.IsAntialias,
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, Settings.StrokeWidth * eraseSettings.BlurRad * 0.1f),
                //Shader = SKShader.CreateRadialGradient(erasePoint, Settings.StrokeWidth / 2,
                //    new[] { SKColors.Transparent, SKColors.Black }, null, SKShaderTileMode.Repeat)
            })
            {
                AddBezierSmoothedPoint(erasePoint, frameViewModel.SelectFrame.LayerVM.baseSurface.Canvas, paint);
            }

            canvasViewModel.EraserCursor = erasePoint;
        }

        private void AddBezierSmoothedPoint(SKPoint point, SKCanvas canvas, SKPaint paint)
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
