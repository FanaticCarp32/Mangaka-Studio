using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Mangaka_Studio.ViewModels
{
    public class ColorPickerViewModel : INotifyPropertyChanged
    {
        private CanvasViewModel canvas;
        private float hue = 0;
        private float saturation = 1;
        private float value = 1;
        private float cursorX = 0;
        private float cursorY = 0;
        private bool colorPalette = true;
        private bool isExpanded = false;
        private bool isUpdateColorTextBox = false;
        private bool isUpdateColor = false;
        private bool isPipetteColor = false;
        private string red;
        private string green;
        private string blue;
        private SKColor selectedColor;
        private SKColor selectedColorPalette;
        private SKColor selectedColorInverse;
        public string GroupName { get; set; } = "ColorGroup";
        public ObservableCollection<SKColor> AvailableColors { get; set; }

        public string Red
        {
            get => red;
            set
            {
                if (int.TryParse(value, out int num) && num >= 0 && num <= 255)
                {
                    red = value;
                    OnPropertyChanged(nameof(Red));
                    UpdateColorTextBox();
                }
            }
        }

        public string Green
        {
            get => green;
            set
            {
                if (int.TryParse(value, out int num) && num >= 0 && num <= 255)
                {
                    green = value;
                    OnPropertyChanged(nameof(Green));
                    UpdateColorTextBox();
                }
            }
        }

        public string Blue
        {
            get => blue;
            set
            {
                if (int.TryParse(value, out int num) && num >= 0 && num <= 255)
                {
                    blue = value;
                    OnPropertyChanged(nameof(Blue));
                    UpdateColorTextBox();
                }
            }
        }

        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
            }
        }

        public float CursorX
        {
            get => cursorX;
            set
            {
                cursorX = value;
                OnPropertyChanged(nameof(CursorX));
            }
        }

        public float CursorY
        {
            get => cursorY;
            set
            {
                cursorY = value;
                OnPropertyChanged(nameof(CursorY));
            }
        }

        public float Hue
        {
            get => hue;
            set
            {
                hue = value;
                OnPropertyChanged(nameof(Hue));
                UpdateColor();
            }
        }

        public float Saturation
        {
            get => saturation;
            set
            {
                saturation = value;
                OnPropertyChanged(nameof(Saturation));
                UpdateColor();
            }
        }

        public float Value
        {
            get => value;
            set
            {
                this.value = value;
                OnPropertyChanged(nameof(Value));
                UpdateColor();
            }
        }

        public SKColor SelectedColor
        {
            get => selectedColor;
            set
            {
                selectedColor = value;
                OnPropertyChanged(nameof(SelectedColor));
            }
        }

        public SKColor SelectedColorPalette
        {
            get => selectedColorPalette;
            set
            {
                selectedColorPalette = value;
                OnPropertyChanged(nameof(SelectedColorPalette));
            }
        }

        public SKColor SelectedColorInverse
        {
            get => selectedColorInverse;
            set
            {
                selectedColorInverse = value;
                OnPropertyChanged(nameof(SelectedColorInverse));
            }
        }

        private bool switchColor = true;
        public bool SwitchColor
        {
            get => switchColor;
            set
            {
                switchColor = value;
                OnPropertyChanged(nameof(SwitchColor));
                UpdateRGB();
            }
        }

        public ICommand GetColorButton { get; }
        public ICommand GetColorButtonPalette { get; }
        public ICommand ToggleExpandCommand { get; }
        public ICommand AddColorCommand { get; }
        public ICommand DeleteColorCommand { get; }
        public ICommand SwitchColorCommand1 { get; }
        public ICommand SwitchColorCommand2 { get; }
        public ICommand SwitchColorPipette { get; }

        public ColorPickerViewModel(CanvasViewModel canvasViewModel)
        {
            canvas = canvasViewModel;
            UpdateColor();
            CursorX = (float)197.5;
            CursorY = -2.5f;
            AvailableColors = new ObservableCollection<SKColor>
            {
                SKColors.Red, SKColors.Green, SKColors.Blue, SKColors.Black, SKColors.White, SKColors.Gray, SKColors.Yellow, SKColors.Orange, SKColors.Brown, SKColors.Violet,
                SKColors.Pink, SKColors.Purple, SKColors.Aqua, SKColors.Silver, SKColors.Coral
            };
            GetColorButton = new RelayCommand(param =>
            {
                colorPalette = false;
                if (SwitchColor)
                {
                    canvas.CurrentTool.Settings.StrokeColor = (SKColor)param;
                    isUpdateColor = true;
                    Red = canvas.CurrentTool.Settings.StrokeColor.Red.ToString();
                    Green = canvas.CurrentTool.Settings.StrokeColor.Green.ToString();
                    Blue = canvas.CurrentTool.Settings.StrokeColor.Blue.ToString();
                    isUpdateColor = false;
                }
                else
                {
                    canvas.CurrentTool.Settings.StrokeColor1 = (SKColor)param;
                    isUpdateColor = true;
                    Red = canvas.CurrentTool.Settings.StrokeColor1.Red.ToString();
                    Green = canvas.CurrentTool.Settings.StrokeColor1.Green.ToString();
                    Blue = canvas.CurrentTool.Settings.StrokeColor1.Blue.ToString();
                    isUpdateColor = false;
                }
                SelectedColorPalette = (SKColor)param;
                
            });
            GetColorButtonPalette = new RelayCommand(_ =>
            {
                colorPalette = true;
                if (SwitchColor)
                    canvas.CurrentTool.Settings.StrokeColor = SelectedColor;
                else
                    canvas.CurrentTool.Settings.StrokeColor1 = SelectedColor;
            });
            ToggleExpandCommand = new RelayCommand(_ => IsExpanded = !IsExpanded);
            AddColorCommand = new RelayCommand(_ => AddNewColor());
            DeleteColorCommand = new RelayCommand(_ => DeleteColor());
            SwitchColorCommand1 = new RelayCommand(_ => SwitchColor = true);
            SwitchColorCommand2 = new RelayCommand(_ => SwitchColor = false);
            SwitchColorPipette = new RelayCommand(param =>
            {
                SelectedColor = (SKColor)param;
                if (SwitchColor)
                {
                    canvas.CurrentTool.Settings.StrokeColor = (SKColor)param;
                    isUpdateColor = true;
                    Red = canvas.CurrentTool.Settings.StrokeColor.Red.ToString();
                    Green = canvas.CurrentTool.Settings.StrokeColor.Green.ToString();
                    Blue = canvas.CurrentTool.Settings.StrokeColor.Blue.ToString();
                    isUpdateColor = false;
                }
                else
                {
                    canvas.CurrentTool.Settings.StrokeColor1 = (SKColor)param;
                    isUpdateColor = true;
                    Red = canvas.CurrentTool.Settings.StrokeColor1.Red.ToString();
                    Green = canvas.CurrentTool.Settings.StrokeColor1.Green.ToString();
                    Blue = canvas.CurrentTool.Settings.StrokeColor1.Blue.ToString();
                    isUpdateColor = false;
                }
                UpdateColorTextBox();
            });
        }
        
        private void AddNewColor()
        {
            if (!AvailableColors.Contains(SelectedColor))
            {
                AvailableColors.Add(SelectedColor);
            }
        }

        private void DeleteColor()
        {
            if (AvailableColors.Contains(SelectedColorPalette))
            {
                AvailableColors.Remove(SelectedColorPalette);
                SelectedColorPalette = SKColors.Transparent;
            }
        }

        private void UpdateColor()
        {
            if (isUpdateColorTextBox) return;
            SelectedColor = SKColor.FromHsv(hue, saturation * 100, value * 100);
            if (colorPalette)
            {
                if (SwitchColor)
                {
                    canvas.CurrentTool.Settings.StrokeColor = SelectedColor;
                    isUpdateColor = true;
                    Red = canvas.CurrentTool.Settings.StrokeColor.Red.ToString();
                    Green = canvas.CurrentTool.Settings.StrokeColor.Green.ToString();
                    Blue = canvas.CurrentTool.Settings.StrokeColor.Blue.ToString();
                    isUpdateColor = false;
                }
                else
                {
                    canvas.CurrentTool.Settings.StrokeColor1 = SelectedColor;
                    isUpdateColor = true;
                    Red = canvas.CurrentTool.Settings.StrokeColor1.Red.ToString();
                    Green = canvas.CurrentTool.Settings.StrokeColor1.Green.ToString();
                    Blue = canvas.CurrentTool.Settings.StrokeColor1.Blue.ToString();
                    isUpdateColor = false;
                }
                
            }
            var r = (byte)(255 - SelectedColor.Red);
            var g = (byte)(255 - SelectedColor.Green);
            var b = (byte)(255 - SelectedColor.Blue);
            SelectedColorInverse = new SKColor(r, g, b, 255);
        }

        private void UpdateRGB()
        {
            isUpdateColor = true;
            if (SwitchColor)
            {
                Red = canvas.CurrentTool.Settings.StrokeColor.Red.ToString();
                Green = canvas.CurrentTool.Settings.StrokeColor.Green.ToString();
                Blue = canvas.CurrentTool.Settings.StrokeColor.Blue.ToString();
            }
            else
            {
                Red = canvas.CurrentTool.Settings.StrokeColor1.Red.ToString();
                Green = canvas.CurrentTool.Settings.StrokeColor1.Green.ToString();
                Blue = canvas.CurrentTool.Settings.StrokeColor1.Blue.ToString();
            }
            isUpdateColor = false;
        }

        private void UpdateColorTextBox()
        {
            if (!isUpdateColor && byte.TryParse(Red, out byte red) && byte.TryParse(Green, out byte green) && byte.TryParse(Blue, out byte blue))
            {
                isUpdateColorTextBox = true;
                SelectedColor = new SKColor(red, green, blue);
                Hue = SelectedColor.Hue;
                SelectedColor.ToHsv(out float h, out float s, out float v);
                Hue = h;
                Saturation = s;
                Value = v;
                CursorX = s * 2 - 2.5f;
                CursorY = (100 - v) * 2 - 2.5f;
                isUpdateColorTextBox = false;
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
