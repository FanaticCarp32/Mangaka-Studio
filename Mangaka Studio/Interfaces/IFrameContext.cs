using Mangaka_Studio.Models;
using Mangaka_Studio.ViewModels;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mangaka_Studio.Interfaces
{
    public interface IFrameContext
    {
        FrameLayerModel SelectFrame { get; }
        ObservableCollection<FrameLayerModel> Frames { get; }
        public LayerModel SelectLayer { get; }
        public SKSurface TempSurface { get; }
        public LayerViewModel LayerVM { get; }
    }

}
