using Mangaka_Studio.Interfaces;
using Mangaka_Studio.Models;
using Mangaka_Studio.ViewModels;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mangaka_Studio.Controls.Renders
{
    public class Renderer
    {
        private readonly IFrameContext frameContext;
        private readonly ICanvasContext canvasContext;
        public Renderer(IFrameContext frameContext, ICanvasContext canvasContext)
        {
            this.frameContext = frameContext;
            this.canvasContext = canvasContext;
        }
        public SKImage RebuildFrames(SKImage imageFrame)
        {
            var frameSurface = SKSurface.Create(new SKImageInfo(canvasContext.CanvasWidth, canvasContext.CanvasHeight, SKColorType.Rgba8888, SKAlphaType.Premul));
            frameSurface.Canvas.Clear(SKColors.Transparent);
            var selectedId = frameContext.SelectFrame?.Id;
            foreach (var frame in frameContext.Frames)
            {
                if (!frame.IsVisible || frame.LayerVM.Layers.Count == 0)
                    continue;
                if (frame.Id == selectedId)
                {
                    continue;
                }
                var tempSurface = SKSurface.Create(new SKImageInfo((int)frame.Bounds.Bounds.Width, (int)frame.Bounds.Bounds.Height, SKColorType.Rgba8888, SKAlphaType.Premul));
                tempSurface.Canvas.Clear(SKColors.Transparent);
                tempSurface.Canvas.Translate(-frame.Bounds.Bounds.Left, -frame.Bounds.Bounds.Top);
                tempSurface.Canvas.ClipPath(frame.Bounds);
                tempSurface.Canvas.DrawImage(frame.LayerVM.GetCompositedImage(), 0, 0);
                //if (frame.Id != 0)
                {
                    if (frame.IsDrawBounds)
                    {
                        using (var boundsPaint = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = frame.BoundsWidth, Color = SKColors.Black, IsAntialias = true })
                        {
                            tempSurface.Canvas.DrawPath(frame.Bounds, boundsPaint);
                        }
                    }
                }
                frameSurface.Canvas.DrawSurface(tempSurface, frame.Bounds.Bounds.Left, frame.Bounds.Bounds.Top);
                tempSurface.Dispose();
            }
            imageFrame = frameSurface.Snapshot();
            frameSurface.Dispose();
            return imageFrame;
        }

        public void RebuildLayers(SKSurface surfaceLayer, LayerModel layer1, LayerModel layer2)
        {
            var screen = surfaceLayer.Snapshot();
            if (screen.Info.Width != canvasContext.CanvasWidth || screen.Info.Height != canvasContext.CanvasHeight)
            {
                surfaceLayer = SKSurface.Create(new SKImageInfo(canvasContext.CanvasWidth, canvasContext.CanvasHeight, SKColorType.Rgba8888, SKAlphaType.Premul));
                surfaceLayer.Canvas.Clear(SKColors.Transparent);
                screen = surfaceLayer.Snapshot();
                layer1.Image = screen;
                layer2.Image = screen;
            }
            surfaceLayer.Canvas.Clear(SKColors.White);
            var layerSurface = SKSurface.Create(layer1.Image.Info);
            layerSurface.Canvas.Clear(SKColors.Transparent);
            var selectedId = frameContext.SelectFrame.LayerVM.SelectLayer?.Id;
            foreach (var layer in frameContext.SelectFrame.LayerVM.Layers)
            {
                if (!layer.IsVisible || layer.Image == null)
                    continue;
                if (layer.Id == selectedId)
                {
                    frameContext.SelectFrame.LayerVM.SelectLayer.IsVisible = true;
                    layer1.Image = layerSurface.Snapshot();
                    surfaceLayer.Canvas.DrawSurface(layerSurface, 0, 0);
                    surfaceLayer.Canvas.DrawImage(frameContext.SelectFrame.LayerVM.SelectLayer?.Image, 0, 0);
                    layerSurface.Canvas.Clear(SKColors.Transparent);
                    continue;
                }
                layerSurface.Canvas.DrawImage(layer.Image, 0, 0);
                foreach (var bubble in layer.ListTemplate)
                {
                    bubble.Draw(layerSurface.Canvas, canvasContext.Scale);
                }
                foreach (var text in layer.ListText)
                {
                    text.Draw(layerSurface.Canvas, canvasContext.Scale);
                }
            }
            layer2.Image = layerSurface.Snapshot();
            surfaceLayer.Canvas.DrawSurface(layerSurface, 0, 0);
            frameContext.SelectFrame.LayerVM.Screenshot = surfaceLayer.Snapshot();
            layerSurface.Dispose();
        }
    }
}
