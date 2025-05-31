using Mangaka_Studio.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace Mangaka_Studio.ViewModels
{
    public class TextTemplatesViewModel : INotifyPropertyChanged
    {
        private TextMode textMode = TextMode.Edit;
        public TextMode TextMode
        {
            get => textMode;
            set
            {
                textMode = value;
                OnPropertyChanged(nameof(TextMode));
            }
        }

        private float opacityBounds = 0.5f;
        public float OpacityBounds
        {
            get => opacityBounds;
            set
            {
                opacityBounds = value;
                OnPropertyChanged(nameof(OpacityBounds));
            }
        }
        private bool isHit = false;
        public bool IsHit
        {
            get => isHit;
            set
            {
                isHit = value;
                OnPropertyChanged(nameof(IsHit));
            }
        }

        private bool isStroke = false;
        public bool IsStroke
        {
            get => isStroke;
            set
            {
                isStroke = value;
                OnPropertyChanged(nameof(IsStroke));
                ApplyIsStroke();
            }
        }

        private void ApplyIsStroke()
        {
            if (frameViewModel.SelectFrame.LayerVM.SelectText != null && frameViewModel.SelectFrame.LayerVM.SelectText.IsSelected)
            {
                frameViewModel.SelectFrame.LayerVM.SelectText.IsStroke = isStroke;
                if (isStroke)
                {
                    OpacityBounds = 1;
                    IsHit = true;
                }
                else
                {
                    OpacityBounds = 0.5f;
                    IsHit = false;
                }
                frameViewModel.SelectFrame.LayerVM.OnPropertyChanged(nameof(frameViewModel.SelectFrame.LayerVM.SelectText));
            }
        }

        private float opacityBoundsTextDel = 0.5f;
        public float OpacityBoundsTextDel
        {
            get => opacityBoundsTextDel;
            set
            {
                opacityBoundsTextDel = value;
                OnPropertyChanged(nameof(OpacityBoundsTextDel));
            }
        }
        private bool isHitTextDel = false;
        public bool IsHitTextDel
        {
            get => isHitTextDel;
            set
            {
                isHitTextDel = value;
                OnPropertyChanged(nameof(IsHitTextDel));
            }
        }

        private string textModeStr = "";
        public string TextModeStr
        {
            get => textModeStr;
            set
            {
                textModeStr = value;
                OnPropertyChanged(nameof(TextModeStr));
            }
        }

        private string isVisibleEditText = "Collapsed";
        public string IsVisibleEditText
        {
            get => isVisibleEditText;
            set
            {
                isVisibleEditText = value;
                OnPropertyChanged(nameof(IsVisibleEditText));
            }
        }


        private string isVisibleMode = "Collapsed";
        public string IsVisibleMode
        {
            get => isVisibleMode;
            set
            {
                isVisibleMode = value;
                OnPropertyChanged(nameof(IsVisibleMode));
            }
        }

        public ObservableCollection<string> FontFamilies { get; set; }

        private string selectedFontFamily;
        public string SelectedFontFamily
        {
            get => selectedFontFamily;
            set
            {
                selectedFontFamily = value;
                OnPropertyChanged(nameof(SelectedFontFamily));
                ApplyFont(selectedFontFamily);
            }
        }

        private void ApplyFont(string fontName)
        {
            if (frameViewModel.SelectFrame.LayerVM.SelectText != null && frameViewModel.SelectFrame.LayerVM.SelectText.IsSelected)
            {
                frameViewModel.SelectFrame.LayerVM.SelectText.FontFamily = fontName;
                frameViewModel.SelectFrame.LayerVM.OnPropertyChanged(nameof(frameViewModel.SelectFrame.LayerVM.SelectText));
            }
        }

        public ObservableCollection<double> FontSizes { get; set; } = new ObservableCollection<double>
        {
            8, 9, 10, 11, 12, 14, 16, 18, 20, 24, 28, 32, 36, 48, 72
        };

        private double selectedFontSize = 14;
        public double SelectedFontSize
        {
            get => selectedFontSize;
            set
            {
                selectedFontSize = value;
                OnPropertyChanged(nameof(SelectedFontSize));
                ApplyFontSize(selectedFontSize);
            }
        }

        private void ApplyFontSize(double fontSize)
        {
            if (frameViewModel.SelectFrame.LayerVM.SelectText != null && frameViewModel.SelectFrame.LayerVM.SelectText.IsSelected)
            {
                frameViewModel.SelectFrame.LayerVM.SelectText.FontSize = (float)fontSize;
                frameViewModel.SelectFrame.LayerVM.OnPropertyChanged(nameof(frameViewModel.SelectFrame.LayerVM.SelectText));
            }
        }

        public ObservableCollection<int> FontWeights { get; set; } = new ObservableCollection<int>
        {
            100, 200, 300, 400, 500, 600, 700, 800, 900
        };

        private int fontStyleWeight = 400;
        public int FontStyleWeight
        {
            get => fontStyleWeight;
            set
            {
                fontStyleWeight = value;
                OnPropertyChanged(nameof(FontStyleWeight));
                ApplyFontStyleWeight();
            }
        }


        public ObservableCollection<int> FontWidths { get; set; } = new ObservableCollection<int>
        {
            1, 2, 3, 4, 5, 6, 7, 8, 9
        };

        private int fontStyleWidth = 5;
        public int FontStyleWidth
        {
            get => fontStyleWidth;
            set
            {
                fontStyleWidth = value;
                OnPropertyChanged(nameof(FontStyleWidth));
                ApplyFontStyleWidth();
            }
        }


        public ObservableCollection<FontSlantModel> FontSlants { get; set; } = new ObservableCollection<FontSlantModel>
        {
            new FontSlantModel { Value = 0, Name = "Обычный" },
            new FontSlantModel { Value = 1, Name = "Курсив" },
            new FontSlantModel { Value = 2, Name = "Наклонный" }
        };

        private int fontStyleSlant = 0;
        public int FontStyleSlant
        {
            get => fontStyleSlant;
            set
            {
                fontStyleSlant = value;
                OnPropertyChanged(nameof(FontStyleSlant));
                ApplyFontStyleSlant();
            }
        }

        private void ApplyFontStyleWeight()
        {
            if (frameViewModel.SelectFrame.LayerVM.SelectText != null && frameViewModel.SelectFrame.LayerVM.SelectText.IsSelected)
            {
                frameViewModel.SelectFrame.LayerVM.SelectText.FontStyleWeight = FontStyleWeight;
                frameViewModel.SelectFrame.LayerVM.OnPropertyChanged(nameof(frameViewModel.SelectFrame.LayerVM.SelectText));
            }
        }

        private void ApplyFontStyleWidth()
        {
            if (frameViewModel.SelectFrame.LayerVM.SelectText != null && frameViewModel.SelectFrame.LayerVM.SelectText.IsSelected)
            {
                frameViewModel.SelectFrame.LayerVM.SelectText.FontStyleWidth = FontStyleWidth;
                frameViewModel.SelectFrame.LayerVM.OnPropertyChanged(nameof(frameViewModel.SelectFrame.LayerVM.SelectText));
            }
        }

        private void ApplyFontStyleSlant()
        {
            if (frameViewModel.SelectFrame.LayerVM.SelectText != null && frameViewModel.SelectFrame.LayerVM.SelectText.IsSelected)
            {
                frameViewModel.SelectFrame.LayerVM.SelectText.FontStyleSlant = FontStyleSlant;
                frameViewModel.SelectFrame.LayerVM.OnPropertyChanged(nameof(frameViewModel.SelectFrame.LayerVM.SelectText));
            }
        }

        private readonly FrameViewModel frameViewModel;

        public ICommand SetCreateModeCommand { get; }
        public ICommand SetCancelModeCommand { get; }
        public ICommand SetDeleteModeCommand { get; }

        public TextTemplatesViewModel(FrameViewModel frameViewModel)
        {
            this.frameViewModel = frameViewModel;
            FontFamilies = new ObservableCollection<string>(
                Fonts.SystemFontFamilies.Select(ff => ff.Source).OrderBy(name => name)
            );
            SetCreateModeCommand = new RelayCommand(_ =>
            {
                TextMode = TextMode.Create;
                TextModeStr = "Выберите место для создания";
                IsVisibleEditText = "Collapsed";
                IsVisibleMode = "Visible";
                if (frameViewModel.SelectFrame.LayerVM.SelectText != null)
                {
                    frameViewModel.SelectFrame.LayerVM.SelectText.IsSelected = false;
                    frameViewModel.SelectFrame.LayerVM.SelectText = null;
                }
            });
            SetDeleteModeCommand = new RelayCommand(_ =>
            {
                TextMode = TextMode.Delete;
                IsVisibleEditText = "Collapsed";
                if (frameViewModel.SelectFrame.LayerVM.SelectText != null && frameViewModel.SelectFrame.LayerVM.SelectText.IsSelected)
                {
                    frameViewModel.SelectFrame.LayerVM.SelectLayer.ListText.Remove(frameViewModel.SelectFrame.LayerVM.SelectLayer.ListText.Where(layer => layer == frameViewModel.SelectFrame.LayerVM.SelectText).First());
                    frameViewModel.SelectFrame.LayerVM.SelectText = null;
                    frameViewModel.SelectFrame.LayerVM.SaveState();
                }
                else if (frameViewModel.SelectFrame.LayerVM.SelectTemplate != null && frameViewModel.SelectFrame.LayerVM.SelectTemplate.IsSelected)
                {
                    frameViewModel.SelectFrame.LayerVM.SelectLayer.ListTemplate.Remove(frameViewModel.SelectFrame.LayerVM.SelectLayer.ListTemplate.Where(layer => layer == frameViewModel.SelectFrame.LayerVM.SelectTemplate).First());
                    frameViewModel.SelectFrame.LayerVM.SelectTemplate = null;
                    frameViewModel.SelectFrame.LayerVM.SaveState();
                }
                TextMode = TextMode.Edit;
                TextModeStr = "";
                IsVisibleMode = "Collapsed";
                IsHitTextDel = false;
                OpacityBoundsTextDel = 0.5f;
            });
            SetCancelModeCommand = new RelayCommand(_ =>
            {
                TextMode = TextMode.Edit;
                IsVisibleEditText = "Collapsed";
                IsVisibleMode = "Collapsed";
                TextModeStr = "";
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
