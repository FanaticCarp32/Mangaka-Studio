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

        public MainViewModel(CanvasViewModel canvasVM, ToolsViewModel toolsVM)
        {
            CanvasVM = canvasVM;
            ToolsVM = toolsVM;
        }
    }
}
