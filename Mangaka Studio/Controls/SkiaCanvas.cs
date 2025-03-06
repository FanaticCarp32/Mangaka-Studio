using SkiaSharp.Views.Desktop;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows;
using Mangaka_Studio.ViewModels;

namespace Mangaka_Studio.Controls
{
    class SkiaCanvas : SKElement
    {
        private readonly List<List<SKPoint>> _strokes = new(); // Список всех линий
        private List<SKPoint> _currentStroke = new();
        private bool _isDrawing = false;
        private Point lastPos;
        private CanvasViewModel canvas;

        public SkiaCanvas(CanvasViewModel canvasViewModel)
        {
            canvas = canvasViewModel;
            this.MouseDown += OnMouseDown;
            this.MouseMove += OnMouseMove;
            this.MouseUp += OnMouseUp;
            this.PaintSurface += OnPaintSurface;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                //flag = true;
                lastPos = e.GetPosition(this);
                return;
            }
            _isDrawing = true;
            _currentStroke = new List<SKPoint>(); // Новый список для нового штриха
            _currentStroke.Add(e.GetPosition(this).ToSKPoint());
            _strokes.Add(_currentStroke); // Добавляем в общий список
            InvalidateVisual();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                Vector d = e.GetPosition(this) - lastPos;
                canvas.PanCommand.Execute(d);
                lastPos = e.GetPosition(this);
                return;
            }
            if (_isDrawing)
            {
                _currentStroke.Add(e.GetPosition(this).ToSKPoint());
                InvalidateVisual();
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = false;
        }

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.White);

            using (var paint = new SKPaint
            {
                Color = SKColors.Black,
                StrokeWidth = 2,
                Style = SKPaintStyle.Stroke,
                StrokeCap = SKStrokeCap.Round
            })
            {
                foreach (var stroke in _strokes)
                {
                    for (int i = 1; i < stroke.Count; i++)
                    {
                        canvas.DrawLine(stroke[i - 1], stroke[i], paint);
                    }
                }
            }
        }
    }
}
