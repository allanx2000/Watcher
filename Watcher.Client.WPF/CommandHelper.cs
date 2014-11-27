using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Watcher.Client.WPF
{
    public class CommandHelper : ICommand
    {
        private Action<object> doAction;
        private Func<object, bool> canExecute;

        public CommandHelper(Action doAction, Func<object, bool> canExecute = null)
            : this(delegate(object param) { doAction.Invoke(); }, canExecute)
        {
        }

        public CommandHelper(Action<object> doAction, Func<object, bool> canExecute = null)
        {
            if (canExecute != null)
            {
                this.canExecute = canExecute;
            }
            else this.canExecute = parameter => true;

            this.doAction = doAction;
        }


        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            doAction.Invoke(parameter);
        }

        bool ICommand.CanExecute(object parameter)
        {
            return canExecute.Invoke(parameter);
        }
    }
}
