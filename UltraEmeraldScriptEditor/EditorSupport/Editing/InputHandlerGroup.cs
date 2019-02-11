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

        public InputHandlerGroup()
        {
            _attached = false;
            _children = new ObservableCollection<IInputHandler>();
            _children.CollectionChanged += OnChildrenChanged;
        }

        #region IInputHandler
        public EditView Owner => throw new NotImplementedException();

        public void Attach()
        {
            foreach (IInputHandler handler in _children)
            {
                handler.Attach();
            }
        }

        public void Detach()
        {
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
    }
}
