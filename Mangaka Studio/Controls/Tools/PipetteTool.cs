using Mangaka_Studio.Interfaces;
using Mangaka_Studio.ViewModels;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mangaka_Studio.Controls.Tools
{
    public class PipetteTool : PenTool
    {
        public PipetteTool(ToolsSettingsViewModel penSettings) : base(penSettings)
        {
        }

        public override void OnMouseDown(ICanvasContext canvasViewModel, SKPoint pos, IColorPickerContext colorPickerViewModel, IFrameContext frameViewModel, ITextTemplatesContext textTemplatesViewModel)
        {
            ApplyPipette(canvasViewModel, pos, colorPickerViewModel, frameViewModel);
            colorPickerViewModel.SwitchColorPipette.Execute(canvasViewModel.ColorPipette);
        }

        public override void OnMouseMove(ICanvasContext canvasViewModel, SKPoint pos, IColorPickerContext colorPickerViewModel, IFrameContext frameViewModel, ITextTemplatesContext textTemplatesViewModel)
        {
            ApplyPipette(canvasViewModel, pos, colorPickerViewModel, frameViewModel);
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

        private void ApplyPipette(ICanvasContext canvasViewModel, SKPoint pos, IColorPickerContext colorPickerViewModel, IFrameContext frameViewModel)
        {
            if (frameViewModel.SelectFrame.LayerVM.Screenshot == null || pos.X < 0 || pos.X > canvasViewModel.CanvasWidth || pos.Y < 0 || pos.Y > canvasViewModel.CanvasHeight)
            {
                canvasViewModel.ColorPipette = SKColors.Transparent;
                return;
            }
            canvasViewModel.ColorPipette = GetColorPixels(frameViewModel.SelectFrame.LayerVM.Screenshot, pos);
        }
    }
}
