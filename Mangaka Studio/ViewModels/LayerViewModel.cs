using Mangaka_Studio.Commands;
using Mangaka_Studio.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mangaka_Studio.ViewModels
{
    public class LayerViewModel : INotifyPropertyChanged
    {
        private int k = 0;
        private int id = 0;
        private CanvasViewModel canvas;
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
                selectLayer = value;
                OnPropertyChanged(nameof(SelectLayer));
            }
        }

        public SKImage Screenshot { get; set; }
        public SKSurface tempSurface { get; set; }
        public bool NeedsRedraw { get; set; } = true;

        public ICommand AddLayerCommand { get; }
        public ICommand DeleteLayerCommand { get; }
        public ICommand ToggleVisibilityCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }

        public LayerViewModel(CanvasViewModel canvasViewModel)
        {
            canvas = canvasViewModel;
            AddLayer();
            AddLayerCommand = new RelayCommand(_ => AddLayer());
            DeleteLayerCommand = new RelayCommand(_ => DeleteLayer());
            ToggleVisibilityCommand = new RelayCommand(_ => ToggleVisibility());
            UndoCommand = new RelayCommand(_ => Undo());
            RedoCommand = new RelayCommand(_ => Redo());
        }

        private void AddLayer()
        {
            SaveState();
            var newLayer = new LayerModel
            {
                Id = k,
                Name = $"Слой {k}",
                Image = SKImage.Create(new SKImageInfo((int)canvas.CanvasWidth, (int)canvas.CanvasHeight, SKColorType.Rgba8888, SKAlphaType.Unpremul))
            };
            k++;
            Layers.Add(newLayer);
            SelectLayer = newLayer;
        }

        private void DeleteLayer()
        {
            if (SelectLayer != null)
            {
                SaveState();
                var removeLayer = SelectLayer;
                Layers.Remove(removeLayer);
                SelectLayer = Layers.FirstOrDefault();

                
                OnPropertyChanged(nameof(SelectLayer));
            }
        }

        private void ToggleVisibility()
        {
            OnPropertyChanged(nameof(SelectLayer));
        }

        public void SaveState()
        {
            if (SelectLayer != null) id = SelectLayer.Id;
            
            //undoStack.Push((SelectLayer, SelectLayer.Surface.Snapshot()));
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
            }
            OnPropertyChanged(nameof(Layers));
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
            }
            OnPropertyChanged(nameof(Layers));
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
                Opacity = layerModel.Opacity
            };
        }

        private SKImage CloneSurface(SKImage image)
        {
            if (image == null) return null;

            SKImage snapshot = image;
            SKSurface clonesurface = SKSurface.Create(snapshot.Info);
            using (SKCanvas canvas = clonesurface.Canvas)
            {
                canvas.DrawImage(snapshot, 0, 0);
            }
            return clonesurface.Snapshot();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
