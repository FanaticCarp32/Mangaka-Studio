using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.ObjectModel;
using SkiaSharp;
using Mangaka_Studio.Controls;
using SkiaSharp.Views.WPF;
using Mangaka_Studio.Interfaces;
using Mangaka_Studio.Controls.Tools;

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
        public float EraserSize { get; set; } = 20f;
        public double ScaleMax { get; set; } = 10.0;
        public double ScaleMin { get; set; } = 0.01;
        
        public TextBox TextEditor { get; set; }
        public bool IsTextPosSnap { get; set; } = false;
        private bool isTextEditor = false;
        public bool IsTextEditor
        {
            get => isTextEditor;
            set
            {
                isTextEditor = value;
                OnPropertyChanged(nameof(IsTextEditor));
            }
        }

        private int canvasWidth = 4000;
        public int CanvasWidth
        {
            get => canvasWidth;
            set
            {
                canvasWidth = value;
                OnPropertyChanged(nameof(CanvasWidth));
            }
        }

        private int canvasHeight = 4000;
        public int CanvasHeight
        {
            get => canvasHeight;
            set
            {
                canvasHeight = value;
                OnPropertyChanged(nameof(CanvasHeight));
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

        public bool PipetteChanged { get; set; } = false;
        public SKPoint? LastErasePoint { get; set; } = null;
        private SKColor colorPipette = SKColors.Transparent;
        public SKColor ColorPipette
        {
            get => colorPipette;
            set
            {
                colorPipette = value;
                OnPropertyChanged(nameof(ColorPipette));
            }
        }

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

        public SKPoint ScalePos { get; set; } = new SKPoint(0,0);

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

        private string isGridText = "Показать";
        public string IsGridText
        {
            get => isGridText;
            set
            {
                isGridText = value;
                OnPropertyChanged(nameof(IsGridText));
            }
        }

        private bool isGrid = false;
        public bool IsGrid
        {
            get => isGrid;
            set
            {
                isGrid = value;
                if (isGrid)
                {
                    IsGridText = "Скрыть";
                }
                else
                {
                    IsGridText = "Показать";
                }
                OnPropertyChanged(nameof(IsGrid));
            }
        }

        private int gridSize = 50;
        public int GridSize
        {
            get => gridSize;
            set
            {
                gridSize = value;
                OnPropertyChanged(nameof(GridSize));
            }
        }

        public ObservableCollection<string> BubbleTemplates { get; set; } = new ObservableCollection<string>
        {
            "pack://application:,,,/Resources/Templates/Bubbles/BubbleTransparent1.png",
            "pack://application:,,,/Resources/Templates/Bubbles/BubbleTransparent2.png",
            "pack://application:,,,/Resources/Templates/Bubbles/BubbleTransparent3.png",
            "pack://application:,,,/Resources/Templates/Bubbles/BubbleTransparent4.png",
            "pack://application:,,,/Resources/Templates/Bubbles/BubbleTransparent5.png",
            "pack://application:,,,/Resources/Templates/Bubbles/BubbleTransparent6.png",
            "pack://application:,,,/Resources/Templates/Bubbles/BubbleTransparent7.png",
            "pack://application:,,,/Resources/Templates/Bubbles/BubbleTransparent8.png",
            "pack://application:,,,/Resources/Templates/Bubbles/BubbleTransparent9.png",
        };

        public ObservableCollection<string> EffectsTemplates { get; set; } = new ObservableCollection<string>
        {
            "pack://application:,,,/Resources/Templates/Effects/Effects1.png",
            "pack://application:,,,/Resources/Templates/Effects/Effects2.png",
            "pack://application:,,,/Resources/Templates/Effects/Effects3.png",
            "pack://application:,,,/Resources/Templates/Effects/полутон.png",
        };

        public float Pressure { get; set; } = 1f;

        public ICommand ZoomCommand { get; }
        public ICommand PanCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand GridCommand { get; }

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

            ResetCommand = new RelayCommand(_ =>
            {
                Scale = 1;
                ScalePos = new SKPoint(CanvasWidth / 2, CanvasHeight / 2);
                Rotate = 0;
                OffsetX = 0;
                OffsetY = 0;
            });
            GridCommand = new RelayCommand(_ => IsGrid = !IsGrid);
        }

        public void OnMouseMoveMouse(SKPoint pos)
        {
            if (pos.X <= CanvasWidth && pos.Y <= CanvasHeight && pos.X >= 0 && pos.Y >= 0)
                CursorPoint = new Point(pos.X, pos.Y);
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

        public Point GetScreenPoint(SKPoint canvasPoint)
        {
            var matrix = SKMatrix.CreateIdentity();

            matrix = SKMatrix.Concat(matrix, SKMatrix.CreateScale((float)Scale, (float)Scale, ScalePos.X, ScalePos.Y));
            matrix = SKMatrix.Concat(matrix, SKMatrix.CreateRotationDegrees((float)Rotate, CanvasWidth / 2, CanvasHeight / 2));
            matrix = SKMatrix.Concat(matrix, SKMatrix.CreateTranslation((float)OffsetX, (float)OffsetY));

            var screenPoint = matrix.MapPoint(canvasPoint);
            return new Point(screenPoint.X, screenPoint.Y);
        }

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
