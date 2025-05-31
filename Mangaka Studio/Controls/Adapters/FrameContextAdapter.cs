using Mangaka_Studio.Interfaces;
using Mangaka_Studio.Models;
using Mangaka_Studio.ViewModels;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mangaka_Studio.Controls.Adapters
{
    public class FrameContextAdapter : IFrameContext
    {
        private readonly FrameViewModel frame;
        public FrameContextAdapter(FrameViewModel frame) => this.frame = frame;
        public ObservableCollection<FrameLayerModel> Frames
        {
            get => frame.Frames;
        }

        public FrameLayerModel? SelectFrame => frame.SelectFrame;
        public LayerModel? SelectLayer => frame.SelectFrame.LayerVM.SelectLayer;
        public SKSurface? TempSurface => frame.SelectFrame.LayerVM.tempSurface;
        public LayerViewModel? LayerVM => frame.SelectFrame.LayerVM;

        public bool IsVisible => frame.SelectFrame?.LayerVM?.SelectLayer?.IsVisible == true;

        public void Draw(SKPath path, SKPaint paint) =>
            frame.SelectFrame.LayerVM.tempSurface.Canvas.DrawPath(path, paint);

        public void DrawPoint(SKPoint point, SKPaint paint) =>
            frame.SelectFrame.LayerVM.tempSurface.Canvas.DrawPoint(point, paint);

        public void CommitTempToBase()
        {
            var vm = frame.SelectFrame.LayerVM;
            vm.baseSurface.Canvas.DrawSurface(vm.tempSurface, 0, 0);
            vm.tempSurface.Canvas.Clear();
            vm.SelectLayer.Image = vm.baseSurface.Snapshot();
        }
    }

}
