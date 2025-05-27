using Mangaka_Studio.Interfaces;
using Mangaka_Studio.ViewModels;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Mangaka_Studio.Models
{
    public class FrameLayerModel
    {
        public int Id { get; set; }
        public FrameModel Model { get; set; }
        public LayerViewModel LayerVM { get; set; }
        public string Name { get; set; }
        public SKPath Bounds { get; set; }
        public bool IsDrawBounds { get; set; } = false;
        public bool IsVisible { get; set; } = true;
        public bool IsSelected { get; set; } = true;
        public float HandleSize { get; } = 10;
        public FrameMode FrameMode { get; set; } = FrameMode.Rectangle;
        public int BoundsWidth { get; set; } = 2;


        public FrameLayerModel (FrameModel model, ICanvasContext canvas)
        {
            Model = model;
            Id = model.Id;
            Name = model.Name;
            Bounds = model.Bounds;
            IsVisible = model.IsVisible;
            LayerVM = new LayerViewModel(canvas)
            {
                //Layers = model.Layers
            };
        }
    }
}
