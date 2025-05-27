using Mangaka_Studio.Controls.Tools;
using Mangaka_Studio.Models;
using Mangaka_Studio.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Mangaka_Studio.ViewModels.ToolsFactory;

namespace Mangaka_Studio.Interfaces
{
    public interface IToolsFactory
    {
        DrawingTools CreateDrawTools(ToolsType tool);
    }
}
