using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EditorSupport.Undo
{
    public sealed class UndoOperationGroup : IUndoableOperation
    {
        public List<IUndoableOperation> Operations => _operations;

        public UndoOperationGroup()
        {
            _operations = new List<IUndoableOperation>();
        }

        public void Redo()
        {
            foreach (var operation in _operations)
            {
                operation.Redo();
            }
        }

        public void Undo()
        {
            foreach (var operation in _operations.Reverse<IUndoableOperation>())
            {
                operation.Undo();
            }
        }

        private List<IUndoableOperation> _operations;
    }
}
