using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EditorSupport.Undo
{
    public class UndoStack
    {
        public UndoStack()
        {
            _operations = new List<IUndoableOperation>();
            _undoProcess = 0;
            _undoing = false;
            _group = null;
        }

        public virtual void AddOperation(IUndoableOperation operation)
        {
            if (_undoing)
            {
                return;
            }
            if (_group != null)
            {
                _group.Operations.Add(operation);
            }
            else
            {
                _operations.RemoveRange(_undoProcess, _operations.Count - _undoProcess);
                _operations.Add(operation);
                ++_undoProcess;
            }
        }

        public virtual Boolean Undo()
        {
            if (!CanUndo())
            {
                return false;
            }
            _undoing = true;
            _operations[_undoProcess - 1].Undo();
            --_undoProcess;
            _undoing = false;
            return true;
        }

        public virtual Boolean Redo()
        {
            if (!CanRedo())
            {
                return false;
            }
            _undoing = true;
            _operations[_undoProcess].Redo();
            ++_undoProcess;
            _undoing = false;
            return true;
        }

        public virtual void Reset()
        {
            _operations.Clear();
            _undoProcess = 0;
        }

        public virtual Boolean CanUndo()
        {
            return _undoProcess > 0;
        }

        public virtual Boolean CanRedo()
        {
            return _undoProcess < _operations.Count;
        }

        public void StartGrouping()
        {
            if (_group != null)
            {
                throw new InvalidOperationException("UndoStack has already started.");
            }
            _group = new UndoOperationGroup();
        }

        public void EndGrouping()
        {
            if (_group == null)
            {
                throw new InvalidOperationException("UndoStack hasn't started yet.");
            }
            var group = _group;
            _group = null;
            AddOperation(group);
        }

        protected List<IUndoableOperation> _operations;
        protected UndoOperationGroup _group;
        protected Boolean _undoing;
        protected Int32 _undoProcess;
    }
}
