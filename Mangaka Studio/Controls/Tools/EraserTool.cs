using Mangaka_Studio.Interfaces;
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
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Mangaka_Studio.Controls.Tools
{
    public abstract class EraserTool : DrawingTools
    {
        public abstract void ApplyEraser(ICanvasContext canvasViewModel, SKPoint erasePoint, IFrameContext frameViewModel);
    }
}
