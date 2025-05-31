using Xunit;
using Moq;
using SkiaSharp;
using System.Collections.Generic;
using Mangaka_Studio.ViewModels;
using Mangaka_Studio.Models;
using System.Reflection;
using Mangaka_Studio.Interfaces;
using Mangaka_Studio.Controls.Tools;
namespace Mangaka_Studio_Test
{
    public class UnitTestPenTool
    {
        [Fact]
        public void OnMouseDown_ShouldStartDrawingAndApplyPen()
        {
            var mockCanvasContext = new Mock<ICanvasContext>();
            var mockColorContext = new Mock<IColorPickerContext>();
            var mockFrameContext = new Mock<IFrameContext>();
            var mockTextContext = new Mock<ITextTemplatesContext>();

            mockCanvasContext.SetupProperty(c => c.Pressure, 1f);
            mockCanvasContext.SetupProperty(c => c.LastErasePoint);
            mockCanvasContext.SetupProperty(c => c.EraserCursor);
            mockCanvasContext.SetupProperty(c => c.CanvasWidth, 1000);
            mockCanvasContext.SetupProperty(c => c.CanvasHeight, 600);

            var penSettings = new PenToolSettingsViewModel
            {
                StrokeWidth = 2,
                Transparent = 100,
                IsSmooth = true,
                StrokeColor = new SKColor(255, 0, 0),
                StrokeColor1 = new SKColor(0, 255, 0)
            };

            var layerVM = new LayerViewModel(mockCanvasContext.Object);
            layerVM.EnsureSurface();
            layerVM.IsModified = false;

            var frameModel = new FrameModel();
            var frameLayerModel = new FrameLayerModel(frameModel, mockCanvasContext.Object)
            {
                LayerVM = layerVM
            };

            mockFrameContext.SetupGet(f => f.SelectFrame).Returns(frameLayerModel);
            var penTool = new PenTool(penSettings);

            var pos = new SKPoint(100, 100);
            Assert.False(layerVM.IsModified);

            penTool.OnMouseDown(mockCanvasContext.Object, pos, mockColorContext.Object, mockFrameContext.Object, mockTextContext.Object);

            Assert.True(mockFrameContext.Object.SelectFrame.LayerVM.IsModified);
            Assert.Equal(pos, mockCanvasContext.Object.LastErasePoint);
            Assert.Equal(pos, mockCanvasContext.Object.EraserCursor);
        }

        [Fact]
        public void OnMouseUp_WhenIsDrawing_ResetsStateAndUpdatesCanvas()
        {
            // Arrange
            var mockCanvasContext = new Mock<ICanvasContext>();
            var mockColorContext = new Mock<IColorPickerContext>();
            var mockFrameContext = new Mock<IFrameContext>();
            var mockTextContext = new Mock<ITextTemplatesContext>();

            mockCanvasContext.SetupProperty(c => c.Pressure, 1f);
            mockCanvasContext.SetupProperty(c => c.LastErasePoint);
            mockCanvasContext.SetupProperty(c => c.EraserCursor);
            mockCanvasContext.SetupProperty(c => c.CanvasWidth, 1000);
            mockCanvasContext.SetupProperty(c => c.CanvasHeight, 600);

            var penSettings = new PenToolSettingsViewModel
            {
                StrokeWidth = 2,
                Transparent = 100,
                IsSmooth = true,
                StrokeColor = new SKColor(255, 0, 0),
                StrokeColor1 = new SKColor(0, 255, 0)
            };

            var layerVM = new LayerViewModel(mockCanvasContext.Object);
            layerVM.EnsureSurface();
            layerVM.IsModified = false;

            var frameModel = new FrameModel();
            var frameLayerModel = new FrameLayerModel(frameModel, mockCanvasContext.Object)
            {
                LayerVM = layerVM
            };

            mockFrameContext.SetupGet(f => f.SelectFrame).Returns(frameLayerModel);
            var penTool = new PenTool(penSettings);

            var pos = new SKPoint(100, 100);
            // Act
            penTool.OnMouseUp(mockCanvasContext.Object, mockFrameContext.Object);

            // Assert
            Assert.False(penTool._isDrawing);
            Assert.Equal(penTool.pointBuffer.Count, 0);
            Assert.Null(mockCanvasContext.Object.LastErasePoint);
            Assert.Null(mockCanvasContext.Object.EraserCursor);
            Assert.NotNull(mockFrameContext.Object.SelectFrame.LayerVM.SelectLayer.Image);
        } 


        [Fact]
        public void AddBezierSmoothedPoint_ShouldDrawPoint_IfLessThan4Points()
        {
            // Arrange
            var surface = SKSurface.Create(new SKImageInfo(100, 100));
            var canvas = surface.Canvas;
            var mockPaint = new SKPaint();
            var penTool = new PenTool(new PenToolSettingsViewModel());

            var privateBuffer = typeof(PenTool).GetField("pointBuffer", BindingFlags.NonPublic | BindingFlags.Instance);
            var buffer = new Queue<SKPoint>();
            privateBuffer!.SetValue(penTool, buffer);

            var point = new SKPoint(10, 10);

            penTool.AddBezierSmoothedPoint(point, canvas, mockPaint);

            // Assert
            Assert.Single(buffer);
            Assert.Equal(point, buffer.Peek());
        }
    }
}
