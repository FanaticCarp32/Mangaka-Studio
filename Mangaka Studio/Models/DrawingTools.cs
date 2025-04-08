using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Mangaka_Studio.ViewModels;
using SkiaSharp;

namespace Mangaka_Studio.Models
{
    public abstract class DrawingTools
    {
        public virtual ToolsSettingsViewModel Settings { get; }
        public abstract void OnMouseDown(CanvasViewModel canvasViewModel, SKPoint pos, ColorPickerViewModel colorPickerViewModel, LayerViewModel layerViewModel);
        public abstract void OnMouseUp(CanvasViewModel canvasViewModel, LayerViewModel layerViewModel);
        public abstract void OnMouseMove(CanvasViewModel canvasViewModel, SKPoint pos, ColorPickerViewModel colorPickerViewModel, LayerViewModel layerViewModel);
    }
}
