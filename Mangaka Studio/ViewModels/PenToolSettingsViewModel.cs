using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mangaka_Studio.ViewModels
{
    public class PenToolSettingsViewModel : ToolsSettingsViewModel
    {
        private bool isSmooth = false;
        public bool IsSmooth
        {
            get => isSmooth;
            set
            {
                isSmooth = value;
                OnPropertyChanged(nameof(IsSmooth));
            }
        }

        private byte transparent = 100;
        public byte Transparent
        {
            get => transparent;
            set
            {
                transparent = value;
                OnPropertyChanged(nameof(Transparent));
            }
        }
    }
}
