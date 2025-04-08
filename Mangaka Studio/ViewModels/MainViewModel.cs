using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mangaka_Studio.ViewModels
{
    public class MainViewModel
    {
        public CanvasViewModel CanvasVM { get; }
        public ToolsViewModel ToolsVM { get; }
        public ColorPickerViewModel ColorVM { get; }
        public LayerViewModel LayerVM { get; }

        public MainViewModel(CanvasViewModel canvasVM, ToolsViewModel toolsVM, ColorPickerViewModel colorPickerViewModel, LayerViewModel layerViewModel)
        {
            CanvasVM = canvasVM;
            ToolsVM = toolsVM;
            ColorVM = colorPickerViewModel;
            LayerVM = layerViewModel;
        }
    }
}
