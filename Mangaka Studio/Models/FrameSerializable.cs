using Mangaka_Studio.ViewModels;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mangaka_Studio.Models
{
    public class FrameSerializable
    {
        public int Id { get; set; }
        public FrameModel Model { get; set; }
        public string Name { get; set; }
        public SKPath Bounds { get; set; }
        public bool IsDrawBounds { get; set; }
        public bool IsVisible { get; set; } 
        public bool IsSelected { get; set; }
        public FrameMode FrameMode { get; set; }
        public int BoundsWidth { get; set; }
    }
}
