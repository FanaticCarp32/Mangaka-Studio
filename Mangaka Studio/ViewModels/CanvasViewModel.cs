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

namespace Mangaka_Studio.ViewModels
{
    public class CanvasViewModel : INotifyPropertyChanged
    {
        private DrawingTools currentTool;
        private double scale = 1.0;
        private double scaleMax = 10.0;
        private double scaleMin = 0.1;
        private double offsetX = 0;
        private double offsetXMax = 1000;
        private double offsetXMin = -1000;
        private double offsetY = 0;
        private double offsetYMax = 600;
        private double offsetYMin = -600;
        private MatrixTransform matrixTransform = new MatrixTransform(Matrix.Identity);

        public MatrixTransform CanvasMatrix
        {
            get => matrixTransform;
            set
            {
                matrixTransform = value;
                OnPropertyChanged(nameof(CanvasMatrix));
            }
        }

        public double Scale
        {
            get => scale;
            set
            {
                scale = value;
                OnPropertyChanged(nameof(Scale));
            }
        }

        public double OffsetX
        {
            get => offsetX;
            set
            {
                offsetX = value;
                OnPropertyChanged(nameof(OffsetX));
            }
        }

        public double OffsetY
        {
            get => offsetY;
            set
            {
                offsetY = value;
                OnPropertyChanged(nameof(OffsetY));
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

        public ICommand ZoomCommand { get; }
        public ICommand PanCommand { get; }

        public CanvasViewModel()
        {
            ZoomCommand = new RelayCommand(param =>
            {
                var tuple = ((double, Point, double, double, double, double))param;
                double zoomFactor = tuple.Item1;
                Point mousePos = tuple.Item2;

                // Ограничиваем масштаб
                double newScale = Math.Max(scaleMin, Math.Min(scaleMax, Scale * zoomFactor));
                Matrix matrix = matrixTransform.Matrix;
                matrix.ScaleAtPrepend(newScale / Scale, newScale / Scale, mousePos.X, mousePos.Y);
                CanvasMatrix = new MatrixTransform(matrix);
                // Применяем новый масштаб
                Scale = newScale;
                //RestrictPosition(tuple.Item3, tuple.Item4, tuple.Item5, tuple.Item6);
            });

            PanCommand = new RelayCommand(param =>
            {
                var movement = (Vector)param;
                double newOffsetX = Math.Max(offsetXMin, Math.Min(offsetXMax, OffsetX + movement.X));
                double newOffsetY = Math.Max(offsetYMin, Math.Min(offsetYMax, OffsetY + movement.Y));
                
                Matrix matrix = matrixTransform.Matrix;
                matrix.Translate(newOffsetX - OffsetX, newOffsetY - OffsetY);
                CanvasMatrix = new MatrixTransform(matrix);
                OffsetX = newOffsetX;
                OffsetY = newOffsetY;
                //MessageBox.Show(movement.ToString());
            });
        }

        private void RestrictPosition(double canvasWidth, double canvasHeight, double viewWidth, double viewHeight)
        {
            Matrix matrix = CanvasMatrix.Matrix;

            double minX = -(canvasWidth * Scale - viewWidth);
            double minY = -(canvasHeight * Scale - viewHeight);
            double maxY = 0;
            double maxX = 0;
            matrix.OffsetX = Math.Min(maxX, Math.Max(minX, matrix.OffsetX));
            matrix.OffsetY = Math.Min(maxY, Math.Max(minY, matrix.OffsetY));
            CanvasMatrix = new MatrixTransform(matrix);
        }

        public void OnMouseDown(Canvas canvas, Point pos)
        {
            currentTool?.OnMouseDown(canvas, pos);
            
        }

        public void OnMouseMove(Canvas canvas, Point pos)
        {
            currentTool?.OnMouseMove(canvas, pos);
        }

        public void OnMouseUp()
        {
            currentTool?.OnMouseUp();
        }
        
        public void OnMouseLeave(Canvas canvas, Point pos)
        {
            currentTool?.OnMouseLeave(canvas, pos);
        }

        public void OnMouseEnter(Canvas canvas, Point pos)
        {
            currentTool?.OnMouseEnter(canvas, pos);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
