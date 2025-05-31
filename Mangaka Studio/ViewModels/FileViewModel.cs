using Mangaka_Studio.Controls.Converters;
using Mangaka_Studio.Controls.Tools;
using Mangaka_Studio.Interfaces;
using Mangaka_Studio.Models;
using Microsoft.Win32;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using Path = System.IO.Path;

namespace Mangaka_Studio.ViewModels
{
    public class FileViewModel : INotifyPropertyChanged
    {
        private readonly FrameViewModel frameViewModel;
        private readonly CanvasViewModel canvasViewModel;
        private readonly ToolsViewModel toolsViewModel;
        private readonly ICanvasContext canvasContext;
        private string lastFilePath;

        public ICommand SaveCommand { get; }
        public ICommand OpenCommand { get; }
        public ICommand IsNewFileCommand { get; }
        public FileViewModel(FrameViewModel frameViewModel, CanvasViewModel canvasViewModel, ToolsViewModel toolsViewModel, ICanvasContext canvasContext)
        {
            this.frameViewModel = frameViewModel;
            this.canvasViewModel = canvasViewModel;
            this.toolsViewModel = toolsViewModel;
            this.canvasContext = canvasContext;
            OpenCommand = new RelayCommand(_ => OpenFile());
            SaveCommand = new RelayCommand(param => SaveFile((string)param));
            this.canvasContext = canvasContext;
        }

        private string EncodeImageToBase64(SKImage image)
        {
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return Convert.ToBase64String(data.ToArray());
        }

        private SKImage DecodeImageFromBase64(string base64)
        {
            var data = Convert.FromBase64String(base64);
            using var stream = new SKMemoryStream(data);
            return SKImage.FromEncodedData(stream);
        }

        public void SaveFile(string asSave)
        {
            var image = frameViewModel.GetCompositedImage();
            if (image != null)
            {
                var frameData = frameViewModel.Frames.Select(frame => new FrameSerializable
                {
                    Id = frame.Id,
                    Model = frame.Model,
                    Name = frame.Name, 
                    Bounds = frame.Bounds,
                    IsDrawBounds = frame.IsDrawBounds,
                    IsVisible = frame.IsVisible,
                    IsSelected = frame.IsSelected,
                    FrameMode = frame.FrameMode,
                    BoundsWidth = frame.BoundsWidth
                }).ToList();
                List<List<LayerSerializable>> layersData = new List<List<LayerSerializable>>();
                List<int> selectLayerData = new List<int>();
                foreach (var frame in frameViewModel.Frames)
                {
                    selectLayerData.Add(frame.LayerVM.SelectLayer.Id);
                    var layerData = frame.LayerVM.Layers.Select(layer => new LayerSerializable
                    {
                        Id = layer.Id,
                        Name = layer.Name,
                        ImageBase64 = EncodeImageToBase64(layer.Image),
                        IsVisible = layer.IsVisible,
                        Opacity = layer.Opacity,
                        ListText = layer.ListText,
                        ListTemplate = layer.ListTemplate
                    }).ToList();
                    layersData.Add(layerData);
                }
                var selectFrameData = frameViewModel.SelectFrame.Id;
                var project = new ProjectData { Frames = frameData,
                    Layers = layersData,
                    SelectFrame = selectFrameData,
                    SelectLayer = selectLayerData,
                    GeneralView = frameViewModel.GeneralView,
                    CanvasWidth = canvasViewModel.CanvasWidth,
                    CanvasHeight = canvasViewModel.CanvasHeight,
                    Scale = canvasViewModel.Scale,
                    ScalePos = canvasViewModel.ScalePos,
                    Rotate = canvasViewModel.Rotate,
                    OffsetX = canvasViewModel.OffsetX,
                    OffsetY = canvasViewModel.OffsetY,
                    IsGrid = canvasViewModel.IsGrid,
                    GridSize = canvasViewModel.GridSize,
                    CurrentTool = canvasViewModel.CurrentTool,
                    LastEraseToolsType = toolsViewModel.LastEraseToolsType,
                    Tools = toolsViewModel.Tools
                };
                var options = new JsonSerializerOptions { WriteIndented = true };
                options.Converters.Add(new DrawingToolsConverter());
                options.Converters.Add(new SKColorConverter());
                options.Converters.Add(new SKPathConverter());
                options.Converters.Add(new SKRectConverter());
                var json = JsonSerializer.Serialize(project, options);
                bool.TryParse(asSave, out var isQuickSave);
                if (!string.IsNullOrEmpty(lastFilePath) && !isQuickSave)
                {
                    var extension = Path.GetExtension(lastFilePath).ToLower();
                    if (extension == ".mgs")
                    {
                        File.WriteAllText(lastFilePath, json);
                    }
                    else
                    {
                        var format = extension switch
                        {
                            ".jpg" or ".jpeg" => SKEncodedImageFormat.Jpeg,
                            ".bmp" => SKEncodedImageFormat.Bmp,
                            ".webp" => SKEncodedImageFormat.Webp,
                            _ => SKEncodedImageFormat.Png,
                        };
                        File.WriteAllText(lastFilePath, json);
                        using var stream = File.OpenWrite(lastFilePath);
                        using var data = image.Encode(format, 100);
                        data.SaveTo(stream);
                    }
                    frameViewModel.SelectFrame.LayerVM.IsModified = false;
                    return;
                }

                var dialog = new SaveFileDialog
                {
                    Filter = "Mangaka Studio (*.mgs)|*.mgs|PNG (*.png)|*.png|JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|BMP (*.bmp)|*.bmp|WEBP (*.webp)|*.webp",
                    Title = "Сохранить изображение",
                    FileName = "image.mgs"
                };

                if (dialog.ShowDialog() == true)
                {
                    frameViewModel.SelectFrame.LayerVM.IsModified = false;
                    lastFilePath = dialog.FileName;
                    var extension = Path.GetExtension(dialog.FileName).ToLower();
                    if (extension == ".mgs")
                    {
                        File.WriteAllText(dialog.FileName, json);
                    }
                    else
                    {
                        var format = extension switch
                        {
                            ".jpg" or ".jpeg" => SKEncodedImageFormat.Jpeg,
                            ".bmp" => SKEncodedImageFormat.Bmp,
                            ".webp" => SKEncodedImageFormat.Webp,
                            _ => SKEncodedImageFormat.Png,
                        };
                        File.WriteAllText(dialog.FileName, json);
                        using var stream = File.OpenWrite(dialog.FileName);
                        image = frameViewModel.GetCompositedFrames();
                        using var data = image.Encode(format, 100);
                        data.SaveTo(stream);
                    }
                }
            }
        }

        public void OpenFile()
        {
            if (frameViewModel.SelectFrame.LayerVM.IsModified)
            {
                var result = MessageBox.Show(
                    "Файл содержит несохраненные изменения. Хотите сохранить их?",
                    "Подтверждение",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    SaveFile("false");
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            var dialog = new OpenFileDialog
            {
                Filter = "Image|*.mgs;*.png;*.jpg;*.jpeg;*.bmp;*.webp",
                Title = "Открыть изображение"
            };

            if (dialog.ShowDialog() == true)
            {
                if (Path.GetExtension(dialog.FileName).ToLower() == ".mgs")
                {
                    var json = File.ReadAllText(dialog.FileName);
                    var options = new JsonSerializerOptions();
                    options.Converters.Add(new DrawingToolsConverter());
                    options.Converters.Add(new SKColorConverter());
                    options.Converters.Add(new SKPathConverter());
                    var project = JsonSerializer.Deserialize<ProjectData>(json, options);

                    canvasViewModel.CanvasWidth = project.CanvasWidth;
                    canvasViewModel.CanvasHeight = project.CanvasHeight;

                    frameViewModel.Frames.Clear();
                    foreach (var frame in project.Frames)
                    {
                        var path = new SKPath();
                        var points = frame.Bounds.Points;
                        path.MoveTo(points[0]);
                        for (var i = 1; i < points.Length - 1; i++)
                        {
                            path.LineTo(points[i]);
                        }
                        path.Close();
                        var frameModel = new FrameModel
                        {
                            Id = frame.Id,
                            Name = frame.Name,
                            Bounds = path,
                            IsVisible = frame.IsVisible
                        };
                        frameViewModel.Frames.Add(new FrameLayerModel(frameModel, canvasContext)
                        {
                            IsDrawBounds = frame.IsDrawBounds,
                            IsSelected = frame.IsSelected,
                            FrameMode = frame.FrameMode,
                            BoundsWidth = frame.BoundsWidth
                        });
                    }

                    frameViewModel.SelectFrame = frameViewModel.Frames.Where(frame => frame.Id == project.SelectFrame).ToList()[0];
                    for (var i = 0; i < frameViewModel.Frames.Count; i++)
                    {
                        frameViewModel.Frames[i].LayerVM.Layers.Clear();
                        foreach (var layer in project.Layers[i])
                        {
                            frameViewModel.Frames[i].LayerVM.Layers.Add(new LayerModel
                            {
                                Id = layer.Id,
                                Name = layer.Name,
                                Image = DecodeImageFromBase64(layer.ImageBase64),
                                IsVisible = layer.IsVisible,
                                Opacity = layer.Opacity,
                                ListText = layer.ListText,
                                ListTemplate = layer.ListTemplate
                            });
                        }
                        frameViewModel.Frames[i].LayerVM.baseSurface = SKSurface.Create(new SKImageInfo((int)canvasViewModel.CanvasWidth, (int)canvasViewModel.CanvasHeight, SKColorType.Rgba8888, SKAlphaType.Premul));
                        frameViewModel.Frames[i].LayerVM.tempSurface = SKSurface.Create(new SKImageInfo((int)canvasViewModel.CanvasWidth, (int)canvasViewModel.CanvasHeight, SKColorType.Rgba8888, SKAlphaType.Premul));
                        frameViewModel.Frames[i].LayerVM.SelectLayer = frameViewModel.Frames[i].LayerVM.Layers.Where(layer => layer.Id == project.SelectLayer[i]).ToList()[0];

                    }
                    frameViewModel.GeneralView = project.GeneralView;
                    canvasViewModel.Scale = project.Scale;
                    canvasViewModel.ScalePos = project.ScalePos;
                    canvasViewModel.Rotate = project.Rotate;
                    canvasViewModel.OffsetX = project.OffsetX;
                    canvasViewModel.OffsetY = project.OffsetY;
                    canvasViewModel.IsGrid = project.IsGrid;
                    canvasViewModel.GridSize = project.GridSize;
                    toolsViewModel.LastEraseToolsType = project.LastEraseToolsType;
                    toolsViewModel.Tools.Clear();
                    toolsViewModel.Tools = new Dictionary<ToolsType, DrawingTools>(project.Tools);
                    foreach (var tool in project.Tools)
                    {
                        if (tool.Value.GetType() == project.CurrentTool.GetType()) 
                        {
                            toolsViewModel.SelectTool(tool.Key);
                        }
                    }
                    frameViewModel.SelectFrame.LayerVM.k = frameViewModel.SelectFrame.LayerVM.Layers.Count;
                    frameViewModel.OnPropertyChanged(nameof(frameViewModel.SelectFrame));
                }
                else
                {
                    using var stream = File.OpenRead(dialog.FileName);
                    var bitmap = SKBitmap.Decode(stream);
                    var info = new SKImageInfo(bitmap.Width, bitmap.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
                    using var surface = SKSurface.Create(info);
                    surface.Canvas.DrawBitmap(bitmap, 0, 0);
                    var image = surface.Snapshot();

                    if (image != null)
                    {
                        var fileName = Path.GetFileNameWithoutExtension(dialog.FileName);
                        canvasViewModel.CanvasHeight = image.Height;
                        canvasViewModel.CanvasWidth = image.Width;
                        NewFrame();
                        frameViewModel.SelectFrame = frameViewModel.Frames.FirstOrDefault();
                        frameViewModel.SelectFrame.LayerVM.Layers.Clear();
                        frameViewModel.SelectFrame.LayerVM.Layers.Add(new LayerModel
                        {
                            Id = frameViewModel.SelectFrame.LayerVM.GetNewIdLayer(true),
                            Name = fileName,
                            Image = image
                        }); 
                        frameViewModel.SelectFrame.LayerVM.baseSurface = SKSurface.Create(new SKImageInfo((int)canvasViewModel.CanvasWidth, (int)canvasViewModel.CanvasHeight, SKColorType.Rgba8888, SKAlphaType.Premul));
                        frameViewModel.SelectFrame.LayerVM.tempSurface = SKSurface.Create(new SKImageInfo((int)canvasViewModel.CanvasWidth, (int)canvasViewModel.CanvasHeight, SKColorType.Rgba8888, SKAlphaType.Premul));
                        frameViewModel.SelectFrame.LayerVM.SelectLayer = frameViewModel.SelectFrame.LayerVM.Layers.FirstOrDefault();
                    }
                }
                lastFilePath = dialog.FileName;
                frameViewModel.SelectFrame.LayerVM.IsModified = false;
            }
        }

        public void NewFile(int width, int height)
        {
            canvasViewModel.CanvasWidth = width;
            canvasViewModel.CanvasHeight = height;
            canvasViewModel.ResetCommand.Execute(null);
            NewFrame();
            frameViewModel.SelectFrame = frameViewModel.Frames.FirstOrDefault();
            frameViewModel.SelectFrame.LayerVM.Layers.Clear();
            var id = frameViewModel.SelectFrame.LayerVM.GetNewIdLayer(true);
            var newLayer = new LayerModel
            {
                Id = id,
                Name = $"Слой {id}"
            };
            var imageInfo = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            using (var surface = SKSurface.Create(imageInfo))
            {
                surface.Canvas.Clear(SKColors.Transparent);
                newLayer.Image = surface.Snapshot();
                surface.Dispose();
            }
            frameViewModel.SelectFrame.LayerVM.Layers.Add(newLayer);
            frameViewModel.SelectFrame.LayerVM.SelectLayer = newLayer;
        }

        private void NewFrame()
        {
            frameViewModel.Frames.Clear();
            var id = frameViewModel.GetNewIdLayer(true);
            var path = new SKPath();
            path.MoveTo(0, 0);
            path.LineTo(canvasViewModel.CanvasWidth, 0);
            path.LineTo(canvasViewModel.CanvasWidth, canvasViewModel.CanvasHeight);
            path.LineTo(0, canvasViewModel.CanvasHeight);
            path.Close();
            var newFrame = new FrameModel
            {
                Id = id,
                Name = $"Кадр {id}",
                Bounds = path
            };
            var frameLayer = new FrameLayerModel(newFrame, canvasContext);
            frameViewModel.Frames.Add(frameLayer);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
