using Mangaka_Studio.Controls.Tools;
using Mangaka_Studio.Interfaces;
using Mangaka_Studio.Models;
using Mangaka_Studio.ViewModels;
using Moq;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mangaka_Studio_Test
{
    public class UnitTestPipetteTool
    {
        private PipetteTool pipetteTool;
        private Mock<ICanvasContext> mockCanvas;
        private Mock<IColorPickerContext> mockColorPicker;
        private Mock<IFrameContext> mockFrame;
        private Mock<ITextTemplatesContext> mockTextTemplates;
        private Mock<ICommand> mockCommand;
        private FrameLayerModel frameLayer;

        public UnitTestPipetteTool()
        {
            var penSettings = new PenToolSettingsViewModel
            {
                StrokeWidth = 2,
                Transparent = 100,
                IsSmooth = true,
                StrokeColor = new SKColor(255, 0, 0),
                StrokeColor1 = new SKColor(0, 255, 0)
            };
            pipetteTool = new PipetteTool(penSettings);

            mockCanvas = new Mock<ICanvasContext>();
            mockColorPicker = new Mock<IColorPickerContext>();
            mockFrame = new Mock<IFrameContext>();
            mockTextTemplates = new Mock<ITextTemplatesContext>();

            mockCommand = new Mock<ICommand>();
            mockColorPicker.SetupGet(c => c.SwitchColorPipette).Returns(mockCommand.Object);
            mockCanvas.SetupProperty(c => c.ColorPipette);


            mockCanvas.SetupGet(c => c.CanvasWidth).Returns(10);
            mockCanvas.SetupGet(c => c.CanvasHeight).Returns(10);

            var screenshot = SKSurface.Create(new SKImageInfo(10, 10));
            screenshot.Canvas.Clear(SKColors.Blue);

            var layerVM = new LayerViewModel(mockCanvas.Object)
            {
                Screenshot = screenshot.Snapshot()
            };

            frameLayer = new FrameLayerModel(new FrameModel(), mockCanvas.Object)
            {
                LayerVM = layerVM
            };

            mockFrame.SetupGet(f => f.SelectFrame).Returns(frameLayer);
        }


        [Fact]
        public void OnMouseDown_ValidPosition_SetsColorAndExecutesCommand()
        {
            var pos = new SKPoint(5, 5);

            pipetteTool.OnMouseDown(mockCanvas.Object, pos, mockColorPicker.Object, mockFrame.Object, mockTextTemplates.Object);

            Assert.Equal(SKColors.Blue, mockCanvas.Object.ColorPipette);
            mockCommand.Verify(c => c.Execute(SKColors.Blue), Times.Once);
        }

        [Fact]
        public void OnMouseMove_ValidPosition_SetsColorWithoutCommand()
        {
            var pos = new SKPoint(5, 5);

            pipetteTool.OnMouseMove(mockCanvas.Object, pos, mockColorPicker.Object, mockFrame.Object, mockTextTemplates.Object);

            Assert.Equal(SKColors.Blue, mockCanvas.Object.ColorPipette);
            mockCommand.Verify(c => c.Execute(It.IsAny<object>()), Times.Never);
        }

        [Theory]
        [InlineData(-1, 5)]
        [InlineData(5, -1)]
        [InlineData(100, 5)]
        [InlineData(5, 100)]
        public void ApplyPipette_PositionOutOfBounds_SetsTransparent(float x, float y)
        {
            var pos = new SKPoint(x, y);

            pipetteTool.OnMouseMove(mockCanvas.Object, pos, mockColorPicker.Object, mockFrame.Object, mockTextTemplates.Object);

            Assert.Equal(SKColors.Transparent, mockCanvas.Object.ColorPipette);
        }

        [Fact]
        public void ApplyPipette_NoScreenshot_SetsTransparent()
        {
            frameLayer.LayerVM.Screenshot = null;

            var pos = new SKPoint(5, 5);
            pipetteTool.OnMouseMove(mockCanvas.Object, pos, mockColorPicker.Object, mockFrame.Object, mockTextTemplates.Object);

            Assert.Equal(SKColors.Transparent, mockCanvas.Object.ColorPipette);
        }

        [Fact]
        public void GetColorPixels_ValidImage_ReturnsCorrectColor()
        {
            using var surface = SKSurface.Create(new SKImageInfo(10, 10));
            surface.Canvas.Clear(SKColors.Red);
            var image = surface.Snapshot();

            var color = pipetteTool.GetColorPixels(image, new SKPoint(5, 5));

            Assert.Equal(SKColors.Red, color);
        }

        [Fact]
        public void GetColorPixels_NullPixels_ReturnsTransparent()
        {
            using var surface = SKSurface.Create(new SKImageInfo(mockCanvas.Object.CanvasWidth, mockCanvas.Object.CanvasHeight));
            surface.Canvas.Clear(SKColors.Red);
            var image = surface.Snapshot();

            var color = pipetteTool.GetColorPixels(image, new SKPoint(-1, -1));

            Assert.Equal(SKColors.Transparent.Alpha, color.Alpha);
        }

    }
}
