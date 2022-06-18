using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GenshinNotifier.Widget {
    /// <summary>
    /// Simplistic delegate command for the demo.
    /// </summary>
    public class SimpleCommand : ICommand {
        public Action? CommandAction { get; set; }
        public Func<bool>? CanExecuteFunc { get; set; }

        public void Execute(object parameter) {
            CommandAction?.Invoke();
        }

        public bool CanExecute(object parameter) {
            return CanExecuteFunc == null || CanExecuteFunc();
        }

        public event EventHandler CanExecuteChanged {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }



    public class DelegateCommand<T> : ICommand {
        private readonly Predicate<T>? _canExecuteMethod;
        private readonly Action<T>? _action;
        public DelegateCommand(Action<T> execute)
                    : this(execute, null) {
            _action = execute;
        }
        public DelegateCommand(Action<T>? execute, Predicate<T>? canExecute) {
            _action = execute;
            _canExecuteMethod = canExecute;
        }
        public bool CanExecute(object parameter) {
            return _canExecuteMethod == null || _canExecuteMethod((T)parameter);
        }
        public void Execute(object parameter) {
            _action?.Invoke((T)parameter);
        }
        public event EventHandler CanExecuteChanged {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
