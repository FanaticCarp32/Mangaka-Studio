using Mangaka_Studio.Interfaces;
using Mangaka_Studio.Models;
using Mangaka_Studio.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mangaka_Studio.Controls.Adapters
{
    public class TextTemplatesContextAdapter : ITextTemplatesContext
    {
        private readonly TextTemplatesViewModel textTemp;
        public TextTemplatesContextAdapter(TextTemplatesViewModel vm) => textTemp = vm;

        public TextMode TextMode
        {
            get => textTemp.TextMode;
            set => textTemp.TextMode = value;
        }
        public string TextModeStr
        {
            get => textTemp.TextModeStr;
            set => textTemp.TextModeStr = value;
        }
        public string IsVisibleMode
        {
            get => textTemp.IsVisibleMode;
            set => textTemp.IsVisibleMode = value;
        }
        public string SelectedFontFamily
        {
            get => textTemp.SelectedFontFamily;
            set => textTemp.SelectedFontFamily = value;
        }
        public double SelectedFontSize
        {
            get => textTemp.SelectedFontSize;
            set => textTemp.SelectedFontSize = value;
        }
        public int FontStyleWeight
        {
            get => textTemp.FontStyleWeight;
            set => textTemp.FontStyleWeight = value;
        }
        public int FontStyleWidth
        {
            get => textTemp.FontStyleWidth;
            set => textTemp.FontStyleWidth = value;
        }
        public int FontStyleSlant
        {
            get => textTemp.FontStyleSlant;
            set => textTemp.FontStyleSlant = value;
        }
        public bool IsStroke
        {
            get => textTemp.IsStroke;
            set => textTemp.IsStroke = value;
        }
        public bool IsHitTextDel
        {
            get => textTemp.IsHitTextDel;
            set => textTemp.IsHitTextDel = value;
        }
        public float OpacityBoundsTextDel
        {
            get => textTemp.OpacityBoundsTextDel;
            set => textTemp.OpacityBoundsTextDel = value;
        }
        public string IsVisibleEditText
        {
            get => textTemp.IsVisibleEditText;
            set => textTemp.IsVisibleEditText = value;
        }
    }
}
