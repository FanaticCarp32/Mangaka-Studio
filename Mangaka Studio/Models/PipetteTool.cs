using Mangaka_Studio.ViewModels;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mangaka_Studio.Models
{
    class PipetteTool : PenTool
    {
        public PipetteTool(ToolsSettingsViewModel penSettings) : base(penSettings)
        {
        }

        public override void OnMouseDown(CanvasViewModel canvasViewModel, SKPoint pos, ColorPickerViewModel colorPickerViewModel, LayerViewModel layerViewModel)
        {
            ApplyPipette(canvasViewModel, pos, colorPickerViewModel, layerViewModel);
            colorPickerViewModel.SwitchColorPipette.Execute(canvasViewModel.ColorPipette);
        }

        public override void OnMouseMove(CanvasViewModel canvasViewModel, SKPoint pos, ColorPickerViewModel colorPickerViewModel, LayerViewModel layerViewModel)
        {
            ApplyPipette(canvasViewModel, pos, colorPickerViewModel, layerViewModel);
        }

        public SKColor GetColorPixels(SKImage sKImage, SKPoint pos)
        {
            using (var pixels = sKImage.PeekPixels())
            {
                if (pixels != null)
                {
                    return pixels.GetPixelColor((int)pos.X, (int)pos.Y);
                }
            }
            return SKColors.Transparent;
        }

        private void ApplyPipette(CanvasViewModel canvasViewModel, SKPoint pos, ColorPickerViewModel colorPickerViewModel, LayerViewModel layerViewModel)
        {
            if (layerViewModel.Screenshot == null || pos.X < 0 || pos.X > canvasViewModel.CanvasWidth || pos.Y < 0 || pos.Y > canvasViewModel.CanvasHeight)
            {
                canvasViewModel.ColorPipette = SKColors.Transparent;
                return;
            }
            canvasViewModel.ColorPipette = GetColorPixels(layerViewModel.Screenshot, pos);
        }
    }
}
