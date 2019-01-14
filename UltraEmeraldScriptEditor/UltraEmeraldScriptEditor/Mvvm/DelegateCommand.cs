using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace UltraEmeraldScriptEditor.Mvvm
{
    /// <summary>
    /// 需要手动管理CanExecuteChanged事件
    /// </summary>
    public sealed class DelegateCommand : ICommand
    {
        #region ICommand
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            Boolean old = _canExecute(parameter);
            if (old != _lastCanExecute)
            {
                _lastCanExecute = old;
                if (CanExecuteChanged != null)
                {
                    CanExecuteChanged(this, new EventArgs());
                }
            }
            return _lastCanExecute.Value;
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
        #endregion

        public DelegateCommand(Action<Object> execute, Predicate<Object> canExecute = null)
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

        private Nullable<Boolean> _lastCanExecute = null;
    }
}
