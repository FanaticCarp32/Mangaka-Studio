using SkiaSharp.Views.Desktop;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows;
using Mangaka_Studio.ViewModels;
using System.ComponentModel;
using System.Windows.Media;
using Mangaka_Studio.Models;
using System.Windows.Controls.Primitives;
using static System.Net.Mime.MediaTypeNames;
using Mangaka_Studio.Interfaces;
using Mangaka_Studio.Controls.Tools;

namespace Mangaka_Studio.Controls.Renders
{
    public class SkiaCanvas : SKElement
    {
        private readonly ICanvasContext canvasContext;
        private readonly FrameViewModel frameViewModel;
        private LayerViewModel layerViewModel;
        private TextTemplatesViewModel textTemplatesViewModel;
        private LayerModel layer1;
        private LayerModel layer2;
        private SKSurface surfaceLayer;
        private SKImage imageFrame;
        private Renderer renderer;

        public SkiaCanvas()
        {
        }

        public SkiaCanvas(FrameViewModel frameViewModel, TextTemplatesViewModel textTemplatesViewModel, IFrameContext frameContext, ICanvasContext canvasContext)
        {
            this.canvasContext = canvasContext;
            this.frameViewModel = frameViewModel;
            renderer = new Renderer(frameContext, canvasContext);
            layer1 = new LayerModel
            {
                Id = -1,
                Name = "",
            };
            layer2 = new LayerModel
            {
                Id = -2,
                Name = "",
            };
            var imageInfo = new SKImageInfo(canvasContext.CanvasWidth, canvasContext.CanvasHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
            using (var surface = SKSurface.Create(imageInfo))
            {
                surface.Canvas.Clear(SKColors.Transparent);
                layer1.Image = surface.Snapshot();
                layer2.Image = surface.Snapshot();
                imageFrame = surface.Snapshot();
                surface.Dispose();
            }
            surfaceLayer = SKSurface.Create(new SKImageInfo(canvasContext.CanvasWidth, canvasContext.CanvasHeight, SKColorType.Rgba8888, SKAlphaType.Premul));
            surfaceLayer.Canvas.Clear(SKColors.Transparent);
            PaintSurface += OnPaintSurface;
            canvasContext.PropertyChanged += CanvasViewModel_PropertyChanged;
            SubscribeLayer(frameViewModel.SelectFrame.LayerVM);
            this.frameViewModel.PropertyChanged += FrameViewModel_PropertyChanged;
            this.textTemplatesViewModel = textTemplatesViewModel;
        }

        private void SubscribeLayer(LayerViewModel layerViewModel)
        {
            if (this.layerViewModel != null)
            {
                WeakEventManager<LayerViewModel, PropertyChangedEventArgs>.RemoveHandler(this.layerViewModel, nameof(LayerViewModel.PropertyChanged), LayerViewModel_PropertyChanged);
            }
            this.layerViewModel = layerViewModel;
            if (this.layerViewModel != null)
            {
                WeakEventManager<LayerViewModel, PropertyChangedEventArgs>.AddHandler(this.layerViewModel, nameof(LayerViewModel.PropertyChanged), LayerViewModel_PropertyChanged);
            }
        }



        private void FrameViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FrameViewModel.IsExpanded) || e.PropertyName == nameof(FrameViewModel.IsHit)
                 || e.PropertyName == nameof(FrameViewModel.OpacityBounds))
            {
                return;
            }
            if (e.PropertyName == nameof(FrameViewModel.SelectFrame))
            {
                SubscribeLayer(frameViewModel.SelectFrame.LayerVM);
            }
            if (e.PropertyName == nameof(FrameViewModel.BoundsWidthVM) || e.PropertyName == nameof(FrameViewModel.IsSelectedVM)
                || e.PropertyName == nameof(FrameViewModel.IsDrawBoundsVM))
            {
                InvalidateVisual();
                return;
            }

            textTemplatesViewModel.IsVisibleEditText = "Collapsed";
            textTemplatesViewModel.TextModeStr = "";
            textTemplatesViewModel.IsHitTextDel = false;
            textTemplatesViewModel.OpacityBoundsTextDel = 0.5f;
            imageFrame = renderer.RebuildFrames(imageFrame);
            renderer.RebuildLayers(surfaceLayer, layer1, layer2);
            InvalidateVisual();
        }

        private void CanvasViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CanvasViewModel.CursorPoint)) return;
            if (e.PropertyName == nameof(CanvasViewModel.CurrentTool))
            {
                if (frameViewModel.SelectFrame.LayerVM.baseSurface != null)
                {
                    frameViewModel.SelectFrame.LayerVM.SelectLayer.Image = frameViewModel.SelectFrame.LayerVM.baseSurface.Snapshot();
                    renderer.RebuildLayers(surfaceLayer, layer1, layer2);
                }
                if (frameViewModel.SelectFrame.LayerVM.SelectText != null)
                {
                    frameViewModel.SelectFrame.LayerVM.SelectText.IsSelected = false;
                    textTemplatesViewModel.IsVisibleEditText = "Collapsed";
                    textTemplatesViewModel.TextModeStr = "";
                    textTemplatesViewModel.IsHitTextDel = false;
                    textTemplatesViewModel.OpacityBoundsTextDel = 0.5f;
                    frameViewModel.SelectFrame.LayerVM.SelectText = null;
                }
                if (frameViewModel.SelectFrame.LayerVM.SelectTemplate != null)
                {
                    frameViewModel.SelectFrame.LayerVM.SelectTemplate.IsSelected = false;
                    textTemplatesViewModel.IsHitTextDel = false;
                    textTemplatesViewModel.OpacityBoundsTextDel = 0.5f;
                    frameViewModel.SelectFrame.LayerVM.SelectTemplate = null;
                }
                InvalidateVisual();
            }
            if (e.PropertyName == nameof(CanvasViewModel.Scale) || e.PropertyName == nameof(CanvasViewModel.Rotate)
                || e.PropertyName == nameof(CanvasViewModel.OffsetX) || e.PropertyName == nameof(CanvasViewModel.OffsetY)
                || e.PropertyName == nameof(CanvasViewModel.CanvasHeight) || e.PropertyName == nameof(CanvasViewModel.CanvasWidth)
                || e.PropertyName == nameof(CanvasViewModel.IsGrid) || e.PropertyName == nameof(CanvasViewModel.GridSize)
                || e.PropertyName == nameof(CanvasViewModel.EraserCursor) || e.PropertyName == nameof(CanvasViewModel.ColorPipette)
                || e.PropertyName == nameof(CanvasViewModel.IsTextEditor))
            {
                if (frameViewModel.SelectFrame.LayerVM.SelectText != null && frameViewModel.SelectFrame.LayerVM.SelectText.IsSelected && canvasContext.IsTextEditor)
                {
                    var text = frameViewModel.SelectFrame.LayerVM.SelectText;
                    var bounds = text.GetBounds();
                    var canvasPoint = new SKPoint(bounds.Left, bounds.Top);
                    var screenPoint = canvasContext.GetScreenPoint(canvasPoint, this);

                    Canvas.SetLeft(canvasContext.TextEditor, screenPoint.X);
                    Canvas.SetTop(canvasContext.TextEditor, screenPoint.Y);
                    canvasContext.TextEditor.FontStyle = text.SlantEnum switch
                    {
                        SKFontStyleSlant.Italic => FontStyles.Italic,
                        SKFontStyleSlant.Oblique => FontStyles.Oblique,
                        _ => FontStyles.Normal
                    };
                    canvasContext.TextEditor.FontWeight = FontWeight.FromOpenTypeWeight(text.FontStyleWeight);
                    canvasContext.TextEditor.FontStretch = text.FontStyleWidth switch
                    {
                        <= 3 => FontStretches.Condensed,
                        >= 7 => FontStretches.Expanded,
                        _ => FontStretches.Normal
                    };
                    canvasContext.TextEditor.RenderTransformOrigin = new Point(0.5, 0.5);
                    canvasContext.TextEditor.RenderTransform = new RotateTransform(text.Rotate);
                    canvasContext.TextEditor.FontSize = text.FontSize * canvasContext.Scale;
                    canvasContext.TextEditor.Width = (bounds.Width) * canvasContext.Scale;
                    canvasContext.TextEditor.Height = (bounds.Height) * canvasContext.Scale;
                }
                InvalidateVisual();
            }
        }

        private void LayerViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LayerViewModel.SelectText))
            {
                if (frameViewModel.SelectFrame.LayerVM.SelectText != null && frameViewModel.SelectFrame.LayerVM.SelectText.IsSelected && canvasContext.IsTextEditor)
                {
                    var text = frameViewModel.SelectFrame.LayerVM.SelectText;
                    var bounds = text.GetBounds();
                    var canvasPoint = new SKPoint(bounds.Left, bounds.Top);
                    var screenPoint = canvasContext.GetScreenPoint(canvasPoint, this);

                    Canvas.SetLeft(canvasContext.TextEditor, screenPoint.X);
                    Canvas.SetTop(canvasContext.TextEditor, screenPoint.Y);
                    canvasContext.TextEditor.FontStyle = text.SlantEnum switch
                    {
                        SKFontStyleSlant.Italic => FontStyles.Italic,
                        SKFontStyleSlant.Oblique => FontStyles.Oblique,
                        _ => FontStyles.Normal
                    };
                    canvasContext.TextEditor.FontWeight = FontWeight.FromOpenTypeWeight(text.FontStyleWeight);
                    canvasContext.TextEditor.FontStretch = text.FontStyleWidth switch
                    {
                        <= 3 => FontStretches.Condensed,
                        >= 7 => FontStretches.Expanded,
                        _ => FontStretches.Normal
                    };
                    canvasContext.TextEditor.RenderTransformOrigin = new Point(0.5, 0.5);
                    canvasContext.TextEditor.RenderTransform = new RotateTransform(text.Rotate);
                    canvasContext.TextEditor.FontSize = text.FontSize * canvasContext.Scale;
                    canvasContext.TextEditor.Width = (bounds.Width + 5) * canvasContext.Scale;
                    canvasContext.TextEditor.Height = (bounds.Height + 5) * canvasContext.Scale;
                }
                InvalidateVisual();
                return;
            }
            if (e.PropertyName == nameof(layerViewModel.SelectTemplate))
            {
                InvalidateVisual();
                return;
            }
            if (frameViewModel.SelectFrame.LayerVM.SelectText != null)
            {
                textTemplatesViewModel.IsVisibleEditText = "Collapsed";
                textTemplatesViewModel.TextModeStr = "";
                textTemplatesViewModel.IsHitTextDel = false;
                textTemplatesViewModel.OpacityBoundsTextDel = 0.5f;
                frameViewModel.SelectFrame.LayerVM.SelectText.IsSelected = false;
                frameViewModel.SelectFrame.LayerVM.SelectText = null;
            }
            if (frameViewModel.SelectFrame.LayerVM.SelectTemplate != null)
            {
                textTemplatesViewModel.IsHitTextDel = false;
                textTemplatesViewModel.OpacityBoundsTextDel = 0.5f;
                frameViewModel.SelectFrame.LayerVM.SelectTemplate.IsSelected = false;
                frameViewModel.SelectFrame.LayerVM.SelectTemplate = null;
            }
            renderer.RebuildLayers(surfaceLayer, layer1, layer2);
            
            InvalidateVisual();
        }

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);
            canvas.Scale((float)canvasContext.Scale, (float)canvasContext.Scale, canvasContext.ScalePos.X, canvasContext.ScalePos.Y);
            canvas.RotateDegrees((float)canvasContext.Rotate, canvasContext.CanvasWidth / 2, canvasContext.CanvasHeight / 2);
            canvas.Translate(new SKPoint((float)canvasContext.OffsetX, (float)canvasContext.OffsetY));

            using (var bgPaint = new SKPaint { Color = SKColors.White, IsAntialias = true })
            {
                canvas.DrawRect(new SKRect(0, 0, canvasContext.CanvasWidth, canvasContext.CanvasHeight), bgPaint);
            }

            if (!frameViewModel.GeneralView)
            {
                using (var framePaint = new SKPaint { Color = new SKColor(0, 0, 0, 64) })
                {
                    canvas.DrawImage(imageFrame, 0, 0, framePaint);
                }
            }
            else
            {
                canvas.DrawImage(imageFrame, 0, 0);
            }

            canvas.Save();
            if (canvasContext.CurrentTool is PipetteTool && frameViewModel.SelectFrame.LayerVM.Screenshot != null)
            {
                canvas.DrawImage(frameViewModel.SelectFrame.LayerVM.Screenshot, 0, 0);
                DrawPipette(canvas);
            }
            else if (frameViewModel.SelectFrame.IsVisible)
            {
                canvas.ClipPath(frameViewModel.SelectFrame.Bounds);
                canvas.DrawImage(layer1.Image, 0, 0);
                var selected = frameViewModel.SelectFrame.LayerVM.SelectLayer;
                if (selected != null)
                {
                    if (canvasContext.CurrentTool is EraserTool)
                    {
                        if (frameViewModel.SelectFrame.LayerVM.baseSurface != null && frameViewModel.SelectFrame.LayerVM.SelectLayer.IsVisible)
                        {
                            canvas.DrawSurface(frameViewModel.SelectFrame.LayerVM.baseSurface, 0, 0);
                        }
                    }
                    else
                    {
                        if (frameViewModel.SelectFrame.LayerVM.SelectLayer.IsVisible)
                        {
                            canvas.DrawImage(frameViewModel.SelectFrame.LayerVM.SelectLayer.Image, 0, 0);
                        }

                        if (frameViewModel.SelectFrame.LayerVM.tempSurface != null && frameViewModel.SelectFrame.LayerVM.SelectLayer.IsVisible)
                        {
                            canvas.DrawSurface(frameViewModel.SelectFrame.LayerVM.tempSurface, 0, 0);
                        }
                    }
                    if (frameViewModel.SelectFrame.LayerVM.SelectLayer.IsVisible)
                    {
                        foreach (var template in frameViewModel.SelectFrame.LayerVM.SelectLayer.ListTemplate)
                        {
                            if (canvasContext.CurrentTool is TextTool)
                            {
                                template.DrawCurrentLayer(canvas, canvasContext.Scale);
                            }
                            else
                            {
                                template.Draw(canvas, canvasContext.Scale);
                            }
                            if (template.IsSelected)
                            {
                                DrawHandles(canvas, template.Bounds, FrameMode.Rectangle);
                            }
                        }
                        foreach (var text in frameViewModel.SelectFrame.LayerVM.SelectLayer.ListText)
                        {
                            if (canvasContext.IsTextEditor && text.IsSelected)
                            {
                                text.DrawTextBox(canvas, canvasContext.Scale);
                            }
                            else if (canvasContext.CurrentTool is TextTool)
                            {
                                text.DrawCurrentLayer(canvas, canvasContext.Scale);
                            }
                            else
                            {
                                text.Draw(canvas, canvasContext.Scale);
                            }
                        }
                    }
                }
                
                canvas.DrawImage(layer2.Image, 0, 0);
                if (frameViewModel.IsDrawBoundsVM)
                {
                    using (var boundsPaint = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = frameViewModel.SelectFrame.BoundsWidth, Color = SKColors.Black, IsAntialias = true })
                    {
                        canvas.DrawPath(frameViewModel.SelectFrame.Bounds, boundsPaint);
                    }
                }
            }
            canvas.Restore();
            if (frameViewModel.SelectFrame != null && frameViewModel.SelectFrame.IsSelected)
            {
                DrawHandles(canvas, frameViewModel.SelectFrame.Bounds, frameViewModel.SelectFrame.FrameMode);
            }
            if (canvasContext.EraserCursor.HasValue)
            {
                using (var eraserPaint = new SKPaint
                {
                    Color = SKColors.Gray,
                    StrokeWidth = 1,
                    Style = SKPaintStyle.Stroke
                })
                {
                    SKPoint cursorPos = canvasContext.EraserCursor.Value;
                    canvas.DrawCircle(cursorPos, canvasContext.CurrentTool.Settings.StrokeWidth / 2, eraserPaint);
                }
            }
            if (canvasContext.IsGrid)
                DrawGrid(canvas, canvasContext.CanvasWidth, canvasContext.CanvasHeight, canvasContext.GridSize);
        }

        private void DrawPipette(SKCanvas sKCanvas)
        {
            if (canvasContext.ColorPipette != SKColors.Transparent)
            {
                SKPoint cursorPos = canvasContext.CursorPoint.ToSKPoint();
                var r = (byte)(255 - canvasContext.ColorPipette.Red);
                var g = (byte)(255 - canvasContext.ColorPipette.Green);
                var b = (byte)(255 - canvasContext.ColorPipette.Blue);
                var invertColor = new SKColor(r, g, b, 255);
                using (var pipettePaint = new SKPaint
                {
                    Color = invertColor,
                    StrokeWidth = Math.Max(2, Math.Min(10, 5 / (float)canvasContext.Scale)),
                    Style = SKPaintStyle.Stroke
                })
                {
                    sKCanvas.DrawCircle(cursorPos, 22, pipettePaint);
                    sKCanvas.DrawCircle(cursorPos, 18, pipettePaint);
                    pipettePaint.Color = canvasContext.ColorPipette;
                    pipettePaint.StrokeWidth = 5;
                    sKCanvas.DrawCircle(cursorPos, 20, pipettePaint);
                }
            }
        }

        private void DrawGrid(SKCanvas canvas, float width, float height, float spacing)
        {
            using (var gridPaint = new SKPaint
            {
                Color = SKColors.Gray,
                StrokeWidth = 1 / (float)canvasContext.Scale,
                Style = SKPaintStyle.Stroke
            })
            {
                for (float x = 0; x < width; x += spacing)
                {
                    canvas.DrawLine(x, 0, x, height, gridPaint);
                }

                for (float y = 0; y < height; y += spacing)
                {
                    canvas.DrawLine(0, y, width, y, gridPaint);
                }
            }
        }

        private void DrawHandles(SKCanvas canvas, SKPath bound, FrameMode mode)
        {
            using var paint = new SKPaint
            {
                Color = SKColors.Blue,
                StrokeWidth = 2 / (float)canvasContext.Scale,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            };
            canvas.DrawPath(bound, paint);
            float handleSize = frameViewModel.SelectFrame.HandleSize / (float)canvasContext.Scale;
            using var handlePaint = new SKPaint
            {
                Color = SKColors.White,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };
            var points = frameViewModel.SelectFrame.Bounds.Points;
            foreach (var p in points)
            {
                canvas.DrawCircle(p, handleSize, handlePaint);
                canvas.DrawCircle(p, handleSize, paint);
            }
        }

        private void DrawHandles(SKCanvas canvas, SKRect bound, FrameMode mode)
        {
            using var paint = new SKPaint
            {
                Color = SKColors.Blue,
                StrokeWidth = 2 / (float)canvasContext.Scale,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            };
            canvas.DrawRect(bound, paint);
            float handleSize = frameViewModel.SelectFrame.HandleSize / (float)canvasContext.Scale;
            using var handlePaint = new SKPaint
            {
                Color = SKColors.White,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };
            List<SKPoint> list = new List<SKPoint>
            {
                new SKPoint(bound.Left, bound.Top),
                new SKPoint(bound.Right, bound.Top),
                new SKPoint(bound.Right, bound.Bottom),
                new SKPoint(bound.Left, bound.Bottom)
            };
            foreach (var p in list)
            {
                canvas.DrawCircle(p, handleSize, handlePaint);
                canvas.DrawCircle(p, handleSize, paint);
            }
        }
    }
}
