using Mangaka_Studio.Commands;
using Mangaka_Studio.Models;
using Mangaka_Studio.Services;
using Microsoft.Win32;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Mangaka_Studio.ViewModels
{
    public class FileViewModel : INotifyPropertyChanged
    {
        private readonly LayerViewModel layerViewModel;
        private readonly CanvasViewModel canvasViewModel;
        private readonly ToolsViewModel toolsViewModel;
        private string lastFilePath;

        public ICommand SaveCommand { get; }
        public ICommand OpenCommand { get; }
        public ICommand IsNewFileCommand { get; }
        public FileViewModel(LayerViewModel layerViewModel, CanvasViewModel canvasViewModel, ToolsViewModel toolsViewModel)
        {
            this.layerViewModel = layerViewModel;
            this.canvasViewModel = canvasViewModel;
            this.toolsViewModel = toolsViewModel;
            OpenCommand = new RelayCommand(_ => OpenFile());
            SaveCommand = new RelayCommand(param => SaveFile((string)param));
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
            var image = layerViewModel.GetCompositedImage();
            if (image != null)
            {
                var layerData = layerViewModel.Layers.Select(layer => new LayerSerializable
                {
                    Id = layer.Id,
                    Name = layer.Name,
                    ImageBase64 = EncodeImageToBase64(layer.Image),
                    IsVisible = layer.IsVisible,
                    Opacity = layer.Opacity
                }).ToList();

                var project = new ProjectData { Layers = layerData,
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
                    layerViewModel.IsModified = false;
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
                    layerViewModel.IsModified = false;
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
                        using var data = image.Encode(format, 100);
                        data.SaveTo(stream);
                    }
                }
            }
        }

        public void OpenFile()
        {
            if (layerViewModel.IsModified)
            {
                var result = MessageBox.Show(
                    "Файл содержит несохраненные изменения. Хотите сохранить их?",
                    "Подтверждение",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    SaveFile("false"); // Сохраняем текущие изменения
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return; // Отменяем операцию
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
                    var project = JsonSerializer.Deserialize<ProjectData>(json, options);
                    layerViewModel.Layers.Clear();
                    foreach (var layer in project.Layers)
                    {
                        layerViewModel.Layers.Add(new LayerModel
                        {
                            Id = layer.Id,
                            Name = layer.Name,
                            Image = DecodeImageFromBase64(layer.ImageBase64),
                            IsVisible = layer.IsVisible,
                            Opacity = layer.Opacity
                        });
                    }
                    canvasViewModel.CanvasWidth = project.CanvasWidth;
                    canvasViewModel.CanvasHeight = project.CanvasHeight;
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
                    layerViewModel.k = layerViewModel.Layers.Count;
                    layerViewModel.SelectLayer = layerViewModel.Layers.FirstOrDefault();
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
                        layerViewModel.Layers.Clear();
                        layerViewModel.Layers.Add(new LayerModel
                        {
                            Id = layerViewModel.GetNewIdLayer(true),
                            Name = fileName,
                            Image = image
                        });
                        layerViewModel.SelectLayer = layerViewModel.Layers.FirstOrDefault();
                    }
                }
                lastFilePath = dialog.FileName;
                layerViewModel.IsModified = false;
            }
        }

        public void NewFile(int width, int height)
        {
            canvasViewModel.CanvasWidth = width;
            canvasViewModel.CanvasHeight = height;
            canvasViewModel.ResetCommand.Execute(null);
            layerViewModel.Layers.Clear();
            var id = layerViewModel.GetNewIdLayer(true);
            var newLayer = new LayerModel
            {
                Id = id,
                Name = $"Слой {id}",
                //Image = SKImage.Create(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul))
            };
            var imageInfo = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            using (var surface = SKSurface.Create(imageInfo))
            {
                surface.Canvas.Clear(SKColors.Transparent);
                newLayer.Image = surface.Snapshot();
                surface.Dispose();
            }
            layerViewModel.Layers.Add(newLayer);
            layerViewModel.SelectLayer = newLayer;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
