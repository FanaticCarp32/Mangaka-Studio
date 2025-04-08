using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mangaka_Studio.ViewModels
{
    class EraserToolSettingsViewModel : ToolsSettingsViewModel
    {
        private bool isAntialias = true;
        public bool IsAntialias
        {
            get => isAntialias;
            set
            {
                isAntialias = value;
                OnPropertyChanged(nameof(IsAntialias));
            }
        }
    }
}
