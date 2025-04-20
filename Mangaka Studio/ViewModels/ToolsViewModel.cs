using Mangaka_Studio.Models;
using Mangaka_Studio.Services;
using Mangaka_Studio.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using static Mangaka_Studio.Services.ToolsFactory;

namespace Mangaka_Studio.ViewModels
{
    public class ToolsViewModel : INotifyPropertyChanged
    {
        private DrawingTools current;
        private readonly IToolsFactory toolsFactory;
        private readonly CanvasViewModel canvas;
        private ToolsType lastEraseToolsType;

        public ToolsType LastEraseToolsType
        {
            get => lastEraseToolsType;
            set
            {
                lastEraseToolsType = value;
                OnPropertyChanged(nameof(LastEraseToolsType));
            }
        }

        public Dictionary<ToolsType, DrawingTools> Tools { get; set; } = new();
        

        public DrawingTools CurrentTool
        {
            get => current;
            set
            {
                current = value;
                OnPropertyChanged(nameof(CurrentTool));
            }
        }

        public ICommand SelectPenCommand { get; }

        public ToolsViewModel(CanvasViewModel canvas1, IToolsFactory toolsFactory1)
        {
            if (toolsFactory1 == null)
            {
                throw new ArgumentNullException(nameof(toolsFactory1), "IToolsFactory не передан");
            }
            canvas = canvas1;
            toolsFactory = toolsFactory1;
            CurrentTool = toolsFactory.CreateDrawTools(ToolsType.Pen);
            var CurrentTool1 = toolsFactory.CreateDrawTools(ToolsType.Pipette);
            LastEraseToolsType = ToolsType.HardEraser;
            Tools.Add(ToolsType.Pen, CurrentTool);
            Tools.Add(ToolsType.Pipette, CurrentTool1);
            canvas.CurrentTool = CurrentTool;
            SelectPenCommand = new RelayCommand(param => {
                if (param is ToolsType toolsType)
                {
                    SelectTool(toolsType);
                }
                else
                {
                    MessageBox.Show("Error");
                }
            });
        }

        public void SelectTool(ToolsType type)
        {
            if (CurrentTool is HardEraser)
            {
                LastEraseToolsType = ToolsType.HardEraser;
            }
            else if (CurrentTool is SoftEraser)
            {
                LastEraseToolsType = ToolsType.SoftEraser;
            }
            if (Tools.ContainsKey(type))
            {
                CurrentTool = Tools[type];
            }
            else
            {
                CurrentTool = toolsFactory.CreateDrawTools(type);
                Tools.Add(type, CurrentTool);
            }
            canvas.CurrentTool = CurrentTool;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
