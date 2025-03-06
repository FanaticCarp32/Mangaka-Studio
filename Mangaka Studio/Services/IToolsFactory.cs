using Mangaka_Studio.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Mangaka_Studio.Services.ToolsFactory;

namespace Mangaka_Studio.Services
{
    public interface IToolsFactory
    {
        DrawingTools CreateDrawTools(ToolsType tool);
    }
}
