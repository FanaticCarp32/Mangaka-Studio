using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mangaka_Studio.Models
{
    public class FrameModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public SKPath Bounds { get; set; }
        public bool IsVisible { get; set; } = true;
    }
}
