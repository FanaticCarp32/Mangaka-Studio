using Mangaka_Studio.Controls.Tools;
using Mangaka_Studio.Interfaces;
using Mangaka_Studio.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using static Mangaka_Studio.ViewModels.ToolsFactory;

namespace Mangaka_Studio.ViewModels
{
    public class ToolsViewModel : INotifyPropertyChanged
    {
        private DrawingTools current;
        private readonly IToolsFactory toolsFactory;
        private readonly CanvasViewModel canvas;
        private IFrameContext frameContext;
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

        public ToolsViewModel(CanvasViewModel canvas, IToolsFactory toolsFactory1, IFrameContext frameContext)
        {
            if (toolsFactory1 == null)
            {
                throw new ArgumentNullException(nameof(toolsFactory1), "IToolsFactory не передан");
            }
            Mouse.OverrideCursor = Cursors.Pen;
            this.canvas = canvas;
            this.frameContext = frameContext;
            toolsFactory = toolsFactory1;
            CurrentTool = toolsFactory.CreateDrawTools(ToolsType.Pen);
            var CurrentTool1 = toolsFactory.CreateDrawTools(ToolsType.Pipette);
            LastEraseToolsType = ToolsType.HardEraser;
            Tools.Add(ToolsType.Pen, CurrentTool);
            Tools.Add(ToolsType.Pipette, CurrentTool1);
            canvas.CurrentTool = CurrentTool;
            SelectPenCommand = new RelayCommand(param =>
            {
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
            if (type is ToolsType.Pipette)
            {
                Mouse.OverrideCursor = new Cursor(Application.GetResourceStream(new Uri("pack://application:,,,/Resources/cursor.cur")).Stream);
            }
            else
            {
                Mouse.OverrideCursor = Cursors.Pen;
            }
            if (Tools.ContainsKey(type))
            {
                CurrentTool = Tools[type];
            }
            else
            {
                CurrentTool = toolsFactory.CreateDrawTools(type);
                if (CurrentTool is TextTool text)
                {
                    text.Initialize(frameContext);
                }
                Tools.Add(type, CurrentTool);
            }
            canvas.CurrentTool = CurrentTool;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
