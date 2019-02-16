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
        }

        public virtual void AddOperation(IUndoableOperation operation)
        {
            _operations.RemoveRange(_undoProcess, _operations.Count - _undoProcess);
            _operations.Add(operation);
            ++_undoProcess;
        }

        public virtual Boolean Undo()
        {
            if (!CanUndo())
            {
                return false;
            }
            _operations[_undoProcess - 1].Undo();
            --_undoProcess;
            return true;
        }

        public virtual Boolean Redo()
        {
            if (!CanRedo())
            {
                return false;
            }
            _operations[_undoProcess].Redo();
            ++_undoProcess;
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

        protected List<IUndoableOperation> _operations;
        protected Int32 _undoProcess;
    }
}
