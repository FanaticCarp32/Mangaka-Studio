using Mangaka_Studio.Models;
using Mangaka_Studio.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Mangaka_Studio.Controls
{
    class ToolTemplateSelector : DataTemplateSelector
    {
        public DataTemplate PenToolTemplate { get; set; }
        public DataTemplate EraserToolTemplate { get; set; }
        public DataTemplate TextToolTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is PenToolSettingsViewModel) return PenToolTemplate;
            if (item is EraserToolSettingsViewModel) return EraserToolTemplate;
            if (item is TextToolSettingsViewModel) return TextToolTemplate;
            return null;
        }
    }
}
