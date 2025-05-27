using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mangaka_Studio.ViewModels
{
    public abstract class ToolsSettingsViewModel : INotifyPropertyChanged
    {
        private float strokeWidth = 20f;
        public virtual float StrokeWidth
        {
            get => strokeWidth;
            set
            {
                strokeWidth = value;
                OnPropertyChanged(nameof(StrokeWidth));
            }
        }
        private SKColor strokeColor = SKColors.Black;
        public virtual SKColor StrokeColor
        {
            get => strokeColor;
            set
            {
                strokeColor = value;
                OnPropertyChanged(nameof(StrokeColor));
            }
        }

        private SKColor strokeColor1 = SKColors.Black;
        public virtual SKColor StrokeColor1
        {
            get => strokeColor1;
            set
            {
                strokeColor1 = value;
                OnPropertyChanged(nameof(StrokeColor1));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
