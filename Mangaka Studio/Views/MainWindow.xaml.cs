using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Mangaka_Studio.Controls;
using Mangaka_Studio.Services;
using Mangaka_Studio.ViewModels;

namespace Mangaka_Studio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CanvasViewModel canvas;
        Point lastPos = new Point(0, 0);
        bool flag = false;
        //private CustomCanvas customCanvas;
        public MainWindow(CanvasViewModel canvas1, MainViewModel mainViewModel)
        {
            InitializeComponent();
            
            canvas = canvas1;
            DataContext = mainViewModel;
            if (DataContext == null)
                MessageBox.Show("DataContext не установлен!");

            MouseWheel += MainWindow_MouseWheel;
            
            //customCanvas = new CustomCanvas();
            //GridCanvas.Children.Add(customCanvas);
            
        }

        private void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (flag || Mouse.LeftButton == MouseButtonState.Pressed) return;
            double zoomfactor = (e.Delta > 0) ? 1.1 : 0.9;
            canvas.ZoomCommand.Execute((zoomfactor, e.GetPosition(MainDrawing), MainDrawing.ActualWidth, MainDrawing.ActualHeight, DrawBorder.ActualWidth, DrawBorder.ActualHeight));
        }

        private void MainDrawing_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                flag = true;
                lastPos = e.GetPosition(this);
                return;
            }
            if (Mouse.LeftButton != MouseButtonState.Pressed) return;
            Point pos = e.GetPosition(MainDrawing);
            canvas.OnMouseDown(MainDrawing, pos);
        }

        private void MainDrawing_MouseMove(object sender, MouseEventArgs e)
        {
            Point pos = e.GetPosition(MainDrawing);
            if (e.MiddleButton == MouseButtonState.Pressed && flag)
            {
                Vector d = e.GetPosition(this) - lastPos;
                canvas.PanCommand.Execute(d);
                lastPos = e.GetPosition(this);
                return;
            }
            canvas.OnMouseMove(MainDrawing, pos);

        }

        private void MainDrawing_MouseUp(object sender, MouseButtonEventArgs e)
        {
            flag = false;
            Point pos = e.GetPosition(MainDrawing);
            canvas.OnMouseUp();
        }

        private void MainDrawing_MouseLeave(object sender, MouseEventArgs e)
        {
            flag = false;
            //MessageBox.Show(e.GetPosition(MainDrawing).ToString());
            canvas.OnMouseLeave(MainDrawing, e.GetPosition(MainDrawing));
        }

        private void MainDrawing_MouseEnter(object sender, MouseEventArgs e)
        {
            //MessageBox.Show(e.GetPosition(MainDrawing).ToString());
            if (Mouse.LeftButton != MouseButtonState.Pressed) return;
            Point pos = e.GetPosition(MainDrawing);
            canvas.OnMouseEnter(MainDrawing, pos);

        }
    }
}