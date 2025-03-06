using Mangaka_Studio.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mangaka_Studio.Services
{
    public class ToolsFactory : IToolsFactory
    {
        public DrawingTools CreateDrawTools(ToolsType tool)
        {
            return tool switch
            {
                ToolsType.Pen => new PenTool(),
                ToolsType.Eraser => new EraserTool(),
                _ => throw new ArgumentException("Неизвестный инструмент")
            };
        }

        
    }

    public enum ToolsType
    {
        Pen,
        Eraser
    }
}
