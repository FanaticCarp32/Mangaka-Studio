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
            CurrentTool = new PenTool();
            canvas.CurrentTool = CurrentTool;
            SelectPenCommand = new RelayCommand(param => {
                if (param is ToolsType toolsType)
                {
                    //MessageBox.Show($"Выбран инструмент: {toolsType}");
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
            CurrentTool = toolsFactory.CreateDrawTools(type);
            canvas.CurrentTool = CurrentTool;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
