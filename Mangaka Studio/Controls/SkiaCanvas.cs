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
        private SKSurface surfaceLayer;

        public SkiaCanvas()
        {
        }

        public SkiaCanvas(CanvasViewModel canvasViewModel, LayerViewModel layerViewModel)
        {
            this.canvasViewModel = canvasViewModel;
            this.layerViewModel = layerViewModel;
            layer1 = new LayerModel
            {
                Id = -1,
                Name = "",
                //Image = SKImage.Create(new SKImageInfo((int)canvasViewModel.CanvasWidth, (int)canvasViewModel.CanvasHeight, SKColorType.Rgba8888, SKAlphaType.Premul))
            };
            layer2 = new LayerModel
            {
                Id = -2,
                Name = "",
                //Image = SKImage.Create(new SKImageInfo((int)canvasViewModel.CanvasWidth, (int)canvasViewModel.CanvasHeight, SKColorType.Rgba8888, SKAlphaType.Premul))
            };
            var imageInfo = new SKImageInfo((int)canvasViewModel.CanvasWidth, (int)canvasViewModel.CanvasHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
            using (var surface = SKSurface.Create(imageInfo))
            {
                surface.Canvas.Clear(SKColors.Transparent);
                layer1.Image = surface.Snapshot();
                layer2.Image = surface.Snapshot();
                surface.Dispose();
            }
            surfaceLayer = SKSurface.Create(new SKImageInfo((int)canvasViewModel.CanvasWidth, (int)canvasViewModel.CanvasHeight, SKColorType.Rgba8888, SKAlphaType.Premul));
            surfaceLayer.Canvas.Clear(SKColors.Transparent);
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
            //layerViewModel.SelectLayer.Image = layerViewModel.tempSurface.Snapshot();
            //layerViewModel.tempSurface.Canvas.Clear(SKColors.Transparent);
            RebuildLayers();
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

            using (var bgPaint = new SKPaint { Color = SKColors.White, IsAntialias = true })
            {
                canvas.DrawRect(new SKRect(0, 0, canvasViewModel.CanvasWidth, canvasViewModel.CanvasHeight), bgPaint);
            }

            if (layerViewModel.NeedsRedraw)
            {
                var temp = surfaceLayer.Canvas;
                temp.Clear(SKColors.Transparent);
                temp.DrawImage(layer1.Image, 0, 0);
                var selected = layerViewModel.SelectLayer;
                if (selected != null)
                {
                    if (selected.IsVisible && selected.Image != null)
                    {
                        temp.DrawImage(selected.Image, 0, 0);
                    }
                }
                temp.DrawImage(layer2.Image, 0, 0);
                
                layerViewModel.NeedsRedraw = false;
            }

            canvas.DrawSurface(surfaceLayer, 0, 0);

            if (layerViewModel.tempSurface != null)
            {
                canvas.DrawSurface(layerViewModel.tempSurface, 0, 0);
            }
            if (canvasViewModel.CurrentTool is PipetteTool)
            {
                DrawPipette(canvas);
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
            if (canvasViewModel.IsGrid)
                DrawGrid(canvas, canvasViewModel.CanvasWidth, canvasViewModel.CanvasHeight, canvasViewModel.GridSize);
        }

        private void RebuildLayers()
        {
            layerViewModel.NeedsRedraw = true;
            surfaceLayer.Canvas.Clear(SKColors.Transparent);
            surfaceLayer = SKSurface.Create(new SKImageInfo((int)canvasViewModel.CanvasWidth, (int)canvasViewModel.CanvasHeight, SKColorType.Rgba8888, SKAlphaType.Premul));
            surfaceLayer.Canvas.Clear(SKColors.Transparent);
            var imageInfo = new SKImageInfo((int)canvasViewModel.CanvasWidth, (int)canvasViewModel.CanvasHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
            using (var surface = SKSurface.Create(imageInfo))
            {
                surface.Canvas.Clear(SKColors.Transparent);
                layer1.Image = surface.Snapshot();
                layer2.Image = surface.Snapshot();
                surface.Dispose();
            }
            //layer1.Image = SKImage.Create(new SKImageInfo((int)canvasViewModel.CanvasWidth, (int)canvasViewModel.CanvasHeight, SKColorType.Rgba8888, SKAlphaType.Premul));
            //layer2.Image = SKImage.Create(new SKImageInfo((int)canvasViewModel.CanvasWidth, (int)canvasViewModel.CanvasHeight, SKColorType.Rgba8888, SKAlphaType.Premul));
            var layer1Surface = SKSurface.Create(layer1.Image.Info);
            var layer2Surface = SKSurface.Create(layer1.Image.Info);
            layer1Surface.Canvas.Clear(SKColors.Transparent);
            layer2Surface.Canvas.Clear(SKColors.Transparent);
            bool isAfterSelected = false;
            var selectedId = layerViewModel.SelectLayer?.Id;
            foreach (var layer in layerViewModel.Layers)
            {
                if (!layer.IsVisible || layer.Image == null)
                    continue;
                if (layer.Id == selectedId)
                {
                    isAfterSelected = true;
                    continue;
                }
                    
                if (!isAfterSelected)
                {
                    layer1Surface.Canvas.DrawImage(layer.Image, 0, 0);
                }
                else if (isAfterSelected)
                {
                    layer2Surface.Canvas.DrawImage(layer.Image, 0, 0);
                }
            }
            layer1.Image = layer1Surface.Snapshot();
            layer2.Image = layer2Surface.Snapshot();
            layer1Surface.Dispose();
            layer2Surface.Dispose();
        }

        private void DrawPipette(SKCanvas sKCanvas)
        {
            layerViewModel.Screenshot = surfaceLayer.Snapshot();
            if (canvasViewModel.ColorPipette != SKColors.Transparent)
            {
                SKPoint cursorPos = canvasViewModel.CursorPoint.ToSKPoint();
                var r = (byte)(255 - canvasViewModel.ColorPipette.Red);
                var g = (byte)(255 - canvasViewModel.ColorPipette.Green);
                var b = (byte)(255 - canvasViewModel.ColorPipette.Blue);
                var invertColor = new SKColor(r, g, b, 255);
                using (var pipettePaint = new SKPaint
                {
                    Color = invertColor,
                    StrokeWidth = Math.Max(2, Math.Min(10, 5 / (float)canvasViewModel.Scale)),
                    Style = SKPaintStyle.Stroke
                })
                {
                    sKCanvas.DrawCircle(cursorPos, 22, pipettePaint);
                    sKCanvas.DrawCircle(cursorPos, 18, pipettePaint);
                    pipettePaint.Color = canvasViewModel.ColorPipette;
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
