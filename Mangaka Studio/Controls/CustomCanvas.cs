using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Mangaka_Studio.Controls
{
    public class CustomCanvas : Canvas
    {
        private DrawingVisual _drawingVisual = new DrawingVisual();  // DrawingVisual для рисования
        private List<Point> points = new List<Point>();  // Список точек для рисования
        private bool isDrawing = false;  // Флаг, рисуем ли мы в данный момент

        public CustomCanvas()
        {
            // Включаем обработку событий мыши
            this.MouseDown += CustomCanvas_MouseDown;
            this.MouseMove += CustomCanvas_MouseMove;
            this.MouseUp += CustomCanvas_MouseUp;
        }

        // Обработчик MouseDown
        private void CustomCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(this);  // Получаем координаты позиции на Canvas
            if (isDrawing) return;  // Если уже рисуем, не начинаем новое рисование

            isDrawing = true;
            points.Clear();  // Очищаем старые точки, начинаем новый путь
            points.Add(pos);  // Добавляем первую точку
            Redraw();  // Перерисовываем
        }

        // Обработчик MouseMove
        private void CustomCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(this);  // Получаем текущую позицию мыши
            if (!isDrawing) return;  // Если не рисуем, ничего не делаем

            // Если точка достаточно далеко от предыдущей, рисуем линию
            double distance = Math.Sqrt(Math.Pow(pos.X - points.Last().X, 2) + Math.Pow(pos.Y - points.Last().Y, 2));
            if (distance > 2)  // Добавляем небольшую погрешность для уменьшения количества точек
            {
                points.Add(pos);  // Добавляем новую точку
                Redraw();  // Перерисовываем
            }
        }

        // Обработчик MouseUp
        private void CustomCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDrawing = false;  // Завершаем рисование
        }

        // Метод для рисования линий с использованием DrawingContext
        private void Redraw()
        {
            // Получаем DrawingContext через DrawingVisual
            using (DrawingContext dc = _drawingVisual.RenderOpen())
            {
                // Если точек недостаточно, не рисуем
                if (points.Count < 2)
                    return;

                // Используем перо для рисования линий
                Pen pen = new Pen(Brushes.Black, 2);
                for (int i = 1; i < points.Count; i++)
                {
                    dc.DrawLine(pen, points[i - 1], points[i]);  // Рисуем линии между точками
                }
            }

            // Уведомляем систему о том, что нужно обновить визуальное содержимое
            this.InvalidateVisual();
        }

        // Переопределяем метод для получения дочерних визуальных элементов
        protected override int VisualChildrenCount => 1;

        // Возвращаем DrawingVisual
        protected override Visual GetVisualChild(int index) => _drawingVisual;
    }




}
