using Mangaka_Studio.Controls.Renders;
using Mangaka_Studio.Interfaces;
using Mangaka_Studio.Models;
using Mangaka_Studio.ViewModels;
using Moq;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;

namespace Mangaka_Studio_Test
{
    public class UnitTestRenderer
    {
        private Mock<ICanvasContext> mockCanvas;
        private Mock<IColorPickerContext> mockColorPicker;
        private Mock<IFrameContext> mockFrame;
        private Mock<ITextTemplatesContext> mockTextTemplates;
        private Mock<ICommand> mockCommand;
        private FrameLayerModel frameLayer;

        public UnitTestRenderer()
        {
            var textSettings = new TextToolSettingsViewModel();

            mockCanvas = new Mock<ICanvasContext>();
            mockColorPicker = new Mock<IColorPickerContext>();
            mockFrame = new Mock<IFrameContext>();
            mockTextTemplates = new Mock<ITextTemplatesContext>();

            mockCanvas.SetupProperty(c => c.ColorPipette);
            //mockTextTemplates.SetupGet(c => c.SetCreateModeCommand).Returns(mockCommand.Object);
            mockCanvas.SetupGet(c => c.CanvasWidth).Returns(100);
            mockCanvas.SetupGet(c => c.CanvasHeight).Returns(100);
        }

        [Fact]
        public void RebuildFrames_DrawsVisibleFrames()
        {
            // Arrange
            var bounds = new SKPath();
            var bounds1 = new SKPath();
            bounds.AddRect(new SKRect(0, 0, 100, 100));
            bounds1.AddRect(new SKRect(0, 0, 50, 50));
            var frames = new ObservableCollection<FrameLayerModel>
            ([
                new FrameLayerModel(
                    new FrameModel
                    {
                        Bounds = bounds,
                        Id = 0,
                        IsVisible = true,
                        Name = ""

                    }, mockCanvas.Object)
                {
                    IsDrawBounds = true,
                    LayerVM = new LayerViewModel(mockCanvas.Object)
                    {
                        Layers = new ObservableCollection<LayerModel> { new LayerModel() }
                    }
                },
                new FrameLayerModel(
                    new FrameModel
                    {
                        Bounds = bounds1,
                        Id = 1,
                        IsVisible = true,
                        Name = ""

                    }, mockCanvas.Object)
                {
                    IsDrawBounds = true,
                    LayerVM = new LayerViewModel(mockCanvas.Object)
                    {
                        Layers = new ObservableCollection<LayerModel> { new LayerModel() }
                    }
                }
            ]);
            var imageLayer = SKImage.Create(new SKImageInfo(mockCanvas.Object.CanvasWidth, mockCanvas.Object.CanvasHeight));
            using var surface = SKSurface.Create(new SKImageInfo(mockCanvas.Object.CanvasWidth, mockCanvas.Object.CanvasHeight));
            using var paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColors.Black
            };
            surface.Canvas.DrawRect(new SKRect(0, 0, 11, 11), paint);
            imageLayer = surface.Snapshot();
            frames[0].LayerVM.Layers.Clear();
            frames[0].LayerVM.Layers.Add(new LayerModel
            {
                Image = imageLayer
            });
            mockFrame.Setup(f => f.Frames).Returns(frames);
            mockFrame.Setup(f => f.SelectFrame).Returns(frames[1]);
            var image = SKImage.Create(new SKImageInfo(mockCanvas.Object.CanvasWidth, mockCanvas.Object.CanvasHeight));

            var renderer = new Renderer(mockFrame.Object, mockCanvas.Object);

            // Act
            var result = renderer.RebuildFrames(image);

            // Assert
            Assert.True(result.PeekPixels().GetPixelColor(10, 10) == SKColors.Black);
            Assert.False(result.PeekPixels().GetPixelColor(11, 11) == SKColors.Black);
        }

        [Fact]
        public void RebuildLayers_DrawsVisibleLayers()
        {
            // Arrange
            var bounds = new SKPath();
            var bounds1 = new SKPath();
            bounds.AddRect(new SKRect(0, 0, 100, 100));
            bounds1.AddRect(new SKRect(0, 0, 50, 50));
            var frame = new FrameLayerModel(
                new FrameModel
                {
                    Bounds = bounds,
                    Id = 0,
                    IsVisible = true,
                    Name = ""

                }, mockCanvas.Object)
            {
                IsDrawBounds = true,
                LayerVM = new LayerViewModel(mockCanvas.Object)
                {
                    Layers = new ObservableCollection<LayerModel> { new LayerModel() }
                }
            };
            var imageLayer = SKImage.Create(new SKImageInfo(mockCanvas.Object.CanvasWidth, mockCanvas.Object.CanvasHeight));
            using var surface = SKSurface.Create(new SKImageInfo(mockCanvas.Object.CanvasWidth, mockCanvas.Object.CanvasHeight));
            using var paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColors.Black
            };

            frame.LayerVM.Layers.Clear();

            surface.Canvas.DrawRect(new SKRect(0, 0, 11, 11), paint);
            imageLayer = surface.Snapshot();
            surface.Canvas.Clear(SKColors.Transparent);
            frame.LayerVM.Layers.Add(new LayerModel
            {
                Id = 0,
                Image = imageLayer
            });

            surface.Canvas.DrawRect(new SKRect(11, 11, 21, 21), paint);
            imageLayer = surface.Snapshot();
            surface.Canvas.Clear(SKColors.Transparent);
            frame.LayerVM.Layers.Add(new LayerModel
            {
                Id = 1,
                Image = imageLayer
            });

            surface.Canvas.DrawRect(new SKRect(21, 21, 31, 31), paint);
            imageLayer = surface.Snapshot();
            surface.Canvas.Clear(SKColors.Transparent);
            frame.LayerVM.Layers.Add(new LayerModel
            {
                Id = 2,
                Image = imageLayer
            });

            frame.LayerVM.SelectLayer = frame.LayerVM.Layers[1];

            var layer1 = new LayerModel
            {
                Id = -1,
                Name = "",
                Image = SKImage.Create(new SKImageInfo(mockCanvas.Object.CanvasWidth, mockCanvas.Object.CanvasHeight))
            };
            var layer2 = new LayerModel
            {
                Id = -2,
                Name = "",
                Image = SKImage.Create(new SKImageInfo(mockCanvas.Object.CanvasWidth, mockCanvas.Object.CanvasHeight))
            };

            mockCanvas.SetupProperty(f => f.Scale, 1f);
            mockFrame.Setup(f => f.SelectFrame).Returns(frame);

            var renderer = new Renderer(mockFrame.Object, mockCanvas.Object);

            // Act
            renderer.RebuildLayers(surface, layer1, layer2);

            // Assert
            Assert.Equal(layer1.Image.PeekPixels().GetPixelColor(10, 10), SKColors.Black);
            Assert.NotEqual(layer1.Image.PeekPixels().GetPixelColor(11, 11), SKColors.Black);
            Assert.Equal(layer2.Image.PeekPixels().GetPixelColor(30, 30), SKColors.Black);
            Assert.NotEqual(layer2.Image.PeekPixels().GetPixelColor(31, 31), SKColors.Black);
        }
    }
}
