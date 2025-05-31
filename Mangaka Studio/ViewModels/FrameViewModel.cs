using Mangaka_Studio.Interfaces;
using Mangaka_Studio.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Mangaka_Studio.ViewModels
{
    public class FrameViewModel : INotifyPropertyChanged
    {
        public int k { get; set; } = 0;
        public bool IsModified { get; set; } = false;
        private int id = 0;
        private ICanvasContext canvas;
        private Point lastWH = new Point(1000, 600);
        private ObservableCollection<FrameLayerModel> frames = new();
        public int BoundsWidthMax { get; set; } = 50;
        public int BoundsWidthMin { get; set; } = 0;
        private bool generalView = true;
        public bool GeneralView
        {
            get => generalView;
            set
            {
                generalView = value;
                OnPropertyChanged(nameof(GeneralView));
            }
        }

        public ObservableCollection<FrameLayerModel> Frames
        {
            get => frames;
            set
            {
                frames = value;
                OnPropertyChanged(nameof(Frames));
            }
        }

        private FrameLayerModel selectFrame;
        public FrameLayerModel SelectFrame
        {
            get => selectFrame;
            set
            {
                selectFrame = value;
                if (selectFrame != null)
                {
                    if (selectFrame.IsDrawBounds)
                    {
                        OpacityBounds = 1;
                        IsHit = true;
                    }
                    else
                    {
                        OpacityBounds = 0.5f;
                        IsHit = false;
                    }
                    OnPropertyChanged(nameof(BoundsWidthVM));
                    OnPropertyChanged(nameof(IsSelectedVM));
                    OnPropertyChanged(nameof(IsDrawBoundsVM));
                    OnPropertyChanged(nameof(SelectFrame));
                }
            }
        }

        private bool isExpanded = false;
        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
            }
        }

        public int BoundsWidthVM
        {
            get => SelectFrame.BoundsWidth;
            set
            {
                SelectFrame.BoundsWidth = value;
                OnPropertyChanged(nameof(BoundsWidthVM));
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

        private string config = "";
        public string Config
        {
            get => config;
            set
            {
                config = value;
                OnPropertyChanged(nameof(Config));
            }
        }

        public bool IsDrawBoundsVM
        {
            get => SelectFrame.IsDrawBounds;
            set
            {
                SelectFrame.IsDrawBounds = value;
                if (selectFrame.IsDrawBounds)
                {
                    OpacityBounds = 1;
                    IsHit = true;
                }
                else
                {
                    OpacityBounds = 0.5f;
                    IsHit = false;
                }
                OnPropertyChanged(nameof(IsDrawBoundsVM));
            }
        }

        public bool IsSelectedVM
        {
            get => SelectFrame.IsSelected;
            set
            {
                SelectFrame.IsSelected = value;
                OnPropertyChanged(nameof(IsSelectedVM));
            }
        }

        private bool isExpandedHelp;
        public bool IsExpandedHelp
        {
            get => isExpandedHelp;
            set
            {
                isExpandedHelp = value;
                OnPropertyChanged(nameof(IsExpandedHelp));
            }
        }

        public SKRect? DirtyRect { get; set; } = null;

        public ICommand AddFrameCommand { get; }
        public ICommand DeleteFrameCommand { get; }
        public ICommand ToggleVisibilityCommand { get; }
        public ICommand ToggleExpandCommand { get; }
        public ICommand ToggleExpandHelpCommand { get; }
        public ICommand SelectFrameModeCommand { get; }
        public ICommand GenerateFrameCommand { get; }

        public FrameViewModel(ICanvasContext canvasViewModel)
        {
            canvas = canvasViewModel;
            lastWH.X = canvas.CanvasWidth;
            lastWH.Y = canvas.CanvasHeight;
            var frameFactory = new FrameLayerModelFactory();
            AddFrame();
            AddFrameCommand = new RelayCommand(_ => AddFrame());
            DeleteFrameCommand = new RelayCommand(_ => DeleteFrame());
            ToggleVisibilityCommand = new RelayCommand(_ => ToggleVisibility());
            ToggleExpandCommand = new RelayCommand(_ => IsExpanded = !IsExpanded);
            ToggleExpandHelpCommand = new RelayCommand(_ => IsExpandedHelp = !IsExpandedHelp);
            SelectFrameModeCommand = new RelayCommand(param => SelectFrameMode((FrameMode)param));
            GenerateFrameCommand = new RelayCommand(_ =>
            {
                var frames = frameFactory.GenerateFromConfig(Config, canvas.CanvasWidth, canvas.CanvasHeight, canvas);
                if (frames == null) return;
                Frames.Clear();
                foreach (var frame in frames)
                {
                    Frames.Add(frame);
                }
                SelectFrame = Frames.FirstOrDefault();
            });
        }

        public SKImage GetCompositedImage()
        {
            var surface = SKSurface.Create(new SKImageInfo((int)canvas.CanvasWidth, (int)canvas.CanvasHeight, SKColorType.Rgba8888, SKAlphaType.Premul));
            foreach (var frame in Frames)
            {
                surface.Canvas.DrawImage(frame.LayerVM.GetCompositedImage(), 0, 0);
            }
            var image = surface.Snapshot();
            surface.Dispose();
            return image;
        }

        public SKImage GetCompositedFrames()
        {
            var frameSurface = SKSurface.Create(new SKImageInfo((int)canvas.CanvasWidth, (int)canvas.CanvasHeight, SKColorType.Rgba8888, SKAlphaType.Premul));
            frameSurface.Canvas.Clear(SKColors.White);
            foreach (var frame in Frames)
            {
                if (!frame.IsVisible || frame.LayerVM.Layers.Count == 0)
                    continue;
                var tempSurface = SKSurface.Create(new SKImageInfo((int)frame.Bounds.Bounds.Width, (int)frame.Bounds.Bounds.Height, SKColorType.Rgba8888, SKAlphaType.Premul));
                tempSurface.Canvas.Clear(SKColors.Transparent);
                tempSurface.Canvas.Translate(-frame.Bounds.Bounds.Left, -frame.Bounds.Bounds.Top);
                tempSurface.Canvas.ClipPath(frame.Bounds);
                tempSurface.Canvas.DrawImage(frame.LayerVM.GetCompositedImage(), 0, 0);
                if (frame.IsDrawBounds)
                {
                    using (var boundsPaint = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = (float)frame.BoundsWidth, Color = SKColors.Black, IsAntialias = true })
                    {
                        tempSurface.Canvas.DrawPath(frame.Bounds, boundsPaint);
                    }
                }
                frameSurface.Canvas.DrawSurface(tempSurface, frame.Bounds.Bounds.Left, frame.Bounds.Bounds.Top);
                tempSurface.Dispose();
            }
            var imageFrame = frameSurface.Snapshot();
            frameSurface.Dispose();
            return imageFrame;
        }

        private void SelectFrameMode(FrameMode mode)
        {
            SelectFrame.FrameMode = mode;
            if (mode == FrameMode.Rectangle)
            {
                var points = SelectFrame.Bounds.Bounds;
                var path = new SKPath();
                path.MoveTo(points.Left, points.Top);
                path.LineTo(points.Right, points.Top);
                path.LineTo(points.Right, points.Bottom);
                path.LineTo(points.Left, points.Bottom);
                path.Close();
                SelectFrame.Bounds = path;
            }
            else if (mode == FrameMode.Polyline)
            {
                var points = SelectFrame.Bounds.Bounds;
                var path = new SKPath();
                path.MoveTo(points.Left, points.Top);
                path.LineTo(points.Left + (points.Right - points.Left) / 2, points.Top);
                path.LineTo(points.Right, points.Top);
                path.LineTo(points.Right, points.Top + (points.Bottom - points.Top) / 2);
                path.LineTo(points.Right, points.Bottom);
                path.LineTo(points.Left + (points.Right - points.Left) / 2, points.Bottom);
                path.LineTo(points.Left, points.Bottom);
                path.LineTo(points.Left, points.Top + (points.Bottom - points.Top) / 2);
                path.Close();
                SelectFrame.Bounds = path;
            }
            OnPropertyChanged(nameof(IsDrawBoundsVM));
        }

        private void AddFrame()
        {
            var id = GetNewIdLayer(false);
            var path = new SKPath();
            path.MoveTo(0, 0);
            path.LineTo(canvas.CanvasWidth, 0);
            path.LineTo(canvas.CanvasWidth, canvas.CanvasHeight);
            path.LineTo(0, canvas.CanvasHeight);
            path.Close();
            var newFrame = new FrameModel
            {
                Id = id,
                Name = $"Кадр {id}",
                Bounds = path
            };
            var frameLayer = new FrameLayerModel(newFrame, canvas);
            Frames.Add(frameLayer);

            SelectFrame = frameLayer;
        }

        public int GetNewIdLayer(bool reset)
        {
            if (reset) k = 0;
            k++;
            return k;
        }

        private void DeleteFrame()
        {
            if (SelectFrame != null && Frames.Count > 1)
            {
                var removeLayer = SelectFrame;
                Frames.Remove(removeLayer);
                SelectFrame = Frames.LastOrDefault();
            }
        }

        private void ToggleVisibility()
        {
            OnPropertyChanged(nameof(Frames));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            IsModified = true;
        }
    }
}
