using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TKD.App.Controllers
{
    class CommandHandler : ICommand
    {
        private Action<object> _action;
        private Func<bool> _canExecute;
        public event EventHandler CanExecuteChanged;

        public CommandHandler(Action<object> action, Func<bool> canExecute)
        {
            _action = action;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute.Invoke();
        }

        public void Execute(object parameter)
        {
            _action(parameter);
        }
    }
}
