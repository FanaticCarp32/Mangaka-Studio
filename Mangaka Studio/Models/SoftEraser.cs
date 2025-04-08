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
        public override ToolsSettingsViewModel Settings { get; }
        private bool isErasing = false;

        public SoftEraser(ToolsSettingsViewModel settings)
        {
            Settings = settings;
        }

        public override void OnMouseDown(CanvasViewModel canvasViewModel, SKPoint pos, ColorPickerViewModel colorPickerViewModel, LayerViewModel layerViewModel)
        {
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
            isErasing = false;
            canvasViewModel.EraserCursor = null;
            canvasViewModel.LastErasePoint = null;
            canvasViewModel.OnPropertyChanged(nameof(canvasViewModel.EraserCursor));
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
                    layerViewModel.SelectLayer.Surface.Canvas.DrawLine(canvasViewModel.LastErasePoint.Value, erasePoint, paint);
                }
            }
            //layerViewModel.SelectLayer.Surface.Canvas.Flush();
        }
    }
}
