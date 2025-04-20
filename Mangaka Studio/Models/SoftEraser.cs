using Mangaka_Studio.ViewModels;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Mangaka_Studio.Models
{
    class SoftEraser : EraserTool
    {
        public override ToolsSettingsViewModel Settings { get; set; }
        private bool isErasing = false;
        private SKSurface surface;

        public SoftEraser(ToolsSettingsViewModel settings)
        {
            Settings = settings;
        }

        public override void OnMouseDown(CanvasViewModel canvasViewModel, SKPoint pos, ColorPickerViewModel colorPickerViewModel, LayerViewModel layerViewModel)
        {
            layerViewModel.IsModified = true;
            surface = SKSurface.Create(layerViewModel.SelectLayer.Image.Info);
            surface.Canvas.Clear(SKColors.Transparent);
            surface.Canvas.DrawImage(layerViewModel.SelectLayer.Image, 0, 0);
            layerViewModel.SelectLayer.Image = SKImage.Create(layerViewModel.SelectLayer.Image.Info);
            layerViewModel.NeedsRedraw = true;
            canvasViewModel.EraserCursor = pos;
            isErasing = true;
            canvasViewModel.LastErasePoint = pos;
            ApplyEraser(canvasViewModel, pos, layerViewModel);
        }

        public override void OnMouseMove(CanvasViewModel canvasViewModel, SKPoint pos, ColorPickerViewModel colorPickerViewModel, LayerViewModel layerViewModel)
        {
            if (isErasing)
            {
                ApplyEraser(canvasViewModel, pos, layerViewModel);
                canvasViewModel.EraserCursor = pos;
                canvasViewModel.LastErasePoint = pos;
            }
        }

        public override void OnMouseUp(CanvasViewModel canvasViewModel, LayerViewModel layerViewModel)
        {
            if (isErasing)
            {
                isErasing = false;
                var surface1 = SKSurface.Create(layerViewModel.SelectLayer.Image.Info);
                surface1.Canvas.DrawImage(surface.Snapshot(), 0, 0);
                layerViewModel.SelectLayer.Image = surface1.Snapshot();
                surface1.Dispose();
                layerViewModel.tempSurface.Canvas.Clear();
                layerViewModel.NeedsRedraw = true;
                canvasViewModel.EraserCursor = null;
                canvasViewModel.LastErasePoint = null;
                //canvasViewModel.OnPropertyChanged(nameof(canvasViewModel.EraserCursor));
            }
        }

        public override void ApplyEraser(CanvasViewModel canvasViewModel, SKPoint erasePoint, LayerViewModel layerViewModel)
        {
            if (layerViewModel.SelectLayer == null)
                return;
            var eraseSettings = Settings as EraserSoftToolSettingsViewModel;
            using (var paint = new SKPaint
            {
                Color = new SKColor(0, 0, 0, eraseSettings.Transparent), // Прозрачность
                StrokeWidth = Settings.StrokeWidth,
                BlendMode = SKBlendMode.DstOut, // Стирание альфа-композитингом
                Style = SKPaintStyle.Stroke,
                StrokeCap = SKStrokeCap.Round,
                IsAntialias = eraseSettings.IsAntialias,
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, Settings.StrokeWidth * eraseSettings.BlurRad * 0.1f),
                //Shader = SKShader.CreateRadialGradient(erasePoint, Settings.StrokeWidth / 2,
                //    new[] { SKColors.Transparent, SKColors.Black }, null, SKShaderTileMode.Repeat)
            })
            {
                if (canvasViewModel.LastErasePoint.HasValue)
                {
                    surface.Canvas.DrawLine(canvasViewModel.LastErasePoint.Value, erasePoint, paint);
                }
            }
            layerViewModel.tempSurface = surface;
            //layerViewModel.SelectLayer.Surface.Canvas.Flush();
        }
    }
}
