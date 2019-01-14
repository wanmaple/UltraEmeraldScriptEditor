using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace UltraEmeraldScriptEditor.Mvvm
{
    /// <summary>
    /// 代理给CommandManager去管理CanExecuteChanged事件，容易引发性能问题（比如焦点变化）
    /// </summary>
    public sealed class RelayCommand : ICommand
    {
        #region ICommand
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canExecute != null)
                {
                    CommandManager.RequerySuggested += value;
                }
            }
            remove {
                if (_canExecute != null)
                {
                    CommandManager.RequerySuggested -= value;
                }
            }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
        #endregion

        public RelayCommand(Action<Object> execute, Predicate<Object> canExecute = null)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }
            _execute = execute;
            _canExecute = canExecute;
        }

        private Action<Object> _execute;
        private Predicate<Object> _canExecute;
    }
}
