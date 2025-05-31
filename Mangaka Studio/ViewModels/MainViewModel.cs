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
        public FrameViewModel FrameVM { get; }
        public FileViewModel FileVM { get; }
        public TextTemplatesViewModel TextTempVM { get; }

        public MainViewModel(CanvasViewModel canvasVM, ToolsViewModel toolsVM, ColorPickerViewModel colorPickerViewModel, FrameViewModel frameViewModel, FileViewModel fileViewModel, TextTemplatesViewModel textTempVM)
        {
            CanvasVM = canvasVM;
            ToolsVM = toolsVM;
            ColorVM = colorPickerViewModel;
            FrameVM = frameViewModel;
            FileVM = fileViewModel;
            TextTempVM = textTempVM;
        }
    }
}
