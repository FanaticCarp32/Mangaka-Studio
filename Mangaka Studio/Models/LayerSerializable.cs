using Mangaka_Studio.Services;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mangaka_Studio.Models
{
    public class LayerSerializable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageBase64 { get; set; }
        public bool IsVisible { get; set; }
        public float Opacity { get; set; }
    }
    public class ProjectData
    {
        public List<LayerSerializable> Layers { get; set; }
        public int CanvasWidth { get; set; }
        public int CanvasHeight { get; set; }
        public double Scale { get; set; }
        public SKPoint ScalePos { get; set; }
        public double OffsetX { get; set; }
        public double OffsetY { get; set; }
        public double Rotate { get; set; }
        public bool IsGrid { get; set; }
        public int GridSize { get; set; }
        public DrawingTools CurrentTool { get; set; }
        public ToolsType LastEraseToolsType { get; set; }
        public Dictionary<ToolsType, DrawingTools> Tools { get; set; }
    }
}
