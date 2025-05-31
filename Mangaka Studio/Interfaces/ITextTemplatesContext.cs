using Mangaka_Studio.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mangaka_Studio.Interfaces
{
    public interface ITextTemplatesContext
    {
        TextMode TextMode { get; set; }
        string TextModeStr { get; set; }
        string IsVisibleMode { get; set; }
        string SelectedFontFamily { get; set; }
        double SelectedFontSize { get; set; }
        int FontStyleWeight { get; set; }
        int FontStyleWidth { get; set; }
        int FontStyleSlant { get; set; }
        bool IsStroke { get; set; }
        bool IsHitTextDel { get; set; }
        float OpacityBoundsTextDel { get; set; }
        string IsVisibleEditText { get; set; }
    }
}
