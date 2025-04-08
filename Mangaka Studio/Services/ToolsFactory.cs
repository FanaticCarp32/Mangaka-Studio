using Mangaka_Studio.Models;
using Mangaka_Studio.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mangaka_Studio.Services
{
    public class ToolsFactory : IToolsFactory
    {
        public Dictionary<ToolsType, ToolsSettingsViewModel> ToolsSettings { get; } = new()
        {
            [ToolsType.Pen] = new PenToolSettingsViewModel(),
            [ToolsType.HardEraser] = new EraserHardToolSettingsViewModel(),
            [ToolsType.SoftEraser] = new EraserSoftToolSettingsViewModel(),
        };
        public DrawingTools CreateDrawTools(ToolsType tool)
        {
            return tool switch
            {
                ToolsType.Pen => new PenTool((PenToolSettingsViewModel)ToolsSettings[ToolsType.Pen]),
                ToolsType.HardEraser => new HardEraser((EraserHardToolSettingsViewModel)ToolsSettings[ToolsType.HardEraser]),
                ToolsType.SoftEraser => new SoftEraser((EraserSoftToolSettingsViewModel)ToolsSettings[ToolsType.SoftEraser]),
                ToolsType.Pipette => new PipetteTool((PenToolSettingsViewModel)ToolsSettings[ToolsType.Pen]),
                _ => throw new ArgumentException("Неизвестный инструмент")
            };
        }
    }

    public enum ToolsType
    {
        Pen,
        HardEraser,
        SoftEraser,
        Pipette
    }
}
