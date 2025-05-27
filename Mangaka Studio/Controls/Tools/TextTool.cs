using Mangaka_Studio.Interfaces;
using Mangaka_Studio.Models;
using Mangaka_Studio.ViewModels;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;

namespace Mangaka_Studio.Controls.Tools
{
    public class TextTool : DrawingTools
    {
        public override ToolsSettingsViewModel Settings { get; set; }

        internal DragMode currentDrag = DragMode.None;
        private SKPoint dragStart;
        private SKPoint originalPos;
        private int originalPosId;
        private SKRect originalBounds;
        private float originalRot;
        private bool isModified = false;
        internal DateTime lastClickTime;
        private const int DoubleClickTimeThreshold = 300;

        [JsonConstructor]
        public TextTool(TextToolSettingsViewModel textSettings)
        {
            Settings = textSettings;
        }

        public void Initialize(IFrameContext frame)
        {
            Settings.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Settings.StrokeColor))
                    ApplyColor(Settings.StrokeColor, frame);
                if (e.PropertyName == nameof(Settings.StrokeColor1))
                    ApplyColor1(Settings.StrokeColor1, frame);
                if (e.PropertyName == nameof(Settings.StrokeWidth))
                    ApplyStrokeWidth(Settings.StrokeWidth, frame);
            };
        }

        private void ApplyStrokeWidth(float width, IFrameContext frameViewModel)
        {
            if (frameViewModel.SelectFrame.LayerVM.SelectText != null && frameViewModel.SelectFrame.LayerVM.SelectText.IsSelected)
            {
                frameViewModel.SelectFrame.LayerVM.SelectText.StrokeWidth = width;
                frameViewModel.SelectFrame.LayerVM.OnPropertyChanged(nameof(frameViewModel.SelectFrame.LayerVM.SelectText));
            }
        }

        private void ApplyColor(SKColor fontColor, IFrameContext frameViewModel)
        {
            if (frameViewModel.SelectFrame.LayerVM.SelectText != null && frameViewModel.SelectFrame.LayerVM.SelectText.IsSelected)
            {
                frameViewModel.SelectFrame.LayerVM.SelectText.Color = fontColor;
                frameViewModel.SelectFrame.LayerVM.OnPropertyChanged(nameof(frameViewModel.SelectFrame.LayerVM.SelectText));
            }
        }

        private void ApplyColor1(SKColor fontColor, IFrameContext frameViewModel)
        {
            if (frameViewModel.SelectFrame.LayerVM.SelectText != null && frameViewModel.SelectFrame.LayerVM.SelectText.IsSelected)
            {
                frameViewModel.SelectFrame.LayerVM.SelectText.ColorStroke = fontColor;
                frameViewModel.SelectFrame.LayerVM.OnPropertyChanged(nameof(frameViewModel.SelectFrame.LayerVM.SelectText));
            }
        }

        public override void OnMouseDown(ICanvasContext canvasViewModel, SKPoint pos, IColorPickerContext colorPickerViewModel, IFrameContext frameViewModel, ITextTemplatesContext textTemplatesViewModel)
        {
            frameViewModel.SelectFrame.LayerVM.IsModified = true;
            if (textTemplatesViewModel.TextMode == TextMode.Create)
            {
                if (frameViewModel.SelectFrame.LayerVM.SelectText != null)
                {
                    frameViewModel.SelectFrame.LayerVM.SelectText.IsSelected = false;
                    if (canvasViewModel.IsTextEditor)
                    {
                        canvasViewModel.IsTextEditor = false;
                        canvasViewModel.TextEditor.Visibility = Visibility.Collapsed;
                    }
                }
                if (frameViewModel.SelectFrame.LayerVM.SelectTemplate != null)
                {
                    frameViewModel.SelectFrame.LayerVM.SelectTemplate.IsSelected = false;
                    frameViewModel.SelectFrame.LayerVM.SelectTemplate = null;
                }
                textTemplatesViewModel.TextMode = TextMode.Edit;
                textTemplatesViewModel.TextModeStr = "Редактирование";
                textTemplatesViewModel.IsVisibleMode = "Collapsed";
                frameViewModel.SelectFrame.LayerVM.AddTextCommand.Execute(pos);

                var text = frameViewModel.SelectFrame.LayerVM.SelectText;
                textTemplatesViewModel.SelectedFontFamily = text.FontFamily;
                textTemplatesViewModel.SelectedFontSize = text.FontSize;
                textTemplatesViewModel.FontStyleWeight = text.FontStyleWeight;
                textTemplatesViewModel.FontStyleWidth = text.FontStyleWidth;
                textTemplatesViewModel.FontStyleSlant = text.FontStyleSlant;
                textTemplatesViewModel.IsStroke = text.IsStroke;
                Settings.StrokeWidth = text.StrokeWidth;
                Settings.StrokeColor = text.Color;
                Settings.StrokeColor1 = text.ColorStroke;

                textTemplatesViewModel.IsHitTextDel = true;
                textTemplatesViewModel.OpacityBoundsTextDel = 1f;
            }
            else
            {
                if (frameViewModel.SelectFrame.LayerVM.SelectText != null && frameViewModel.SelectFrame.LayerVM.SelectText.IsSelected)
                {
                    isModified = true;
                    var currentTime = DateTime.Now;
                    var timeDiff = (currentTime - lastClickTime).TotalMilliseconds;

                    if (timeDiff <= DoubleClickTimeThreshold)
                    {
                        canvasViewModel.ShowTextEditor(frameViewModel.SelectFrame.LayerVM.SelectText);
                        return;
                    }

                    lastClickTime = currentTime;

                    var text = frameViewModel.SelectFrame.LayerVM.SelectText;
                    var bounds = text.GetBounds();
                    var handle = text.PointCircle(bounds, canvasViewModel.Scale);
                    SKPoint center = new SKPoint(bounds.MidX, bounds.MidY);
                    float angleDegrees = text.Rotate;
                    float angleRadians = MathF.PI * angleDegrees / 180f;
                    float cos = MathF.Cos(angleRadians);
                    float sin = MathF.Sin(angleRadians);
                    float x = (handle.X - center.X) * cos - (handle.Y - center.Y) * sin + center.X;
                    float y = (handle.X - center.X) * sin + (handle.Y - center.Y) * cos + center.Y;
                    SKPoint rotatedPoint = new SKPoint(x, y);
                    var rotatedBounds = text.RotateRect(bounds, angleDegrees);
                    if (SKPoint.Distance(pos, rotatedPoint) < frameViewModel.SelectFrame.HandleSize / canvasViewModel.Scale)
                    {
                        currentDrag = DragMode.Rotate;
                        dragStart = pos;
                        originalPos = frameViewModel.SelectFrame.LayerVM.SelectText.Position;
                        originalRot = text.Rotate;
                        return;
                    }
                    if (rotatedBounds.Contains(pos.X, pos.Y))
                    {
                        currentDrag = DragMode.MoveAll;
                        dragStart = pos;
                        originalPos = frameViewModel.SelectFrame.LayerVM.SelectText.Position;
                        return;
                    }
                }
                else if (frameViewModel.SelectFrame.LayerVM.SelectTemplate != null && frameViewModel.SelectFrame.LayerVM.SelectTemplate.IsSelected)
                {
                    isModified = true;
                    var template = frameViewModel.SelectFrame.LayerVM.SelectTemplate;
                    SKPoint center = new SKPoint(template.Bounds.MidX, template.Bounds.MidY);
                    List<SKPoint> list = new List<SKPoint>
                    {
                        new SKPoint(template.Bounds.Left, template.Bounds.Top),
                        new SKPoint(template.Bounds.Right, template.Bounds.Top),
                        new SKPoint(template.Bounds.Right, template.Bounds.Bottom),
                        new SKPoint(template.Bounds.Left, template.Bounds.Bottom)
                    };
                    for (var i = 0; i < list.Count; i++)
                    {
                        if (SKPoint.Distance(pos, list[i]) < frameViewModel.SelectFrame.HandleSize / canvasViewModel.Scale)
                        {
                            currentDrag = DragMode.MovePoint;
                            dragStart = pos;
                            originalPosId = i;
                            originalBounds = SKRect.Create(template.Bounds.Location, template.Bounds.Size);
                            return;
                        }
                    }

                    if (template.Bounds.Contains(pos.X, pos.Y))
                    {
                        currentDrag = DragMode.MoveAll;
                        dragStart = pos;
                        originalBounds = SKRect.Create(template.Bounds.Location, template.Bounds.Size);
                        return;
                    }
                }
                List<TextModel> listText = [.. frameViewModel.SelectFrame.LayerVM.SelectLayer.ListText];
                listText.Reverse();
                var select = false;
                foreach (var text in listText)
                {
                    var bounds = text.GetBounds();
                    var handle = text.PointCircle(bounds, canvasViewModel.Scale);
                    SKPoint center = new SKPoint(bounds.MidX, bounds.MidY);
                    float angleDegrees = text.Rotate;
                    var rotatedBounds = text.RotateRect(bounds, angleDegrees);
                    if (rotatedBounds.Contains(pos.X, pos.Y) && !select)
                    {
                        if (frameViewModel.SelectFrame.LayerVM.SelectTemplate != null)
                        {
                            frameViewModel.SelectFrame.LayerVM.SelectTemplate.IsSelected = false;
                            frameViewModel.SelectFrame.LayerVM.SelectTemplate = null;
                        }
                        frameViewModel.SelectFrame.LayerVM.SelectText = text;
                        text.IsSelected = true;
                        textTemplatesViewModel.SelectedFontFamily = text.FontFamily;
                        textTemplatesViewModel.SelectedFontSize = text.FontSize;
                        textTemplatesViewModel.FontStyleWeight = text.FontStyleWeight;
                        textTemplatesViewModel.FontStyleWidth = text.FontStyleWidth;
                        textTemplatesViewModel.FontStyleSlant = text.FontStyleSlant;
                        textTemplatesViewModel.IsStroke = text.IsStroke;
                        Settings.StrokeWidth = text.StrokeWidth;
                        Settings.StrokeColor = text.Color;
                        Settings.StrokeColor1 = text.ColorStroke;
                        select = true;
                        textTemplatesViewModel.IsHitTextDel = true;
                        textTemplatesViewModel.OpacityBoundsTextDel = 1f;
                        if (canvasViewModel.IsTextEditor)
                        {
                            canvasViewModel.IsTextEditor = false;
                            canvasViewModel.TextEditor.Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        text.IsSelected = false;
                    }
                }
                if (!select)
                {
                    frameViewModel.SelectFrame.LayerVM.SelectText = null;
                    if (canvasViewModel.IsTextEditor)
                    {
                        canvasViewModel.IsTextEditor = false;
                        canvasViewModel.TextEditor.Visibility = Visibility.Collapsed;
                    }

                    List<TemplateModel> listTemplate = [.. frameViewModel.SelectFrame.LayerVM.SelectLayer.ListTemplate];
                    listTemplate.Reverse();
                    var selectTemplate = false;
                    foreach (var template in listTemplate)
                    {
                        var bounds = template.Bounds;
                        SKPoint center = new SKPoint(bounds.MidX, bounds.MidY);
                        if (bounds.Contains(pos.X, pos.Y) && !selectTemplate)
                        {
                            frameViewModel.SelectFrame.LayerVM.SelectTemplate = template;
                            template.IsSelected = true;
                            textTemplatesViewModel.IsHitTextDel = true;
                            textTemplatesViewModel.OpacityBoundsTextDel = 1f;
                            selectTemplate = true;
                        }
                        else
                        {
                            template.IsSelected = false;
                        }
                    }
                    if (!selectTemplate)
                    {
                        textTemplatesViewModel.IsHitTextDel = false;
                        textTemplatesViewModel.OpacityBoundsTextDel = 0.5f;
                        frameViewModel.SelectFrame.LayerVM.SelectTemplate = null;
                    }
                }
            }
            if (frameViewModel.SelectFrame.LayerVM.SelectText == null || !frameViewModel.SelectFrame.LayerVM.SelectText.IsSelected)
            {
                textTemplatesViewModel.IsVisibleEditText = "Collapsed";
                textTemplatesViewModel.TextModeStr = "";
            }
            else if (frameViewModel.SelectFrame.LayerVM.SelectText != null || frameViewModel.SelectFrame.LayerVM.SelectText.IsSelected)
            {
                textTemplatesViewModel.IsVisibleEditText = "Visible";
                textTemplatesViewModel.TextModeStr = "Редактирование";
            }
        }

        float Snap(float value, float step)
        {
            return (float)(Math.Round(value / step) * step);
        }

        public override void OnMouseMove(ICanvasContext canvasViewModel, SKPoint pos, IColorPickerContext colorPickerViewModel, IFrameContext frameViewModel, ITextTemplatesContext textTemplatesViewModel)
        {
            if (currentDrag == DragMode.None) return;
            if (frameViewModel.SelectFrame != null && frameViewModel.SelectFrame.LayerVM.SelectText != null && frameViewModel.SelectFrame.LayerVM.SelectText.IsSelected
                || frameViewModel.SelectFrame.LayerVM.SelectTemplate != null && frameViewModel.SelectFrame.LayerVM.SelectTemplate.IsSelected)
            {
                if (isModified)
                {
                    frameViewModel.SelectFrame.LayerVM.SaveState();
                    isModified = false;
                }
                if (currentDrag == DragMode.MoveAll)
                {
                    float dx;
                    float dy;
                    if (canvasViewModel.IsTextPosSnap)
                    {
                        dx = Snap(pos.X - dragStart.X, 10);
                        dy = Snap(pos.Y - dragStart.Y, 10);
                    }
                    else
                    {
                        dx = pos.X - dragStart.X;
                        dy = pos.Y - dragStart.Y;
                    }
                    if (frameViewModel.SelectFrame.LayerVM.SelectText != null)
                    {
                        frameViewModel.SelectFrame.LayerVM.SelectText.Position = new SKPoint(dx, dy) + originalPos;
                    }
                    else
                    {
                        frameViewModel.SelectFrame.LayerVM.SelectTemplate.Bounds = SKRect.Create(originalBounds.Location + new SKPoint(dx, dy), originalBounds.Size);
                    }
                }
                else if (currentDrag == DragMode.Rotate)
                {
                    var text = frameViewModel.SelectFrame.LayerVM.SelectText;
                    var bounds = text.GetBounds();
                    var center = text.Position;

                    var angleStart = MathF.Atan2(dragStart.Y - center.Y, dragStart.X - center.X);
                    var angleNow = MathF.Atan2(pos.Y - center.Y, pos.X - center.X);

                    float deltaAngle = angleNow - angleStart;

                    if (canvasViewModel.IsTextPosSnap)
                    {
                        text.Rotate = Snap(originalRot + deltaAngle * (180f / MathF.PI), 10);
                    }
                    else
                    {
                        text.Rotate = originalRot + deltaAngle * (180f / MathF.PI);
                    }

                }
                else if (currentDrag == DragMode.MovePoint)
                {
                    float dx;
                    float dy;
                    if (canvasViewModel.IsTextPosSnap)
                    {
                        dx = Snap(pos.X - dragStart.X, 10);
                        dy = Snap(pos.Y - dragStart.Y, 10);
                    }
                    else
                    {
                        dx = pos.X - dragStart.X;
                        dy = pos.Y - dragStart.Y;
                    }
                    if (originalPosId == 0)
                    {
                        var bounds = new SKRect 
                        {
                            Left = originalBounds.Left + dx,
                            Right = originalBounds.Right,
                            Top = originalBounds.Top + dy,
                            Bottom = originalBounds.Bottom,
                        };
                        if (bounds.Width <= 0 || bounds.Height <= 0) return;
                        frameViewModel.SelectFrame.LayerVM.SelectTemplate.Bounds = bounds;
                    }
                    else if (originalPosId == 1)
                    {
                        var bounds = new SKRect
                        {
                            Left = originalBounds.Left,
                            Right = originalBounds.Right + dx,
                            Top = originalBounds.Top + dy,
                            Bottom = originalBounds.Bottom,
                        };
                        if (bounds.Width <= 0 || bounds.Height <= 0) return;
                        frameViewModel.SelectFrame.LayerVM.SelectTemplate.Bounds = bounds;
                    }
                    else if (originalPosId == 2)
                    {
                        var bounds = new SKRect
                        {
                            Left = originalBounds.Left,
                            Right = originalBounds.Right + dx,
                            Top = originalBounds.Top,
                            Bottom = originalBounds.Bottom + dy,
                        };
                        if (bounds.Width <= 0 || bounds.Height <= 0) return;
                        frameViewModel.SelectFrame.LayerVM.SelectTemplate.Bounds = bounds;
                    }
                    else if (originalPosId == 3)
                    {
                        var bounds = new SKRect
                        {
                            Left = originalBounds.Left + dx,
                            Right = originalBounds.Right,
                            Top = originalBounds.Top,
                            Bottom = originalBounds.Bottom + dy,
                        };
                        if (bounds.Width <= 0 || bounds.Height <= 0) return;
                        frameViewModel.SelectFrame.LayerVM.SelectTemplate.Bounds = bounds;
                    }
                }
                canvasViewModel.NotifyEraserCursorChanged();
            }
        }

        public override void OnMouseUp(ICanvasContext canvasViewModel, IFrameContext frameViewModel)
        {
            currentDrag = DragMode.None;
        }
    }
}
