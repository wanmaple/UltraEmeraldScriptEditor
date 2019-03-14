using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace EditorSupport.Editing
{
    public class InputCommandsHandler : IInputHandler
    {
        public ObservableCollection<CommandBinding> CommandBindings => _commandBindings;
        public ObservableCollection<InputBinding> InputBindings => _inputBindings;

        public InputCommandsHandler(EditView owner)
        {
            _owner = owner ?? throw new ArgumentNullException("owner");
            _commandBindings = new ObservableCollection<CommandBinding>();
            _commandBindings.CollectionChanged += OnCommandBindingsChanged;
            _inputBindings = new ObservableCollection<InputBinding>();
            _inputBindings.CollectionChanged += OnInputBindingsChanged;
            _attached = false;
        }

        #region IInputHandler
        public EditView Owner => _owner;

        public void Attach()
        {
            if (_attached)
            {
                throw new InvalidOperationException("Input handler has already attached.");
            }
            _attached = true;

            _owner.CommandBindings.AddRange(_commandBindings);
            _owner.InputBindings.AddRange(_inputBindings);
        }

        public void Detach()
        {
            if (!_attached)
            {
                throw new InvalidOperationException("Input handler hasn't been attached yet.");
            }
            _attached = false;

            foreach (CommandBinding binding in _commandBindings)
            {
                _owner.CommandBindings.Remove(binding);
            }
            foreach (InputBinding binding in _inputBindings)
            {
                _owner.InputBindings.Remove(binding);
            }
        }
        #endregion

        private void OnInputBindingsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!_attached)
            {
                return;
            }
            if (e.OldItems != null)
            {
                foreach (InputBinding binding in e.OldItems)
                {
                    _owner.InputBindings.Remove(binding);
                }
            }
            _owner.InputBindings.AddRange(e.NewItems);
        }

        private void OnCommandBindingsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!_attached)
            {
                return;
            }
            if (e.OldItems != null)
            {
                foreach (CommandBinding binding in e.OldItems)
                {
                    _owner.CommandBindings.Remove(binding);
                }
            }
            _owner.CommandBindings.AddRange(e.NewItems);
        }

        private ObservableCollection<CommandBinding> _commandBindings;
        private ObservableCollection<InputBinding> _inputBindings;
        private EditView _owner;
        private Boolean _attached;
    }
}
