using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Xml.Linq;
using Mangaka_Studio.Controls.Renders;
using Mangaka_Studio.Controls.Tools;
using Mangaka_Studio.Interfaces;
using Mangaka_Studio.Models;
using Mangaka_Studio.ViewModels;
using Mangaka_Studio.Views;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using static System.Net.Mime.MediaTypeNames;
using Image = System.Windows.Controls.Image;

namespace Mangaka_Studio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CanvasViewModel canvas;
        ColorPickerViewModel color;
        TextTemplatesViewModel textTemp;
        private ICanvasContext canvasContext;
        private IFrameContext frameContext;
        private IColorPickerContext colorPickerContext;
        private ITextTemplatesContext textTemplatesContext;
        FrameViewModel frame;
        FileViewModel file;
        SKPoint lastPos = new SKPoint(0, 0);
        bool flag = false;
        bool isEdit = false;
        bool flagStylus = false;
        DateTime lastClickTime;
        const int DoubleClickTimeThreshold = 300;
        Object draggedItem;
        int snap = 10;
        SkiaCanvas skiaCanvas;
        DragMode currentDrag = DragMode.None;
        int currentPoint;
        SKPoint dragStart;
        SKPoint[] originalBounds;

        public MainWindow(ToolsViewModel toolsViewModel, CanvasViewModel canvas1, MainViewModel mainViewModel, ColorPickerViewModel colorPickerViewModel, FrameViewModel frameViewModel, FileViewModel fileViewModel, TextTemplatesViewModel textTemplatesViewModel,
            ICanvasContext canvasContext, IColorPickerContext colorPickerContext, IFrameContext frameContext, ITextTemplatesContext textTemplatesContext)
        {
            InitializeComponent();
            canvas = canvas1;
            color = colorPickerViewModel;
            frame = frameViewModel;
            file = fileViewModel;
            textTemp = textTemplatesViewModel;
            this.canvasContext = canvasContext;
            this.frameContext = frameContext;
            this.colorPickerContext = colorPickerContext;
            this.textTemplatesContext = textTemplatesContext;
            DataContext = mainViewModel;
            if (DataContext == null)
                MessageBox.Show("DataContext не установлен!");
            
            MouseMove += MainDrawing_MouseMove;
            PreviewMouseUp += MainDrawing_MouseUp;
            GridCanvas.MouseWheel += MainDrawing_MouseWheel;
            skiaCanvas = new SkiaCanvas(frame, textTemplatesViewModel, frameContext, canvasContext);
            GridCanvas.Children.Add(skiaCanvas);
            skiaCanvas.MouseDown += MainDrawing_MouseDown;
            skiaCanvas.MouseMove += MainDrawing_MouseMoveCursor;
            skiaCanvas.StylusDown += SkiaCanvas_StylusDown;
            skiaCanvas.StylusMove += SkiaCanvas_StylusMove;
            TextEditor.LostFocus += TextEditor_LostFocus;
            TextEditor.KeyDown += TextEditor_KeyDown;
            TextEditor.TextChanged += TextEditor_TextChanged;
            skiaCanvas.AllowDrop = true;
            skiaCanvas.Drop += MainDrawing_Drop;
            skiaCanvas.DragEnter += MainDrawing_DragEnter;
            skiaCanvas.PreviewDragOver += MainDrawing_DragOver;
        }

        private void SkiaCanvas_StylusMove(object sender, StylusEventArgs e)
        {
            if (frame.SelectFrame == null || frame.SelectFrame.LayerVM.Layers.Count == 0) return;
            UpdatePressure(e);
            var pos1 = e.GetPosition(MainDrawing).ToSKPoint();
            var pos = canvas.GetCanvasPoint(pos1);

            if (currentDrag != DragMode.None)
            {
                var dx = pos.X - dragStart.X;
                var dy = pos.Y - dragStart.Y;
                SKPoint[] points = (SKPoint[])originalBounds.Clone();
                SwitchDrag(dx, dy, points);
                var path = new SKPath();
                path.MoveTo(points[0]);
                for (var i = 1; i < points.Length; i++)
                {
                    path.LineTo(points[i]);
                }
                path.Close();
                frame.SelectFrame.Bounds = path;
                canvas.OnPropertyChanged(nameof(canvas.EraserCursor));
                return;
            }

            if (canvas.CurrentTool is TextTool && Keyboard.IsKeyDown(Key.LeftShift))
            {
                canvas.IsTextPosSnap = true;
            }
            else if (canvas.CurrentTool is TextTool)
            {
                canvas.IsTextPosSnap = false;
            }

            canvas.CurrentTool.OnMouseMove(canvasContext, pos, colorPickerContext, frameContext, textTemplatesContext);
        }

        private void SkiaCanvas_StylusDown(object sender, StylusDownEventArgs e)
        {
            if (frame.SelectFrame == null || frame.SelectFrame.LayerVM.Layers.Count == 0) return;
            UpdatePressure(e);
            var pos1 = e.GetPosition(MainDrawing).ToSKPoint();
            var pos = canvas.GetCanvasPoint(pos1);
            flag = true;

            if (frame.SelectFrame != null && frame.SelectFrame.IsSelected)
            {
                if (frame.SelectFrame.LayerVM.SelectText != null)
                    frame.SelectFrame.LayerVM.SelectText.IsSelected = false;
                var handles = frame.SelectFrame.Bounds.Points;
                for (int i = 0; i < handles.Length; i++)
                {
                    if (SKPoint.Distance(pos, handles[i]) < frame.SelectFrame.HandleSize)
                    {
                        currentDrag = DragMode.MovePoint;
                        currentPoint = i;
                        dragStart = pos;
                        originalBounds = frame.SelectFrame.Bounds.Points;
                        return;
                    }
                }
                if (frame.SelectFrame.Bounds.Contains(pos.X, pos.Y))
                {
                    currentDrag = DragMode.MoveAll;
                    dragStart = pos;
                    originalBounds = frame.SelectFrame.Bounds.Points;
                }
                return;
            }

            if (pos.X <= canvas.CanvasWidth && pos.Y <= canvas.CanvasHeight && pos.X >= 0 && pos.Y >= 0)
            {
                if (canvas.CurrentTool is not TextTool)
                    frame.SelectFrame.LayerVM.SaveState();
                frame.SelectFrame.LayerVM.EnsureSurface();
                canvas.TextEditor = TextEditor;
                canvas.CurrentTool.OnMouseDown(canvasContext, pos, colorPickerContext, frameContext, textTemplatesContext);
            }
            flagStylus = true;
        }

        private void UpdatePressure(StylusEventArgs e)
        {
            var points = e.GetStylusPoints(MainDrawing);
            if (points.Count > 0)
            {
                canvas.Pressure = points[0].PressureFactor;
            }
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
            if (flagStylus || frame.SelectFrame == null || frame.SelectFrame.LayerVM.Layers.Count == 0) return;
            canvas.Pressure = 1f;
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

            if (frame.SelectFrame != null && frame.SelectFrame.IsSelected)
            {
                if (frame.SelectFrame.LayerVM.SelectText != null)
                    frame.SelectFrame.LayerVM.SelectText.IsSelected = false;
                var handles = frame.SelectFrame.Bounds.Points;
                for (int i = 0; i < handles.Length; i++)
                {
                    if (SKPoint.Distance(pos, handles[i]) < frame.SelectFrame.HandleSize)
                    {
                        currentDrag = DragMode.MovePoint;
                        currentPoint = i;
                        dragStart = pos;
                        originalBounds = frame.SelectFrame.Bounds.Points;
                        return;
                    }
                }
                if (frame.SelectFrame.Bounds.Contains(pos.X, pos.Y))
                {
                    currentDrag = DragMode.MoveAll;
                    dragStart = pos;
                    originalBounds = frame.SelectFrame.Bounds.Points;
                }
                return;
            }

            if (pos.X <= canvas.CanvasWidth && pos.Y <= canvas.CanvasHeight && pos.X >= 0 && pos.Y >= 0)
            {
                if (canvas.CurrentTool is not TextTool)    
                    frame.SelectFrame.LayerVM.SaveState();
                frame.SelectFrame.LayerVM.EnsureSurface();
                canvas.TextEditor = TextEditor;
                canvas.CurrentTool.OnMouseDown(canvasContext, pos, colorPickerContext, frameContext, textTemplatesContext);
            }
        }

        private void MainDrawing_MouseMove(object sender, MouseEventArgs e)
        {
            if (flagStylus || frame.SelectFrame == null || frame.SelectFrame.LayerVM.Layers.Count == 0) return;
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

            if (currentDrag != DragMode.None)
            {
                var dx = pos.X - dragStart.X;
                var dy = pos.Y - dragStart.Y;
                SKPoint[] points = (SKPoint[])originalBounds.Clone();
                SwitchDrag(dx, dy, points);
                var path = new SKPath();
                path.MoveTo(points[0]);
                for (var i = 1; i < points.Length; i++)
                {
                    path.LineTo(points[i]);
                }
                path.Close();
                frame.SelectFrame.Bounds = path;
                canvas.OnPropertyChanged(nameof(canvas.EraserCursor));
                return;
            }

            if (canvas.CurrentTool is TextTool && Keyboard.IsKeyDown(Key.LeftShift))
            {
                canvas.IsTextPosSnap = true;
            }
            else if (canvas.CurrentTool is TextTool)
            {
                canvas.IsTextPosSnap = false;
            }
            canvas.CurrentTool.OnMouseMove(canvasContext, pos, colorPickerContext, frameContext, textTemplatesContext);
            //Debug.WriteLine($"MouseMove {e.GetPosition(this)}   " + DateTime.Now);
            //base.OnMouseMove(e);
        }

        private void SwitchDrag(float dx, float dy, SKPoint[] originalBounds)
        {
            switch (currentDrag)
            {
                case DragMode.MoveAll:
                    for (int i = 0; i < originalBounds.Length; i++)
                    {
                        var newX = originalBounds[i].X + dx;
                        var newY = originalBounds[i].Y + dy;
                        if (Keyboard.IsKeyDown(Key.LeftShift))
                        {
                            newX = Snap(newX, 10);
                            newY = Snap(newY, 10);
                        }
                        originalBounds[i] = new SKPoint(newX, newY);
                    }
                    break;
                case DragMode.MovePoint:
                    if (frame.SelectFrame.FrameMode == FrameMode.Polyline)
                    {
                        var newX = originalBounds[currentPoint].X + dx;
                        var newY = originalBounds[currentPoint].Y + dy;
                        if (Keyboard.IsKeyDown(Key.LeftShift))
                        {
                            newX = Snap(newX, snap);
                            newY = Snap(newY, snap);
                        }
                        originalBounds[currentPoint] = new SKPoint(newX, newY);
                    }
                    else if(frame.SelectFrame.FrameMode == FrameMode.Rectangle)
                    {
                        if (Keyboard.IsKeyDown(Key.LeftShift))
                        {
                            if (currentPoint == 0)
                            {
                                originalBounds[0].X = Snap(originalBounds[0].X + dx, snap);
                                originalBounds[0].Y = Snap(originalBounds[0].Y + dy, snap);
                                originalBounds[1].Y = Snap(originalBounds[1].Y + dy, snap);
                                originalBounds[3].X = Snap(originalBounds[3].X + dx, snap);
                            }
                            else if (currentPoint == 1)
                            {
                                originalBounds[1].X = Snap(originalBounds[1].X + dx, snap);
                                originalBounds[1].Y = Snap(originalBounds[1].Y + dy, snap);
                                originalBounds[0].Y = Snap(originalBounds[0].Y + dy, snap);
                                originalBounds[2].X = Snap(originalBounds[2].X + dx, snap);
                            }
                            else if (currentPoint == 2)
                            {
                                originalBounds[2].X = Snap(originalBounds[2].X + dx, snap);
                                originalBounds[2].Y = Snap(originalBounds[2].Y + dy, snap);
                                originalBounds[3].Y = Snap(originalBounds[3].Y + dy, snap);
                                originalBounds[1].X = Snap(originalBounds[1].X + dx, snap);
                            }
                            else if (currentPoint == 3)
                            {
                                originalBounds[3].X = Snap(originalBounds[3].X + dx, snap);
                                originalBounds[3].Y = Snap(originalBounds[3].Y + dy, snap);
                                originalBounds[2].Y = Snap(originalBounds[2].Y + dy, snap);
                                originalBounds[0].X = Snap(originalBounds[0].X + dx, snap);
                            }
                        }
                        else
                        {
                            if (currentPoint == 0)
                            {
                                originalBounds[0].X += dx;
                                originalBounds[0].Y += dy;
                                originalBounds[1].Y += dy;
                                originalBounds[3].X += dx;
                            }
                            else if (currentPoint == 1)
                            {
                                originalBounds[1].X += dx;
                                originalBounds[1].Y += dy;
                                originalBounds[0].Y += dy;
                                originalBounds[2].X += dx;
                            }
                            else if (currentPoint == 2)
                            {
                                originalBounds[2].X += dx;
                                originalBounds[2].Y += dy;
                                originalBounds[3].Y += dy;
                                originalBounds[1].X += dx;
                            }
                            else if (currentPoint == 3)
                            {
                                originalBounds[3].X += dx;
                                originalBounds[3].Y += dy;
                                originalBounds[2].Y += dy;
                                originalBounds[0].X += dx;
                            }
                        }
                    }
                    break;
            }
        }

        float Snap(float value, float step)
        {
            return (float)(Math.Round(value / step) * step);
        }

        private void TextEditor_LostFocus(object sender, RoutedEventArgs e)
        {
            TextEditor.Visibility = Visibility.Collapsed;
            //if (frame.SelectFrame.LayerVM.SelectText != null)
            //{
            //    frame.SelectFrame.LayerVM.SelectText.Text = TextEditor.Text;
            //    frame.SelectFrame.LayerVM.SelectText.IsSelected = false;
            //    frame.SelectFrame.LayerVM.SelectText = null;
            //}
            canvas.IsTextEditor = false;
        }

        private void TextEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextEditor.Visibility = Visibility.Collapsed;
                frame.SelectFrame.LayerVM.SelectText.Text = TextEditor.Text;
                canvas.IsTextEditor = false;
            }
        }

        private void TextEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            canvas.OnPropertyChanged(nameof(canvas.EraserCursor));
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
            if (frame.SelectFrame.LayerVM.Layers.Count == 0) return;
            if (currentDrag != DragMode.None)
            {
                currentDrag = DragMode.None;
            }
            flag = false;
            flagStylus = false;
            canvas.CurrentTool.OnMouseUp(canvasContext, frameContext);
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

            //var targetLayer = (LayerModel)targetItem.DataContext;

            if (draggedItem is LayerModel draggedLayer && targetItem.DataContext is LayerModel targetLayer)
            {
                var layers = listBox.ItemsSource as ObservableCollection<LayerModel>;
                if (layers != null)
                {
                    int oldIndex = layers.IndexOf(draggedLayer);
                    int newIndex = layers.IndexOf(targetLayer);

                    if (oldIndex != newIndex)
                    {
                        layers.Move(oldIndex, newIndex);
                    }
                }
                frame.SelectFrame.LayerVM.OnPropertyChanged(nameof(layers));
            }
            else if (draggedItem is FrameLayerModel draggedFrame && targetItem.DataContext is FrameLayerModel targetFrame)
            {
                var layers = listBox.ItemsSource as ObservableCollection<FrameLayerModel>;
                if (layers != null)
                {
                    int oldIndex = layers.IndexOf(draggedFrame);
                    int newIndex = layers.IndexOf(targetFrame);

                    if (oldIndex != newIndex)
                    {
                        layers.Move(oldIndex, newIndex);
                    }
                }
                frame.SelectFrame.LayerVM.OnPropertyChanged(nameof(layers));
            }
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
                    draggedItem = item.DataContext;

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
                if (textBox == null)
                {
                    textBox = stackPanel.FindName("FrameNameTextBox") as TextBox;
                }
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
                    frame.SelectFrame.LayerVM.SelectLayer.Name = textBox.Text;
                }
                else
                {
                    textBlock = stackPanel.FindName("FrameNameTextBlock") as TextBlock;
                    if (textBlock != null)
                    {
                        textBlock.Visibility = Visibility.Visible;
                        frame.SelectFrame.Name = textBox.Text;
                    }
                }
            }
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var focusedElement = Keyboard.FocusedElement as UIElement;
            if (focusedElement is TextBox textBox && (textBox.Name == "LayerNameTextBox" || textBox.Name == "FrameNameTextBox"))
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (frame.SelectFrame.LayerVM.IsModified) // Проверяем флаг изменений
            {
                var result = MessageBox.Show(
                    "Файл содержит несохраненные изменения. Хотите сохранить их?",
                    "Подтверждение",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    file.SaveFile("false"); // Сохраняем изменения
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true; // Отменяем закрытие окна
                }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (frame.SelectFrame.LayerVM.IsModified)
            {
                var result = MessageBox.Show(
                    "Файл содержит несохраненные изменения. Хотите сохранить их?",
                    "Подтверждение",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    file.SaveFile("false"); // Сохраняем текущие изменения
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return; // Отменяем операцию
                }
            }
            var dialog = new NewFileDialog();
            if (dialog.ShowDialog() == true)
            {
                file.NewFile(dialog.width, dialog.height);
            }
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !flag)
            {
                if (sender is Image image && image.DataContext is string path)
                {
                    DragDrop.DoDragDrop(image, path, DragDropEffects.Copy);
                }
            }
        }

        private void MainDrawing_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                string droppedPath = (string)e.Data.GetData(DataFormats.StringFormat);

                var pos1 = e.GetPosition(MainDrawing);
                var dpi = VisualTreeHelper.GetDpi(MainDrawing);
                var physicalX = (float)(pos1.X * dpi.DpiScaleX);
                var physicalY = (float)(pos1.Y * dpi.DpiScaleY);
                SKPoint screenPoint = new SKPoint(physicalX, physicalY);
                SKPoint pos = canvas.GetCanvasPoint(screenPoint);
                using var bitmap = LoadBitmapFromPackUri(droppedPath);
                if (bitmap != null)
                {
                    float width = bitmap.Width;
                    float height = bitmap.Height;

                    var bounds = new SKRect(
                        pos.X - width / 2,
                        pos.Y - height / 2,
                        pos.X + width / 2,
                        pos.Y + height / 2
                    );

                    textTemp.IsVisibleEditText = "Collapsed";
                    textTemp.TextModeStr = "";
                    textTemp.IsHitTextDel = false;
                    textTemp.OpacityBoundsTextDel = 0.5f;
                    if (frame.SelectFrame.LayerVM.SelectTemplate != null)
                    {
                        frame.SelectFrame.LayerVM.SelectTemplate.IsSelected = false;
                        frame.SelectFrame.LayerVM.SelectTemplate = null;
                    }
                    frame.SelectFrame.LayerVM.AddTemplate(bounds, droppedPath);
                }
            }
        }

        private SKBitmap LoadBitmapFromPackUri(string packUri)
        {
            var uri = new Uri(packUri, UriKind.Absolute);
            StreamResourceInfo resourceInfo = System.Windows.Application.GetResourceStream(uri);
            if (resourceInfo != null)
            {
                using var stream = resourceInfo.Stream;
                return SKBitmap.Decode(stream);
            }
            return null;
        }

        private void MainDrawing_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.StringFormat)
                ? DragDropEffects.Copy
                : DragDropEffects.None;
            e.Handled = true;
        }

        private void MainDrawing_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.StringFormat)
                ? DragDropEffects.Copy
                : DragDropEffects.None;
            e.Handled = true;
        }

    }
}