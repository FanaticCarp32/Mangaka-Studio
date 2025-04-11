using System.Collections.ObjectModel;
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
using Mangaka_Studio.Models;
using Mangaka_Studio.Services;
using Mangaka_Studio.ViewModels;
using SkiaSharp;
using SkiaSharp.Views.WPF;

namespace Mangaka_Studio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CanvasViewModel canvas;
        ColorPickerViewModel color;
        LayerViewModel layer;
        SKPoint lastPos = new SKPoint(0, 0);
        bool flag = false;
        bool isEdit = false;
        DateTime lastClickTime;
        const int DoubleClickTimeThreshold = 300;
        LayerModel draggedItem;
        SkiaCanvas skiaCanvas;

        public MainWindow(CanvasViewModel canvas1, MainViewModel mainViewModel, ColorPickerViewModel colorPickerViewModel, LayerViewModel layerViewModel)
        {
            InitializeComponent();
            
            canvas = canvas1;
            color = colorPickerViewModel;
            layer = layerViewModel;
            DataContext = mainViewModel;
            if (DataContext == null)
                MessageBox.Show("DataContext не установлен!");
            canvas.ActualWidth = MainDrawing.ActualWidth;
            canvas.ActualHeight = MainDrawing.ActualHeight;
            
            MouseMove += MainDrawing_MouseMove;
            PreviewMouseUp += MainDrawing_MouseUp;
            GridCanvas.MouseWheel += MainDrawing_MouseWheel;
            skiaCanvas = new SkiaCanvas(canvas, layer);
            GridCanvas.Children.Add(skiaCanvas);
            skiaCanvas.MouseDown += MainDrawing_MouseDown;
            skiaCanvas.MouseMove += MainDrawing_MouseMoveCursor;
        }

        private void MainDrawing_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (flag || Mouse.LeftButton == MouseButtonState.Pressed) return;
            double zoomfactor = (e.Delta > 0) ? 1.1 : 0.9;
            //if (mousePosBefore.X <= canvas.CanvasWidth && mousePosBefore.Y <= canvas.CanvasHeight && mousePosBefore.X >= 0 && mousePosBefore.Y >= 0)
            {
                if (!canvas.ScaleChanged(zoomfactor)) return;
                var mousePosBefore = (e.GetPosition(MainDrawing).ToSKPoint());
                var canvasPointBefore = canvas.GetCanvasPoint(mousePosBefore);
                canvas.ZoomCommand.Execute((zoomfactor, canvasPointBefore));
                var mousePosAfter = (e.GetPosition(MainDrawing).ToSKPoint());
                var canvasPointAfter = canvas.GetCanvasPoint(mousePosAfter);
                canvas.OffsetX += canvasPointAfter.X - canvasPointBefore.X;
                canvas.OffsetY += canvasPointAfter.Y - canvasPointBefore.Y;
            }
        }

        private void MainDrawing_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var pos1 = e.GetPosition(MainDrawing);
            var dpi = VisualTreeHelper.GetDpi(MainDrawing);
            var physicalX = (float)(pos1.X * dpi.DpiScaleX);
            var physicalY = (float)(pos1.Y * dpi.DpiScaleY);
            SKPoint screenPoint = new SKPoint(physicalX, physicalY);
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                lastPos = canvas.GetCanvasPoint(screenPoint);
                //if (lastPos.X <= canvas.CanvasWidth && lastPos.Y <= canvas.CanvasHeight && lastPos.X >= 0 && lastPos.Y >= 0) 
                flag = true;
                return;
            }
            if (Mouse.LeftButton != MouseButtonState.Pressed) return;
            flag = true;
            SKPoint pos = canvas.GetCanvasPoint(screenPoint);
            if (pos.X <= canvas.CanvasWidth && pos.Y <= canvas.CanvasHeight && pos.X >= 0 && pos.Y >= 0)
            {
                layer.SaveState();
                canvas.OnMouseDown(canvas, pos, color, layer);
            }
        }

        private void MainDrawing_MouseMove(object sender, MouseEventArgs e)
        {
            if (canvas.CurrentTool is PipetteTool)
            {
                Mouse.OverrideCursor = new Cursor(Application.GetResourceStream(new Uri("pack://application:,,,/Resources/cursor.cur")).Stream);
            }
            else
            {
                Mouse.OverrideCursor = Cursors.Pen;
            }
            var pos1 = e.GetPosition(MainDrawing);
            var dpi = VisualTreeHelper.GetDpi(MainDrawing);
            var physicalX = (float)(pos1.X * dpi.DpiScaleX);
            var physicalY = (float)(pos1.Y * dpi.DpiScaleY);
            SKPoint screenPoint = new SKPoint(physicalX, physicalY);
            SKPoint pos = canvas.GetCanvasPoint(screenPoint);
            if (e.MiddleButton == MouseButtonState.Pressed && flag)
            {
                SKPoint posWindow = e.GetPosition(MainDrawing).ToSKPoint();
                //if (!(posWindow.X <= MainDrawing.ActualWidth - 50  && posWindow.Y <= MainDrawing.ActualHeight - 50 && posWindow.X >= 0 + 50 && posWindow.Y >= 0 + 50)) return;
                Vector d = new Vector((canvas.GetCanvasPoint(screenPoint) - lastPos).X, (canvas.GetCanvasPoint(screenPoint) - lastPos).Y);
                canvas.PanCommand.Execute(d);
                lastPos = canvas.GetCanvasPoint(screenPoint);
                return;
            }
            canvas.OnMouseMove(canvas, pos, color, layer);
        }

        private void MainDrawing_MouseMoveCursor(object sender, MouseEventArgs e)
        {
            var pos1 = e.GetPosition(MainDrawing);
            var dpi = VisualTreeHelper.GetDpi(MainDrawing);
            var physicalX = (float)(pos1.X * dpi.DpiScaleX);
            var physicalY = (float)(pos1.Y * dpi.DpiScaleY);
            SKPoint screenPoint = new SKPoint(physicalX, physicalY);
            SKPoint pos = canvas.GetCanvasPoint(screenPoint);
            canvas.OnMouseMoveMouse(pos);
        }

        private void MainDrawing_MouseUp(object sender, MouseButtonEventArgs e)
        {
            flag = false;
            canvas.OnMouseUp(canvas, layer);
        }

        private void ColorPicker_MouseDown(object sender, MouseButtonEventArgs e)
        {
            UpdateSaturationAndValue(e.GetPosition(ColorPicker));
        }

        private void ColorPicker_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !flag)
            {
                UpdateSaturationAndValue(e.GetPosition(ColorPicker));
            }
        }

        private void UpdateSaturationAndValue(Point position)
        {
            double width = ColorPicker.ActualWidth;
            double height = ColorPicker.ActualHeight;
            var clampedX = Math.Clamp(position.X, 0, width);
            var clampedY = Math.Clamp(position.Y, 0, height);
            float sat = (float)(clampedX / width);
            float val = 1 - (float)(clampedY / height); // Инверсия Y, так как верх должен быть светлым
            color.Saturation = Math.Clamp(sat, 0, 1);
            color.Value = Math.Clamp(val, 0, 1);
            color.CursorX = (float)clampedX - 2.5f;
            color.CursorY = (float)clampedY - 2.5f;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            PopupPaletteColor.IsOpen = false;
        }

        private void ListBox_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Scroll;
        }

        private void ListBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Scroll;
        }

        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox == null || draggedItem == null) return;

            var targetItem = GetListBoxItem(listBox, e.GetPosition(listBox));
            if (targetItem == null || targetItem.DataContext == draggedItem) return;

            var targetLayer = (LayerModel)targetItem.DataContext;
            var layers = listBox.ItemsSource as ObservableCollection<LayerModel>;

            if (layers != null)
            {
                int oldIndex = layers.IndexOf(draggedItem);
                int newIndex = layers.IndexOf(targetLayer);

                if (oldIndex != newIndex)
                {
                    layers.Move(oldIndex, newIndex);
                }
            }
            layer.OnPropertyChanged(nameof(layers));
        }

        private void ListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !flag)
            {
                var listbox = sender as ListBox;
                if (listbox == null) return;
                var item = GetListBoxItem(listbox, e.GetPosition(listbox));

                if (item != null)
                {
                    if (IsClickOnCheckBox(item, e)) return;
                    draggedItem = (LayerModel)item.DataContext;

                    DragDrop.DoDragDrop(item, draggedItem, DragDropEffects.Move);
                }
            }
        }

        private ListBoxItem GetListBoxItem(ListBox listbox, Point pos)
        {
            var hitTestResult = VisualTreeHelper.HitTest(listbox, pos);
            if (pos.X < 0 || pos.Y < 0 || pos.X > listbox.ActualWidth || pos.Y > listbox.ActualHeight) return null;
            var depObj = hitTestResult.VisualHit;

            while (depObj != null && !(depObj is ListBoxItem))
            {
                depObj = VisualTreeHelper.GetParent(depObj);
            }
            return depObj as ListBoxItem;
        }

        private bool IsClickOnCheckBox(ListBoxItem item, MouseEventArgs e)
        {
            var position = e.GetPosition(item);
            var hitTestResult = VisualTreeHelper.HitTest(item, position);
            var depObj = hitTestResult?.VisualHit;

            while (depObj != null)
            {
                if (depObj is CheckBox || depObj is TextBlock || depObj is TextBox)
                {
                    return true; // Клик был на CheckBox, игнорируем DragDrop
                }
                depObj = VisualTreeHelper.GetParent(depObj);
            }

            return false;
        }

        private void LayerNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (isEdit)
            {
                isEdit = false;
                return;
            }
            HideTextBox(sender);
        }

        private void LayerNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                isEdit = false;
                HideTextBox(sender);
            }
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                var currentTime = DateTime.Now;
                var timeDiff = (currentTime - lastClickTime).TotalMilliseconds;

                if (timeDiff <= DoubleClickTimeThreshold)
                {
                    SwitchToEditMode(sender);
                }

                lastClickTime = currentTime;
            }
        }

        private void SwitchToEditMode(object sender)
        {
            var textBlock = sender as TextBlock;
            var stackPanel = textBlock?.Parent as StackPanel;
            if (stackPanel != null)
            {
                textBlock.Visibility = Visibility.Collapsed;
                var textBox = stackPanel.FindName("LayerNameTextBox") as TextBox;
                if (textBox != null)
                {
                    isEdit = true;
                    textBox.Visibility = Visibility.Visible;
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        textBox.Focus(); // Устанавливаем фокус
                        textBox.SelectAll(); // Выделяем весь текст
                    }), System.Windows.Threading.DispatcherPriority.Input);
                }
            }
        }

        private void HideTextBox(object sender)
        {
            var textBox = sender as TextBox;
            var stackPanel = textBox?.Parent as StackPanel;
            if (stackPanel != null)
            {
                textBox.Visibility = Visibility.Collapsed;
                var textBlock = stackPanel.FindName("LayerNameTextBlock") as TextBlock;
                if (textBlock != null)
                {
                    textBlock.Visibility = Visibility.Visible;
                    layer.SelectLayer.Name = textBox.Text;

                }
            }
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var focusedElement = Keyboard.FocusedElement as UIElement;
            if (focusedElement is TextBox textBox && textBox.Name == "LayerNameTextBox")
            {
                var clickedElement = e.OriginalSource as DependencyObject;
                if (!IsParentOf(textBox, clickedElement))
                {
                    if (isEdit)
                    {
                        isEdit = false;
                    }
                    HideTextBox(textBox); // Скрываем TextBox
                }
            }
        }

        private bool IsParentOf(DependencyObject parent, DependencyObject child)
        {
            DependencyObject current = child;
            while (current != null)
            {
                if (current == parent)
                {
                    return true;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return false;
        }
    }
}