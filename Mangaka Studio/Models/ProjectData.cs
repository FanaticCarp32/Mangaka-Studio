using Mangaka_Studio.Controls.Tools;
using Mangaka_Studio.ViewModels;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mangaka_Studio.Models
{
    public class ProjectData
    {
        public List<FrameSerializable> Frames { get; set; }
        public List<List<LayerSerializable>> Layers { get; set; }
        public int SelectFrame { get; set; }
        public List<int> SelectLayer { get; set; }
        public bool GeneralView { get; set; }
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
