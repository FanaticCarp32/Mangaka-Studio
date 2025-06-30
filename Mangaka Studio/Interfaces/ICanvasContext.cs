using Mangaka_Studio.Controls.Tools;
using Mangaka_Studio.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace Mangaka_Studio.Interfaces
{
    public interface ICanvasContext
    {
        float Pressure { get; set; }
        bool IsTextPosSnap { get; set; }
        double Scale { get; set; }
        TextBox TextEditor { get; set; }
        bool IsTextEditor { get; set; }
        int CanvasWidth { get; set; }
        int CanvasHeight { get; set; }
        double Rotate { get; set; }
        SKPoint ScalePos { get; set; }
        double OffsetX { get; set; }
        double OffsetY { get; set; }
        DrawingTools CurrentTool { get; set; }
        bool IsGrid { get; set; }
        int GridSize { get; set; }
        Point CursorPoint { get; set; }
        SKColor ColorPipette { get; set; }
        SKPoint? LastErasePoint { get; set; }
        SKPoint? EraserCursor { get; set; }
        void NotifyEraserCursorChanged();
        void ShowTextEditor(TextModel text);
        Point GetScreenPoint(SKPoint canvasPoint, Visual visualForDpi);
        event PropertyChangedEventHandler PropertyChanged;
    }

}
