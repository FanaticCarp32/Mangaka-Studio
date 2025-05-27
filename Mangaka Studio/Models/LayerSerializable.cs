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
        public List<TextModel> ListText { get; set; }
        public List<TemplateModel> ListTemplate { get; set; }
    }
    
}
