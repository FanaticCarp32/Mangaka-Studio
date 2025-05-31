using Mangaka_Studio.Interfaces;
using Mangaka_Studio.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mangaka_Studio.Controls.Adapters
{
    public class ColorPickerContextAdapter : IColorPickerContext
    {
        private readonly ColorPickerViewModel color;
        public ColorPickerContextAdapter(ColorPickerViewModel vm) => color = vm;
        public bool SwitchColor
        {
            get => color.SwitchColor;
            set => color.SwitchColor = value;
        }
        public ICommand SwitchColorPipette { get => color.SwitchColorPipette; }
    }
}
