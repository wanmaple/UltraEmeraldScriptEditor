using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace EditorSupport.Editing
{
    public class InputHandlerGroup : IInputHandler
    {
        public ObservableCollection<IInputHandler> Children => _children;

        public InputHandlerGroup(EditView owner)
        {
            _owner = owner ?? throw new ArgumentNullException("owner");
            _attached = false;
            _children = new ObservableCollection<IInputHandler>();
            _children.CollectionChanged += OnChildrenChanged;
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

            foreach (IInputHandler handler in _children)
            {
                handler.Attach();
            }
        }

        public void Detach()
        {
            if (!_attached)
            {
                throw new InvalidOperationException("Input handler hasn't been attached yet.");
            }
            _attached = false;

            foreach (IInputHandler handler in _children)
            {
                handler.Detach();
            }
        }
        #endregion

        private void OnChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!_attached)
            {
                return;
            }
            if (e.OldItems != null)
            {
                foreach (IInputHandler handler in e.OldItems)
                {
                    handler.Detach();
                }
            }
            if (e.NewItems != null)
            {
                foreach (IInputHandler handler in e.NewItems)
                {
                    handler.Attach();
                }
            }
        }

        private ObservableCollection<IInputHandler> _children;
        private Boolean _attached;
        private EditView _owner;
    }
}
