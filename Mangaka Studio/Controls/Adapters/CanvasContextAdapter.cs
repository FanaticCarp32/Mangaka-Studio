using Mangaka_Studio.Controls.Tools;
using Mangaka_Studio.Interfaces;
using Mangaka_Studio.Models;
using Mangaka_Studio.ViewModels;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Mangaka_Studio.Controls.Adapters
{
    public class CanvasContextAdapter : ICanvasContext
    {
        private readonly CanvasViewModel canvas;
        public CanvasContextAdapter(CanvasViewModel vm) => canvas = vm;
        
        public float Pressure
        {
            get => canvas.Pressure;
            set => canvas.Pressure = value;
        }

        public bool IsTextPosSnap
        {
            get => canvas.IsTextPosSnap;
            set => canvas.IsTextPosSnap = value;
        }

        public double Scale
        {
            get => canvas.Scale;
            set => canvas.Scale = value;
        }

        public TextBox TextEditor
        {
            get => canvas.TextEditor;
            set => canvas.TextEditor = value;
        }

        public int CanvasWidth
        {
            get => canvas.CanvasWidth;
            set => canvas.CanvasWidth = value;
        }

        public int CanvasHeight
        {
            get => canvas.CanvasHeight;
            set => canvas.CanvasHeight = value;
        }

        public SKColor ColorPipette
        {
            get => canvas.ColorPipette;
            set => canvas.ColorPipette = value;
        }

        public bool IsTextEditor 
        {
            get => canvas.IsTextEditor;
            set => canvas.IsTextEditor = value;
        }

        public SKPoint? LastErasePoint
        {
            get => canvas.LastErasePoint;
            set => canvas.LastErasePoint = value;
        }

        public SKPoint? EraserCursor
        {
            get => canvas.EraserCursor;
            set => canvas.EraserCursor = value;
        }

        public double Rotate
        {
            get => canvas.Rotate;
            set => canvas.Rotate = value;
        }

        public SKPoint ScalePos
        {
            get => canvas.ScalePos;
            set => canvas.ScalePos = value;
        }

        public DrawingTools CurrentTool
        {
            get => canvas.CurrentTool;
            set => canvas.CurrentTool = value;
        }

        public bool IsGrid
        {
            get => canvas.IsGrid;
            set => canvas.IsGrid = value;
        }

        public int GridSize
        {
            get => canvas.GridSize;
            set => canvas.GridSize = value;
        }

        public Point CursorPoint
        {
            get => canvas.CursorPoint;
            set => canvas.CursorPoint = value;
        }

        public double OffsetX
        {
            get => canvas.OffsetX;
            set => canvas.OffsetX = value;
        }
        public double OffsetY
        {
            get => canvas.OffsetY;
            set => canvas.OffsetY = value;
        }

        public void NotifyEraserCursorChanged()
        {
            canvas.OnPropertyChanged(nameof(EraserCursor));
        }

        public void ShowTextEditor(TextModel text)
        {
            var bounds = text.GetBounds();
            var canvasPoint = new SKPoint(bounds.Left, bounds.Top);
            var screenPoint = canvas.GetScreenPoint(canvasPoint);
            Canvas.SetLeft(TextEditor, screenPoint.X);
            Canvas.SetTop(TextEditor, screenPoint.Y);

            TextEditor.FontStyle = text.SlantEnum switch
            {
                SKFontStyleSlant.Italic => FontStyles.Italic,
                SKFontStyleSlant.Oblique => FontStyles.Oblique,
                _ => FontStyles.Normal
            };
            TextEditor.FontWeight = FontWeight.FromOpenTypeWeight(text.FontStyleWeight);
            TextEditor.FontStretch = text.FontStyleWidth switch
            {
                <= 3 => FontStretches.Condensed,
                >= 7 => FontStretches.Expanded,
                _ => FontStretches.Normal
            };
            TextEditor.RenderTransformOrigin = new Point(0.5, 0.5);
            TextEditor.RenderTransform = new RotateTransform(text.Rotate);
            TextEditor.FontFamily = new FontFamily(text.FontFamily);
            TextEditor.FontSize = text.FontSize * Scale;
            TextEditor.Width = bounds.Width * Scale;
            TextEditor.Height = bounds.Height * Scale;
            if (!IsTextEditor)
            {
                IsTextEditor = true;
                TextEditor.Visibility = Visibility.Visible;
                TextEditor.Focus();
            }
        }

        public Point GetScreenPoint(SKPoint canvasPoint)
        {
            return canvas.GetScreenPoint(canvasPoint);
        }

        public event PropertyChangedEventHandler PropertyChanged
        {
            add => canvas.PropertyChanged += value;
            remove => canvas.PropertyChanged -= value;
        }
    }

}
