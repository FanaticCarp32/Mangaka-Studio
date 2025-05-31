using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mangaka_Studio.Models
{
    public class LayerModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public SKImage Image { get; set; }
        public bool IsVisible { get; set; } = true;
        public float Opacity { get; set; } = 1.0f;
        public List<TextModel> ListText { get; set; } = new List<TextModel>();
        public List<TemplateModel> ListTemplate { get; set; } = new List<TemplateModel>();
    }
}
