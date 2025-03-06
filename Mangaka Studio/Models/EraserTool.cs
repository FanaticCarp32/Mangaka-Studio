using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Mangaka_Studio.Models
{
    class EraserTool : DrawingTools
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
                Stroke = canvas.Background,
                StrokeThickness = 10,
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

        public override void OnMouseMove(Canvas canvas, Point pos)
        {
            if (flag && canvas != null && path != null)
            {
                if (pos.X < 0 || pos.Y < 0 || pos.X > canvas.ActualWidth || pos.Y > canvas.ActualHeight) return;

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
                //pathFigure.StartPoint = pos;
                listPoints.Add(pos);

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
                if (canvas.ActualWidth - pos.X > canvas.ActualHeight - pos.Y && canvas.ActualHeight - pos.Y > pos.X) pos.X = 0;
                if (canvas.ActualWidth - pos.X > canvas.ActualHeight - pos.Y && canvas.ActualHeight - pos.Y < pos.X) pos.Y = canvas.ActualHeight;
                if (canvas.ActualWidth - pos.X < canvas.ActualHeight - pos.Y && canvas.ActualWidth - pos.X > pos.Y) pos.Y = 0;
                if (canvas.ActualWidth - pos.X < canvas.ActualHeight - pos.Y && canvas.ActualWidth - pos.X < pos.Y) pos.X = canvas.ActualWidth;
                listPoints.Add(pos);
                listPoints.Add(pos);

                path = new Path
                {
                    Stroke = canvas.Background,
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
