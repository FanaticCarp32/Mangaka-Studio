using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mangaka_Studio.ViewModels
{
    class EraserSoftToolSettingsViewModel : EraserToolSettingsViewModel
    {
        private SKBlendMode blendMode = SKBlendMode.SrcOut;
        public SKBlendMode BlendMode
        {
            get => blendMode;
            set
            {
                blendMode = value;
                OnPropertyChanged(nameof(BlendMode));
            }
        }

        private float blurRad = 5;
        public float BlurRad
        {
            get => blurRad;
            set
            {
                blurRad = value;
                OnPropertyChanged(nameof(BlurRad));
            }
        }

        private byte transparent = 50;
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
