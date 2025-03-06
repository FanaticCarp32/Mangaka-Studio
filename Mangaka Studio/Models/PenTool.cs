using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Input;

namespace Mangaka_Studio.Models
{
    class PenTool : DrawingTools
    {
        private bool flag = false;
        private List<Point> listPoints = new List<Point>();
        private PathFigure pathFigure;
        private PathGeometry pathGeometry;
        private Path path;

        public override void OnMouseDown(Canvas canvas, Point pos)
        {
            if (canvas == null) return;
            flag = true;
            listPoints.Clear();
            listPoints.Add(pos);
            listPoints.Add(pos);
            
            path = new Path
            {
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round
            };
            pathGeometry = new PathGeometry();
            pathFigure = new PathFigure { StartPoint = pos, IsClosed = false, IsFilled = false };
            pathGeometry.Figures.Add(pathFigure);
            path.Data = pathGeometry;
            canvas.Children.Add(path);
        }

        public override void OnMouseMove(Canvas canvas, Point pos)
        {
            if (flag && canvas != null && path != null)
            {
                if (pos.X < 0 || pos.Y < 0 || pos.X > canvas.ActualWidth || pos.Y > canvas.ActualHeight) return;

                Point lastPoint = listPoints[^1];
                double distance = Math.Sqrt(Math.Pow(pos.X - lastPoint.X, 2) + Math.Pow(pos.Y - lastPoint.Y, 2));
                if (distance > 2)
                {
                    listPoints.Add(pos);
                    Point p0 = listPoints[^2];
                    Point p1 = listPoints[^1];
                    Point p2 = pos;
                    Point c0 = new Point((p0.X + p1.X) / 2, (p0.Y + p1.Y) / 2);
                    Point c1 = new Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
                    BezierSegment bezier = new BezierSegment(
                        c0,
                        c1,
                        pos,
                        true
                    );
                    pathFigure.Segments.Add(bezier);
                }

                if (listPoints.Count > 1000)
                {
                    listPoints.RemoveAt(0);
                }
            }
        }

        public override void OnMouseUp()
        {
            flag = false;
            path = null;
        }

        public override void OnMouseLeave(Canvas canvas, Point pos)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed && flag)
            {
                if (pos.X < 0) pos.X = 0;
                if (pos.Y < 0) pos.Y = 0;
                if (pos.X > canvas.ActualWidth) pos.X = canvas.ActualWidth;
                if (pos.Y > canvas.ActualHeight) pos.Y = canvas.ActualHeight;
                Point p0 = listPoints[^2];
                Point p1 = listPoints[^1];
                Point p2 = pos;
                Point c0 = new Point((p0.X + p1.X) / 2, (p0.Y + p1.Y) / 2);
                Point c1 = new Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
                BezierSegment bezier = new BezierSegment(
                        c0,
                        c1,
                        pos,
                        true
                    );
                pathFigure.Segments.Add(bezier);
            }
            
            path = null;
        }
        
        public override void OnMouseEnter(Canvas canvas, Point pos)
        {
            if (flag)
            {
                listPoints.Clear();
                double left = pos.X;                                // Расстояние до левой границы
                double right = canvas.ActualWidth - pos.X;         // Расстояние до правой границы
                double top = pos.Y;                                 // Расстояние до верхней границы
                double bottom = canvas.ActualHeight - pos.Y;       // Расстояние до нижней границы

                // Определяем, какая граница ближе
                if (left <= right && left <= top && left <= bottom)
                {
                    pos.X = 0; // Ближе к левой границе
                }
                else if (right <= left && right <= top && right <= bottom)
                {
                    pos.X = canvas.ActualWidth; // Ближе к правой границе
                }
                else if (top <= left && top <= right && top <= bottom)
                {
                    pos.Y = 0; // Ближе к верхней границе
                }
                else if (bottom <= left && bottom <= right && bottom <= top)
                {
                    pos.Y = canvas.ActualHeight; // Ближе к нижней границе
                }


                listPoints.Add(pos);
                listPoints.Add(pos);

                path = new Path
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 2,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round,
                    StrokeLineJoin = PenLineJoin.Round
                };
                pathGeometry = new PathGeometry();
                pathFigure = new PathFigure { StartPoint = pos };
                pathGeometry.Figures.Add(pathFigure);
                path.Data = pathGeometry;
                canvas.Children.Add(path);
                
            }
        }
    }
}
