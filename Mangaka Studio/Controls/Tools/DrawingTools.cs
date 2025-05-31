using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Mangaka_Studio.ViewModels;
using SkiaSharp;
using System.Text.Json.Serialization;
using Mangaka_Studio.Interfaces;

namespace Mangaka_Studio.Controls.Tools
{
    public abstract class DrawingTools
    {
        public virtual ToolsSettingsViewModel Settings { get; set; }
        public abstract void OnMouseDown(ICanvasContext canvasViewModel, SKPoint pos, IColorPickerContext colorPickerViewModel, IFrameContext frameViewModel, ITextTemplatesContext textTemplatesViewModel);
        public abstract void OnMouseUp(ICanvasContext canvasViewModel, IFrameContext frameViewModel);
        public abstract void OnMouseMove(ICanvasContext canvasViewModel, SKPoint pos, IColorPickerContext colorPickerViewModel, IFrameContext frameViewModel, ITextTemplatesContext textTemplatesViewModel);
    }
}
