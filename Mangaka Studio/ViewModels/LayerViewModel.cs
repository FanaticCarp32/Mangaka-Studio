using Mangaka_Studio.Interfaces;
using Mangaka_Studio.Models;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Mangaka_Studio.ViewModels
{
    public class LayerViewModel : INotifyPropertyChanged
    {
        public int k { get; set; } = 0;
        public bool IsModified { get; set; } = false;
        private int id = 0;
        private ICanvasContext canvas;
        private int frameWidth;
        private int frameHeight;
        private Point lastWH = new Point(1000, 600);
        /*private Stack<(LayerModel Layer, SKImage Image)> undoStack = new Stack<(LayerModel Layer, SKImage Image)>();
        private Stack<(LayerModel Layer, SKImage Image)> redoStack = new Stack<(LayerModel Layer, SKImage Image)>();
        */
        private Stack<(ObservableCollection<LayerModel>, int)> undoStack = new();
        private Stack<(ObservableCollection<LayerModel>, int)> redoStack = new();
        private ObservableCollection<LayerModel> layers = new();
        public ObservableCollection<LayerModel> Layers
        {
            get => layers;
            set
            {
                layers = value;
                OnPropertyChanged(nameof(Layers));
            }
        }

        private LayerModel selectLayer;
        public LayerModel SelectLayer
        {
            get => selectLayer;
            set
            {
                //if (flagDel)
                //    PrevSelectLayer = null;
                //else
                //    PrevSelectLayer = SelectLayer;
                selectLayer = value;
                //if (SelectLayer == null)
                //    PrevSelectLayer = null;

                if (baseSurface != null && SelectLayer != null)
                {
                    //var screenshot = baseSurface.Snapshot();
                    //Layers.Where(layer => layer.Id == PrevSelectLayer.Id).ToList()[0].Image = screenshot;
                    baseSurface.Canvas.Clear(SKColors.Transparent);
                    baseSurface.Canvas.DrawImage(SelectLayer.Image, 0, 0);
                }
                //if (baseSurface != null && flagDel && SelectLayer != null)
                //{
                //    flagDel = false;
                //    baseSurface.Canvas.Clear(SKColors.Transparent);
                //    baseSurface.Canvas.DrawImage(SelectLayer.Image, 0, 0);
                //}
                OnPropertyChanged(nameof(SelectLayer));
            }
        }

        private TextModel selectText;
        public TextModel SelectText
        {
            get => selectText;
            set
            {
                selectText = value;
                OnPropertyChanged(nameof(SelectText));
            }
        }
        
        private TemplateModel selectTemplate;
        public TemplateModel SelectTemplate
        {
            get => selectTemplate;
            set
            {
                selectTemplate = value;
                OnPropertyChanged(nameof(SelectTemplate));
            }
        }

        public bool ChangeSelectLayer { get; set; } = false;
        public SKRect? DirtyRect { get; set; } = null;
        public SKImage Screenshot { get; set; }
        public SKSurface tempSurface { get; set; }
        public SKSurface baseSurface { get; set; }
        public bool NeedsRedraw { get; set; } = true;

        public ICommand AddLayerCommand { get; }
        public ICommand DeleteLayerCommand { get; }
        public ICommand ToggleVisibilityCommand { get; }
        public ICommand AddTextCommand { get; set; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }

        public LayerViewModel(ICanvasContext canvasViewModel)
        {
            canvas = canvasViewModel;
            lastWH.X = canvas.CanvasWidth;
            lastWH.Y = canvas.CanvasHeight;
            //Layers.CollectionChanged += (s, e) => IsModified = true;
            AddLayer(false);
            AddLayerCommand = new RelayCommand(_ => AddLayer(true));
            DeleteLayerCommand = new RelayCommand(_ => DeleteLayer());
            ToggleVisibilityCommand = new RelayCommand(_ => ToggleVisibility());
            AddTextCommand = new RelayCommand(param => AddText((SKPoint)param));
            UndoCommand = new RelayCommand(_ => Undo());
            RedoCommand = new RelayCommand(_ => Redo());
        }

        private void AddText(SKPoint cursorP)
        {
            SaveState();
            if (selectLayer == null) return;
            var text = new TextModel
            {
                Position = cursorP,
                Text = "Текст"
            };
            SelectLayer.ListText.Add(text);
            SelectText = text;
        }

        public void AddTemplate(SKRect rect, string path)
        {
            SaveState();
            if (selectLayer == null) return;
            var template = new TemplateModel
            {
                Bounds = rect,
                Path = path
            };
            SelectLayer.ListTemplate.Add(template);
            SelectTemplate = template;
        }

        public SKImage GetCompositedImage()
        {
            var surface = SKSurface.Create(new SKImageInfo((int)canvas.CanvasWidth, (int)canvas.CanvasHeight, SKColorType.Rgba8888, SKAlphaType.Premul));
            foreach (var layer in Layers)
            {
                surface.Canvas.DrawImage(layer.Image, 0, 0);
                foreach (var bubble in layer.ListTemplate)
                {
                    bubble.IsSelected = false;
                    bubble.Draw(surface.Canvas, canvas.Scale);
                }
                foreach (var text in layer.ListText)
                {
                    text.IsSelected = false;
                    text.Draw(surface.Canvas, canvas.Scale);
                }
            }
            var image = surface.Snapshot();
            surface.Dispose();
            return image;
        }

        private void AddLayer(bool saveState)
        {
            if (saveState)
                SaveState();
            var id = GetNewIdLayer(false);
            var newLayer = new LayerModel
            {
                Id = id,
                Name = $"Слой {id}",
            };
            var imageInfo = new SKImageInfo((int)canvas.CanvasWidth, (int)canvas.CanvasHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
            using (var surface = SKSurface.Create(imageInfo))
            {
                surface.Canvas.Clear(SKColors.Transparent);
                newLayer.Image = surface.Snapshot();
                surface.Dispose();
            }
            Layers.Add(newLayer);
            
            SelectLayer = newLayer;
        }

        public int GetNewIdLayer(bool reset)
        {
            if (reset) k = 0;
            k++;
            return k;
        }
        private bool flagDel = false;
        private void DeleteLayer()
        {
            if (SelectLayer != null && Layers.Count > 1)
            {
                SaveState();
                var removeLayer = SelectLayer;
                Layers.Remove(removeLayer);
                flagDel = true;
                SelectLayer = Layers.LastOrDefault();

            }
        }

        private void ToggleVisibility()
        {
            OnPropertyChanged(nameof(Layers));
        }

        public void SaveState()
        {
            if (SelectLayer != null) id = SelectLayer.Id;
            ObservableCollection<LayerModel> newLayer = new ObservableCollection<LayerModel>(Layers.Select(layer => CloneLayer(layer)));
            undoStack.Push((newLayer, id));

            redoStack.Clear();
        }

        private void Undo()
        {
            if (undoStack.Count == 0) return;
            redoStack.Push((new ObservableCollection<LayerModel>(Layers.Select(layer => CloneLayer(layer))), SelectLayer.Id));
            var tuple = undoStack.Pop();
            ObservableCollection<LayerModel> prevLayer = new ObservableCollection<LayerModel>(tuple.Item1.Select(layer => CloneLayer(layer)));
            UpdateLayers(prevLayer);
            if (Layers.Where(layer => layer.Id == tuple.Item2).ToList().Count > 0)
            {
                SelectLayer = Layers.Where(layer => layer.Id == tuple.Item2).ToList()[0];
                SelectTemplate = null;
                SelectText = null;
            }
        }

        private void Redo()
        {
            if (redoStack.Count == 0) return;
            undoStack.Push((new ObservableCollection<LayerModel>(Layers.Select(layer => CloneLayer(layer))), SelectLayer == null ? 0 : SelectLayer.Id));
            var tuple = redoStack.Pop();
            ObservableCollection<LayerModel> nextLayer = new ObservableCollection<LayerModel>(tuple.Item1.Select(layer => CloneLayer(layer)));
            UpdateLayers(nextLayer);
            if (Layers.Where(layer => layer.Id == tuple.Item2).ToList().Count > 0)
            {
                SelectLayer = Layers.Where(layer => layer.Id == tuple.Item2).ToList()[0];
                SelectTemplate = null;
                SelectText = null;
            }
        }

        private void UpdateLayers(ObservableCollection<LayerModel> layerModels)
        {
            Layers.Clear();
            foreach (var layer in layerModels.Select(layer => CloneLayer(layer)))
            {
                Layers.Add(layer);
            }
        }

        private LayerModel CloneLayer(LayerModel layerModel)
        {
            return new LayerModel
            {
                Id = layerModel.Id,
                Name = layerModel.Name,
                Image = layerModel.Image,
                IsVisible = layerModel.IsVisible,
                Opacity = layerModel.Opacity,
                ListText = [.. layerModel.ListText.Select(text => CloneText(text))],
                ListTemplate = [.. layerModel.ListTemplate.Select(bubble => CloneTemplate(bubble))]
            };
        }

        private TextModel CloneText(TextModel textModel)
        {
            return new TextModel
            {
                FontFamily = textModel.FontFamily,
                FontSize = textModel.FontSize,
                FontStyleSlant = textModel.FontStyleSlant,
                FontStyleWeight = textModel.FontStyleWeight,
                FontStyleWidth = textModel.FontStyleWidth,
                Color = textModel.Color,
                ColorStroke = textModel.ColorStroke,
                IsSelected = false,
                IsStroke = textModel.IsStroke,
                Position = textModel.Position,
                Rotate = textModel.Rotate,
                StrokeWidth = textModel.StrokeWidth,
                Text = textModel.Text
            };
        }

        private TemplateModel CloneTemplate(TemplateModel templateModel)
        {
            return new TemplateModel
            {
                Bounds = SKRect.Create(templateModel.Bounds.Location, templateModel.Bounds.Size),
                IsSelected = false,
                Path = templateModel.Path
            };
        }

        public void EnsureSurface()
        {
            if (baseSurface == null || tempSurface == null || lastWH.X != SelectLayer.Image.Info.Width || lastWH.Y != SelectLayer.Image.Info.Height)
            {
                lastWH.X = canvas.CanvasWidth;
                lastWH.Y = canvas.CanvasHeight;
                baseSurface?.Dispose();
                tempSurface?.Dispose();

                baseSurface = SKSurface.Create(SelectLayer.Image.Info);
                baseSurface.Canvas.Clear();
                tempSurface = SKSurface.Create(SelectLayer.Image.Info);
                tempSurface.Canvas.Clear();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) 
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            IsModified = true;
        }
    }
}
