using Mangaka_Studio.Controls.Tools;
using Mangaka_Studio.Interfaces;
using Mangaka_Studio.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mangaka_Studio.ViewModels
{
    public class ToolsFactory : IToolsFactory
    {
        public ToolsFactory()
        {

            ToolsSettings = new Dictionary<ToolsType, ToolsSettingsViewModel>
            {
                [ToolsType.Pen] = new PenToolSettingsViewModel(),
                [ToolsType.HardEraser] = new EraserHardToolSettingsViewModel(),
                [ToolsType.SoftEraser] = new EraserSoftToolSettingsViewModel(),
                [ToolsType.Text] = new TextToolSettingsViewModel()
            };
        }

        public Dictionary<ToolsType, ToolsSettingsViewModel> ToolsSettings { get; }
        public DrawingTools CreateDrawTools(ToolsType tool)
        {
            return tool switch
            {
                ToolsType.Pen => new PenTool((PenToolSettingsViewModel)ToolsSettings[tool]),
                ToolsType.HardEraser => new HardEraser((EraserHardToolSettingsViewModel)ToolsSettings[tool]),
                ToolsType.SoftEraser => new SoftEraser((EraserSoftToolSettingsViewModel)ToolsSettings[tool]),
                ToolsType.Pipette => new PipetteTool((PenToolSettingsViewModel)ToolsSettings[ToolsType.Pen]),
                ToolsType.Text => new TextTool((TextToolSettingsViewModel)ToolsSettings[tool]),
                _ => throw new ArgumentException("Неизвестный инструмент")
            };
        }
    }
}
