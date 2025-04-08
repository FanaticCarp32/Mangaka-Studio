using Mangaka_Studio.ViewModels;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Mangaka_Studio.Models
{
    class HardEraser : EraserTool
    {
        public override ToolsSettingsViewModel Settings { get; }
        private bool isErasing = false;

        public HardEraser(ToolsSettingsViewModel settings)
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
            var eraseHardSettings = Settings as EraserHardToolSettingsViewModel;
            using (var paint = new SKPaint
            {
                Color = SKColors.Transparent, // Прозрачность
                StrokeWidth = Settings.StrokeWidth,
                BlendMode = SKBlendMode.Clear, // Стирание альфа-композитингом
                Style = SKPaintStyle.Stroke,
                StrokeCap = SKStrokeCap.Round,
                IsAntialias = eraseHardSettings.IsAntialias
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
