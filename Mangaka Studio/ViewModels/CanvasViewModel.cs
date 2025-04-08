using Mangaka_Studio.Models;
using Mangaka_Studio.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Mangaka_Studio.Commands;
using System.Windows.Media;
using System.Collections.ObjectModel;
using SkiaSharp;
using Mangaka_Studio.Controls;
using SkiaSharp.Views.WPF;

namespace Mangaka_Studio.ViewModels
{
    public class CanvasViewModel : INotifyPropertyChanged
    {
        private DrawingTools currentTool;
        private double scale = 1.0;
        private double scaleRound = 1.0;
        private double offsetX = 0;
        private double offsetXMax = 1000;
        private double offsetXMin = -500;
        private double offsetY = 0;
        private double offsetYMax = 600;
        private double offsetYMin = -300;
        private bool isScale = false;
        private SKPoint? eraserCursor;
        private MatrixTransform matrixTransform = new MatrixTransform(Matrix.Identity);
        public float CanvasWidth { get; set; } = 1000;
        public float CanvasHeight { get; set; } = 600;
        public float EraserSize { get; set; } = 20f;
        public double ScaleMax { get; set; } = 10.0;
        public double ScaleMin { get; set; } = 0.3;

        private SKSurface surface;
        public SKSurface Surface
        {
            get => surface;
            set
            {
                surface = value;
                OnPropertyChanged(nameof(Surface));
            }
        }

        public SKPoint? EraserCursor
        {
            get => eraserCursor;
            set
            {
                eraserCursor = value;
                OnPropertyChanged(nameof(EraserCursor));
            }
        }

        public MatrixTransform CanvasMatrix
        {
            get => matrixTransform;
            set
            {
                matrixTransform = value;
                OnPropertyChanged(nameof(CanvasMatrix));
            }
        }
        public List<SKPoint> StrokePoints { get; } = new();

        public SKPoint? LastErasePoint { get; set; } = null;
        public double ActualWidth { get; set; } = 0;
        public double ActualHeight { get; set; } = 0;
        public SKColor ColorPipette { get; set; } = SKColors.Transparent;

        public double Scale
        {
            get => scale;
            set
            {
                scale = value;
                OnPropertyChanged(nameof(Scale));
                UpdateScale();
            }
        }

        private SKPoint scalePos = new SKPoint(0,0);
        public SKPoint ScalePos
        {
            get => scalePos;
            set
            {
                scalePos = value;
                OnPropertyChanged(nameof(ScalePos));
            }
        }

        public double ScaleRound
        {
            get => scaleRound;
            set
            {
                scaleRound = Math.Round(value, 3);
                OnPropertyChanged(nameof(ScaleRound));
            }
        }

        public double OffsetX
        {
            get => offsetX;
            set
            {
                offsetX = Math.Round(value);
                OnPropertyChanged(nameof(OffsetX));
            }
        }

        public double OffsetY
        {
            get => offsetY;
            set
            {
                offsetY = Math.Round(value);
                OnPropertyChanged(nameof(OffsetY));
            }
        }

        private double rotate = 0;
        public double Rotate
        {
            get => rotate;
            set
            {
                rotate = value;
                OnPropertyChanged(nameof(Rotate));
            }
        }

        public DrawingTools CurrentTool
        {
            get => currentTool;
            set
            {
                currentTool = value;
                OnPropertyChanged(nameof(CurrentTool));
            }
        }

        private Point cursorPoint;
        public Point CursorPoint
        {
            get => cursorPoint;
            set
            {
                cursorPoint = new Point((int)value.X, (int)value.Y);
                OnPropertyChanged(nameof(CursorPoint));
            }
        }

        public ICommand ZoomCommand { get; }
        public ICommand PanCommand { get; }
        public ICommand ResetCommand { get; }

        public CanvasViewModel()
        {
            ZoomCommand = new RelayCommand(param =>
            {
                isScale = true;
                var tuple = ((double, SKPoint))param;
                double zoomFactor = tuple.Item1;
                SKPoint mousePos = tuple.Item2;
                double newScale = Math.Max(ScaleMin, Math.Min(ScaleMax, Scale * zoomFactor));
                ScalePos = mousePos;
                Scale = newScale;
                ScaleRound = Scale;
                isScale = false;
            });

            PanCommand = new RelayCommand(param =>
            {
                var movement = (Vector)param;
                OffsetX += movement.X;
                OffsetY += movement.Y;
            });

            ResetCommand = new RelayCommand(param =>
            {
                Scale = 1;
                ScalePos = new SKPoint(CanvasWidth / 2, CanvasHeight / 2);
                Rotate = 0;
                OffsetX = 0;
                OffsetY = 0;
            });
        }

        public void OnMouseDown(CanvasViewModel canvasViewModel, SKPoint pos, ColorPickerViewModel colorPickerViewModel, LayerViewModel layerViewModel)
        {
            currentTool?.OnMouseDown(canvasViewModel, pos, colorPickerViewModel, layerViewModel);
        }

        public void OnMouseMove(CanvasViewModel canvasViewModel, SKPoint pos, ColorPickerViewModel colorPickerViewModel, LayerViewModel layerViewModel)
        {
            currentTool?.OnMouseMove(canvasViewModel, pos, colorPickerViewModel, layerViewModel);
        }

        public void OnMouseMoveMouse(SKPoint pos)
        {
            if (pos.X <= CanvasWidth && pos.Y <= CanvasHeight && pos.X >= 0 && pos.Y >= 0)
                CursorPoint = new Point(pos.X, pos.Y);
        }

        public void OnMouseUp(CanvasViewModel canvasViewModel, LayerViewModel layerViewModel)
        {
            currentTool?.OnMouseUp(canvasViewModel, layerViewModel);
        }

        public bool ScaleChanged(double zoomFactor)
        {
            double newScale = Math.Max(ScaleMin, Math.Min(ScaleMax, Scale * zoomFactor));
            if (newScale == ScaleMin || newScale == ScaleMax)
            {
                if (Scale == ScaleMin || Scale == ScaleMax) return false;
            }
            return true;
        }

        public SKPoint GetCanvasPoint(SKPoint screenPoint)
        {
            var matrix = SKMatrix.CreateIdentity();

            matrix = SKMatrix.Concat(matrix, SKMatrix.CreateTranslation(-(float)OffsetX, -(float)OffsetY));
            matrix = SKMatrix.Concat(matrix, SKMatrix.CreateRotationDegrees(-(float)Rotate, CanvasWidth / 2, CanvasHeight / 2));
            matrix = SKMatrix.Concat(matrix, SKMatrix.CreateScale(1f / (float)Scale, 1f / (float)Scale, ScalePos.X, ScalePos.Y));

            return matrix.MapPoint(screenPoint);
        }

        /*public SKPoint GetTransformedCanvasCenter()
        {
            // Центр канваса без смещения
            float originalCenterX = CanvasWidth / 2;
            float originalCenterY = CanvasHeight / 2;
            // Применяем смещение
            double transformedCenterX = originalCenterX + OffsetX;
            double transformedCenterY = originalCenterY + OffsetY;

            return new SKPoint((float)transformedCenterX, (float)transformedCenterY);
        }*/

        private void UpdateScale()
        {
            if (isScale) return;
            ScaleRound = Scale;
            ScalePos = new SKPoint(CanvasWidth / 2, CanvasHeight / 2);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
