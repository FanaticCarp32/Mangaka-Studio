using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Mangaka_Studio.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> execute;
        private readonly Func<object, bool> canExecute;

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute)); 
            this.canExecute = canExecute;
        }

        public bool CanExecute(object param) => canExecute == null || canExecute(param);
        public void Execute(object param)
        {
            if (param == null)
            {
                MessageBox.Show("Ошибка: параметр команды равен null");
                return;
            }
            execute(param);
        }

        /*public void RaiseCanExecuteChange()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }*/
    }
}
