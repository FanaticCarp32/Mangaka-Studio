using Mangaka_Studio.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Mangaka_Studio.Controls
{
    class ToolEraseTemplateSelector : DataTemplateSelector
    {
        public DataTemplate EraserHardToolTemplate { get; set; }
        public DataTemplate EraserSoftToolTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is HardEraser) return EraserHardToolTemplate;
            if (item is SoftEraser) return EraserSoftToolTemplate;
            return null;
        }
    }
}
