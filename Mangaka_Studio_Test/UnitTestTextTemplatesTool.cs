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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Resources;

namespace Mangaka_Studio_Test
{
    public class UnitTestTextTemplatesTool
    {
        private TextTool textTool;
        private Mock<ICanvasContext> mockCanvas;
        private Mock<IColorPickerContext> mockColorPicker;
        private Mock<IFrameContext> mockFrame;
        private Mock<ITextTemplatesContext> mockTextTemplates;
        private Mock<ICommand> mockCommand;
        private FrameLayerModel frameLayer;

        public UnitTestTextTemplatesTool()
        {
            var textSettings = new TextToolSettingsViewModel();
            textTool = new TextTool(textSettings);

            mockCanvas = new Mock<ICanvasContext>();
            mockColorPicker = new Mock<IColorPickerContext>();
            mockFrame = new Mock<IFrameContext>();
            mockTextTemplates = new Mock<ITextTemplatesContext>();

            mockCanvas.SetupProperty(c => c.ColorPipette);
            //mockTextTemplates.SetupGet(c => c.SetCreateModeCommand).Returns(mockCommand.Object);
            mockCanvas.SetupGet(c => c.CanvasWidth).Returns(100);
            mockCanvas.SetupGet(c => c.CanvasHeight).Returns(100);

            var layerVM = new LayerViewModel(mockCanvas.Object)
            {
            };

            frameLayer = new FrameLayerModel(new FrameModel(), mockCanvas.Object)
            {
                LayerVM = layerVM
            };

            mockFrame.SetupGet(f => f.SelectFrame).Returns(frameLayer);
        }

        [Fact]
        public void OnMouseDown_TextModeCreate_UpdatesTextSettings()
        {
            // Arrange
            var pos = new SKPoint(20, 30);
            var textElement = new TextModel
            {
                FontFamily = "Arial",
                FontSize = 14,
                FontStyleWeight = 800,
                FontStyleWidth = 5,
                FontStyleSlant = 1,
                IsStroke = true,
                StrokeWidth = 3,
                Color = SKColors.Red,
                ColorStroke = SKColors.Blue,
                IsSelected = true
            };

            var templateElement = new TemplateModel { IsSelected = true };

            var layer = new LayerViewModel(mockCanvas.Object)
            {
                SelectText = textElement,
                SelectTemplate = templateElement
            };

            var mockAddTextCommand = new Mock<ICommand>();
            mockAddTextCommand.Setup(c => c.Execute(It.IsAny<object>()))
                .Callback(() => layer.SelectText = textElement);
            layer.AddTextCommand = mockAddTextCommand.Object;

            frameLayer.LayerVM = layer;
            mockFrame.Setup(f => f.SelectFrame).Returns(frameLayer);
            mockTextTemplates.SetupProperty(t => t.TextMode, TextMode.Create);
            mockTextTemplates.SetupProperty(t => t.TextModeStr);
            mockTextTemplates.SetupProperty(t => t.IsVisibleMode);
            mockTextTemplates.SetupProperty(t => t.SelectedFontFamily);
            mockTextTemplates.SetupProperty(t => t.SelectedFontSize);
            mockTextTemplates.SetupProperty(t => t.FontStyleWeight);
            mockTextTemplates.SetupProperty(t => t.FontStyleWidth);
            mockTextTemplates.SetupProperty(t => t.FontStyleSlant);
            mockTextTemplates.SetupProperty(t => t.IsStroke);
            mockTextTemplates.SetupProperty(t => t.IsHitTextDel);
            mockTextTemplates.SetupProperty(t => t.OpacityBoundsTextDel);

            // Act
            textTool.OnMouseDown(mockCanvas.Object, pos, mockColorPicker.Object, mockFrame.Object, mockTextTemplates.Object);

            // Assert
            Assert.True(layer.IsModified);
            Assert.False(textElement.IsSelected);
            Assert.False(templateElement.IsSelected);
            Assert.Null(layer.SelectTemplate);
            Assert.Equal(TextMode.Edit, mockTextTemplates.Object.TextMode);
            Assert.Equal("", mockTextTemplates.Object.TextModeStr);
            Assert.Equal("Collapsed", mockTextTemplates.Object.IsVisibleMode);

            Assert.Equal("Arial", mockTextTemplates.Object.SelectedFontFamily);
            Assert.Equal(14, mockTextTemplates.Object.SelectedFontSize);
            Assert.Equal(800, mockTextTemplates.Object.FontStyleWeight);
            Assert.Equal(5, mockTextTemplates.Object.FontStyleWidth);
            Assert.Equal(1, mockTextTemplates.Object.FontStyleSlant);
            Assert.True(mockTextTemplates.Object.IsStroke);
            Assert.True(mockTextTemplates.Object.IsHitTextDel);
            Assert.Equal(1f, mockTextTemplates.Object.OpacityBoundsTextDel);
            Assert.Equal(SKColors.Red, textTool.Settings.StrokeColor);
            Assert.Equal(SKColors.Blue, textTool.Settings.StrokeColor1);
            Assert.Equal(3, textTool.Settings.StrokeWidth);
        }

        [Fact]
        public void OnMouseDown_DoubleClick_ShowsTextEditor()
        {
            // Arrange
            var pos = new SKPoint(50, 50);
            var textElement = new TextModel
            {
                Position = new SKPoint(50, 50),
                Text = "Text",
                Rotate = 0,
                FontFamily = "Arial",
                FontSize = 14,
                FontStyleWeight = 800,
                FontStyleWidth = 5,
                FontStyleSlant = 1,
                IsStroke = true,
                StrokeWidth = 3,
                Color = SKColors.Red,
                ColorStroke = SKColors.Blue,
                IsSelected = true
            };

            var layer = new LayerViewModel(mockCanvas.Object)
            {
                SelectText = textElement
            };
            frameLayer.LayerVM = layer;

            mockFrame.Setup(f => f.SelectFrame).Returns(frameLayer);
            mockCanvas.SetupProperty(c => c.Scale, 1f);
            mockTextTemplates.SetupProperty(t => t.TextMode, TextMode.Edit);
            textTool.lastClickTime = DateTime.Now; 
            Assert.NotNull(frameLayer.LayerVM.SelectText);
            Assert.True(frameLayer.LayerVM.SelectText.IsSelected);

            // Act
            Thread.Sleep(50);
            textTool.OnMouseDown(mockCanvas.Object, pos, mockColorPicker.Object, mockFrame.Object, mockTextTemplates.Object);

            Assert.NotNull(frameLayer.LayerVM.SelectText);
            Assert.True(frameLayer.LayerVM.SelectText.IsSelected);
            Assert.True(frameLayer.LayerVM.IsModified);
            // Assert
            mockCanvas.Verify(c => c.ShowTextEditor(textElement), Times.Once);
        }

        [Fact]
        public void OnMouseDown_SelectedTemplate()
        {
            // Arrange
            var pos = new SKPoint(50, 50);

            string droppedPath = "pack://application:,,,/Resources/Templates/Bubbles/BubbleTransparent1.png";
            var bounds = new SKRect(
                0, 0, mockCanvas.Object.CanvasWidth, mockCanvas.Object.CanvasHeight
            );
            var templateElement = new TemplateModel
            {
                IsSelected = false,
                Path = droppedPath,
                Bounds = bounds
            };
            var layer = new LayerViewModel(mockCanvas.Object)
            {
                SelectTemplate = null
            };
            layer.SelectLayer.ListTemplate.Add(templateElement);
            frameLayer.LayerVM = layer;

            mockFrame.Setup(f => f.SelectFrame).Returns(frameLayer);
            mockTextTemplates.SetupProperty(t => t.TextMode, TextMode.Edit);

            // Act
            textTool.OnMouseDown(mockCanvas.Object, pos, mockColorPicker.Object, mockFrame.Object, mockTextTemplates.Object);

            // Assert
            Assert.NotNull(frameLayer.LayerVM.SelectTemplate);
            Assert.True(frameLayer.LayerVM.SelectTemplate.IsSelected);
        }

        [Fact]
        public void OnMouseDown_NotSelectedTemplate()
        {
            // Arrange
            var pos = new SKPoint(70, 70);

            string droppedPath = "pack://application:,,,/Resources/Templates/Bubbles/BubbleTransparent1.png";
            var bounds = new SKRect(
                0, 0, mockCanvas.Object.CanvasWidth / 2, mockCanvas.Object.CanvasHeight / 2
            );
            var templateElement = new TemplateModel
            {
                IsSelected = true,
                Path = droppedPath,
                Bounds = bounds
            };
            var layer = new LayerViewModel(mockCanvas.Object)
            {
                SelectTemplate = templateElement
            };
            layer.SelectLayer.ListTemplate.Add(templateElement);
            frameLayer.LayerVM = layer;

            mockCanvas.SetupProperty(c => c.Scale, 1f);
            mockFrame.Setup(f => f.SelectFrame).Returns(frameLayer);
            mockTextTemplates.SetupProperty(t => t.TextMode, TextMode.Edit);

            // Act
            textTool.OnMouseDown(mockCanvas.Object, pos, mockColorPicker.Object, mockFrame.Object, mockTextTemplates.Object);

            // Assert
            Assert.Null(frameLayer.LayerVM.SelectTemplate);
        }

        [Fact]
        public void OnMouseDown_SelectedText()
        {
            // Arrange
            var pos = new SKPoint(50, 50);

            var textElement = new TextModel
            {
                Position = new SKPoint(50, 50),
                Text = "Text",
                Rotate = 0,
                FontFamily = "Arial",
                FontSize = 14,
                FontStyleWeight = 800,
                FontStyleWidth = 5,
                FontStyleSlant = 1,
                IsStroke = true,
                StrokeWidth = 3,
                Color = SKColors.Red,
                ColorStroke = SKColors.Blue,
                IsSelected = false
            };
            var layer = new LayerViewModel(mockCanvas.Object)
            {
                SelectText = null
            };
            layer.SelectLayer.ListText.Add(textElement);
            frameLayer.LayerVM = layer;

            mockFrame.Setup(f => f.SelectFrame).Returns(frameLayer);
            mockTextTemplates.SetupProperty(t => t.TextMode, TextMode.Edit);

            // Act
            textTool.OnMouseDown(mockCanvas.Object, pos, mockColorPicker.Object, mockFrame.Object, mockTextTemplates.Object);

            // Assert
            Assert.NotNull(frameLayer.LayerVM.SelectText);
            Assert.True(frameLayer.LayerVM.SelectText.IsSelected);
        }

        [Fact]
        public void OnMouseDown_NotSelectedText()
        {
            // Arrange
            var pos = new SKPoint(70, 70);

            var textElement = new TextModel
            {
                Position = new SKPoint(10, 10),
                Text = "Text",
                Rotate = 0,
                FontFamily = "Arial",
                FontSize = 14,
                FontStyleWeight = 800,
                FontStyleWidth = 5,
                FontStyleSlant = 1,
                IsStroke = true,
                StrokeWidth = 3,
                Color = SKColors.Red,
                ColorStroke = SKColors.Blue,
                IsSelected = true
            };
            var layer = new LayerViewModel(mockCanvas.Object)
            {
                SelectText = textElement
            };
            layer.SelectLayer.ListText.Add(textElement);
            frameLayer.LayerVM = layer;

            mockCanvas.SetupProperty(c => c.Scale, 1f);
            mockFrame.Setup(f => f.SelectFrame).Returns(frameLayer);
            mockTextTemplates.SetupProperty(t => t.TextMode, TextMode.Edit);

            // Act
            textTool.OnMouseDown(mockCanvas.Object, pos, mockColorPicker.Object, mockFrame.Object, mockTextTemplates.Object);

            // Assert
            Assert.Null(frameLayer.LayerVM.SelectText);
        }

        [Fact]
        public void OnMouseMove_Text()
        {
            // Arrange
            var pos = new SKPoint(70, 70);

            var textElement = new TextModel
            {
                Position = new SKPoint(50, 50),
                Text = "Text",
                Rotate = 0,
                FontFamily = "Arial",
                FontSize = 14,
                FontStyleWeight = 800,
                FontStyleWidth = 5,
                FontStyleSlant = 1,
                IsStroke = true,
                StrokeWidth = 3,
                Color = SKColors.Red,
                ColorStroke = SKColors.Blue,
                IsSelected = true
            };
            var layer = new LayerViewModel(mockCanvas.Object)
            {
                SelectText = textElement
            };
            frameLayer.LayerVM = layer;

            mockFrame.Setup(f => f.SelectFrame).Returns(frameLayer);
            mockTextTemplates.SetupProperty(t => t.TextMode, TextMode.Edit);
            textTool.currentDrag = DragMode.MoveAll;
            var position = textElement.Position;
            // Act
            textTool.OnMouseMove(mockCanvas.Object, pos, mockColorPicker.Object, mockFrame.Object, mockTextTemplates.Object);

            // Assert
            Assert.Equal(pos, frameLayer.LayerVM.SelectText.Position);
        }

        [Fact]
        public void OnMouseRotate_Text()
        {
            // Arrange
            var pos = new SKPoint(70, 70);

            var textElement = new TextModel
            {
                Position = new SKPoint(50, 50),
                Text = "Text",
                Rotate = 0,
                FontFamily = "Arial",
                FontSize = 14,
                FontStyleWeight = 800,
                FontStyleWidth = 5,
                FontStyleSlant = 1,
                IsStroke = true,
                StrokeWidth = 3,
                Color = SKColors.Red,
                ColorStroke = SKColors.Blue,
                IsSelected = true
            };
            var layer = new LayerViewModel(mockCanvas.Object)
            {
                SelectText = textElement
            };
            frameLayer.LayerVM = layer;

            mockFrame.Setup(f => f.SelectFrame).Returns(frameLayer);
            mockTextTemplates.SetupProperty(t => t.TextMode, TextMode.Edit);
            textTool.currentDrag = DragMode.Rotate;
            var rotate = textElement.Rotate;
            // Act
            textTool.OnMouseMove(mockCanvas.Object, pos, mockColorPicker.Object, mockFrame.Object, mockTextTemplates.Object);

            // Assert
            Assert.NotEqual(rotate, frameLayer.LayerVM.SelectText.Rotate);
        }

        [Fact]
        public void OnMouseMove_Template()
        {
            // Arrange
            var pos = new SKPoint(70, 70);

            string droppedPath = "pack://application:,,,/Resources/Templates/Bubbles/BubbleTransparent1.png";
            var bounds = new SKRect(
                0, 0, mockCanvas.Object.CanvasWidth / 2, mockCanvas.Object.CanvasHeight / 2
            );
            var templateElement = new TemplateModel
            {
                IsSelected = true,
                Path = droppedPath,
                Bounds = bounds
            };
            var layer = new LayerViewModel(mockCanvas.Object)
            {
                SelectTemplate = templateElement
            };
            frameLayer.LayerVM = layer;

            mockFrame.Setup(f => f.SelectFrame).Returns(frameLayer);
            mockTextTemplates.SetupProperty(t => t.TextMode, TextMode.Edit);
            textTool.currentDrag = DragMode.MoveAll;
            var position = templateElement.Bounds;
            // Act
            textTool.OnMouseMove(mockCanvas.Object, pos, mockColorPicker.Object, mockFrame.Object, mockTextTemplates.Object);

            // Assert
            Assert.NotEqual(new SKPoint(position.MidX, position.MidY), new SKPoint(frameLayer.LayerVM.SelectTemplate.Bounds.MidX, frameLayer.LayerVM.SelectTemplate.Bounds.MidY));
        }
    }
}
