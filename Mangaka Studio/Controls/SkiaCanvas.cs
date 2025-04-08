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

namespace Mangaka_Studio.Controls
{
    public class SkiaCanvas : SKElement
    {
        private readonly CanvasViewModel canvasViewModel;
        private readonly LayerViewModel layerViewModel;
        private LayerModel layer1;
        private LayerModel layer2;

        public SkiaCanvas()
        {
            this.canvasViewModel = new CanvasViewModel();
            this.layerViewModel = new LayerViewModel(canvasViewModel);
        }

        public SkiaCanvas(CanvasViewModel canvasViewModel, LayerViewModel layerViewModel)
        {
            this.canvasViewModel = canvasViewModel;
            this.layerViewModel = layerViewModel;
            layer1 = new LayerModel
            {
                Id = -1,
                Name = "",
                Surface = SKSurface.Create(new SKImageInfo((int)canvasViewModel.CanvasWidth, (int)canvasViewModel.CanvasHeight, SKColorType.Rgba8888, SKAlphaType.Unpremul))
            };
            layer2 = new LayerModel
            {
                Id = -2,
                Name = "",
                Surface = SKSurface.Create(new SKImageInfo((int)canvasViewModel.CanvasWidth, (int)canvasViewModel.CanvasHeight, SKColorType.Rgba8888, SKAlphaType.Unpremul))
            };
            layer1.Surface.Canvas.Clear(SKColors.Transparent);
            layer2.Surface.Canvas.Clear(SKColors.Transparent);
            this.PaintSurface += OnPaintSurface;
            this.canvasViewModel.PropertyChanged += CanvasViewModel_PropertyChanged;
            this.layerViewModel.PropertyChanged += LayerViewModel_PropertyChanged;
        }

        private void CanvasViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //if (e.PropertyName == nameof(CanvasViewModel.Strokes) || e.PropertyName == nameof(CanvasViewModel.EraserCursor) || e.PropertyName == nameof(CanvasViewModel.CanvasMatrix))
            InvalidateVisual();
        }

        private void LayerViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            bool flag = false;
            
            if (layerViewModel.Layers.Count > 2)
            {
                layer1.Surface.Canvas.Clear(SKColors.Transparent);
                layer2.Surface.Canvas.Clear(SKColors.Transparent);
                foreach (var layer in layerViewModel.Layers)
                {
                    if (layer.IsVisible && layer.Surface != null)
                    {
                        if (layerViewModel.SelectLayer == null)
                        {
                            layerViewModel.SelectLayer = layerViewModel.Layers.FirstOrDefault();
                        }
                        if (layer.Id == layerViewModel.SelectLayer.Id)
                        {
                            flag = true;
                            continue;
                        }
                        else if (!flag)
                        {
                            layer1.Surface.Canvas.DrawSurface(layer.Surface, 0, 0);
                        }
                        else if (flag)
                        {
                            layer2.Surface.Canvas.DrawSurface(layer.Surface, 0, 0);
                        }
                    }
                }
            }
            InvalidateVisual();
        }

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);
            canvas.Scale((float)canvasViewModel.Scale, (float)canvasViewModel.Scale, canvasViewModel.ScalePos.X, canvasViewModel.ScalePos.Y);
            canvas.RotateDegrees((float)canvasViewModel.Rotate, canvasViewModel.CanvasWidth / 2, canvasViewModel.CanvasHeight / 2);
            canvas.Translate(new SKPoint((float)canvasViewModel.OffsetX, (float)canvasViewModel.OffsetY));
            canvas.ClipRect(new SKRect(0, 0, canvasViewModel.CanvasWidth, canvasViewModel.CanvasHeight));

            using (var bgPaint = new SKPaint { Color = SKColors.White })
            {
                canvas.DrawRect(new SKRect(0, 0, canvasViewModel.CanvasWidth, canvasViewModel.CanvasHeight), bgPaint);
            }

            if (layerViewModel.Layers.Count > 2)
            {
                canvas.DrawSurface(layer1.Surface, 0, 0);
                if (layerViewModel.SelectLayer.IsVisible && layerViewModel.SelectLayer.Surface != null)
                {
                    canvas.DrawSurface(layerViewModel.SelectLayer.Surface, 0, 0);
                }
                canvas.DrawSurface(layer2.Surface, 0, 0);
            }
            else
            {
                foreach (var layers in layerViewModel.Layers)
                {
                    if (layers.IsVisible && layers.Surface != null)
                    {
                        canvas.DrawSurface(layers.Surface, 0, 0);
                    }
                }
            }
            if (layerViewModel.tempSurface != null)
            {
                canvas.DrawSurface(layerViewModel.tempSurface, 0, 0);
            }
            if (canvasViewModel.CurrentTool is PipetteTool)
            {
                using (var tempSurface = SKSurface.Create(new SKImageInfo((int)canvasViewModel.CanvasWidth, (int)canvasViewModel.CanvasHeight, SKColorType.Rgba8888, SKAlphaType.Unpremul)))
                {
                    var tempCanvas = tempSurface.Canvas;
                    tempCanvas.Clear(SKColors.Transparent);
                    using (var bgPaint = new SKPaint { Color = SKColors.White })
                    {
                        tempCanvas.DrawRect(new SKRect(0, 0, canvasViewModel.CanvasWidth, canvasViewModel.CanvasHeight), bgPaint);
                    }
                    if (layerViewModel.Layers.Count > 2)
                    {
                        tempCanvas.DrawSurface(layer1.Surface, 0, 0);
                        if (layerViewModel.SelectLayer.IsVisible && layerViewModel.SelectLayer.Surface != null)
                        {
                            tempCanvas.DrawSurface(layerViewModel.SelectLayer.Surface, 0, 0);
                        }
                        tempCanvas.DrawSurface(layer2.Surface, 0, 0);
                    }
                    else
                    {
                        foreach (var layers in layerViewModel.Layers)
                        {
                            if (layers.IsVisible && layers.Surface != null)
                            {
                                tempCanvas.DrawSurface(layers.Surface, 0, 0);
                            }
                        }
                    }

                    layerViewModel.Screenshot = tempSurface.Snapshot();
                }
                if (canvasViewModel.ColorPipette != SKColors.Transparent)
                {
                    using (var pipettePaint = new SKPaint
                    {
                        Color = SKColors.Black,
                        StrokeWidth = 2,
                        Style = SKPaintStyle.Stroke
                    })
                    {
                        SKPoint cursorPos = canvasViewModel.CursorPoint.ToSKPoint();
                        var r = (byte)(255 - canvasViewModel.ColorPipette.Red);
                        var g = (byte)(255 - canvasViewModel.ColorPipette.Green);
                        var b = (byte)(255 - canvasViewModel.ColorPipette.Blue);
                        var invertColor = new SKColor(r, g, b, 255);
                        pipettePaint.Color = invertColor;
                        pipettePaint.StrokeWidth = (Math.Max(2, Math.Min(10, 5 / (float)canvasViewModel.Scale)));
                        canvas.DrawCircle(cursorPos, 22, pipettePaint);
                        canvas.DrawCircle(cursorPos, 18, pipettePaint);
                        pipettePaint.Color = canvasViewModel.ColorPipette;
                        pipettePaint.StrokeWidth = 5;
                        canvas.DrawCircle(cursorPos, 20, pipettePaint);
                    }

                }
                
            }

            if (canvasViewModel.EraserCursor.HasValue)
            {
                using (var eraserPaint = new SKPaint
                {
                    Color = SKColors.Gray,
                    StrokeWidth = 1,
                    Style = SKPaintStyle.Stroke
                })
                {
                    SKPoint cursorPos = canvasViewModel.EraserCursor.Value;
                    canvas.DrawCircle(cursorPos, canvasViewModel.CurrentTool.Settings.StrokeWidth / 2, eraserPaint);
                }
            }

            //DrawGrid(canvas, canvasViewModel.CanvasWidth, canvasViewModel.CanvasHeight, 50);
        }

        private void DrawGrid(SKCanvas canvas, float width, float height, float spacing)
        {
            using (var gridPaint = new SKPaint
            {
                Color = SKColors.Gray,
                StrokeWidth = 1 / (float)canvasViewModel.Scale,
                Style = SKPaintStyle.Stroke
            })
            {
                for (float x = 0; x < width; x += spacing)
                {
                    canvas.DrawLine(x, 0, x, height, gridPaint);
                }

                for (float y = 0; y < width; y += spacing)
                {
                    canvas.DrawLine(0, y, width, y, gridPaint);
                }
            }
        }
    }
}
