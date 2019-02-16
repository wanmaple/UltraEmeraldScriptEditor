using EditorSupport.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EditorSupport.Undo
{
    /// <summary>
    /// 光标和选中框的撤销和恢复。
    /// </summary>
    public sealed class EditingOperation : IUndoableOperation
    {
        public EditingOperation(EditView view, EditingOffsetUpdate update)
        {
            _view = view ?? throw new ArgumentNullException("view");
            _update = update ?? throw new ArgumentNullException("update");
        }

        #region IUndoableOperation
        public void Redo()
        {
            Int32 caretMoving = _update.CaretMoving;
            Int32 selectionStartMoving = _update.SelectionStartMoving;
            Int32 selectionEndMoving = _update.SelectionEndMoving;
            MoveCaretAndSelection(caretMoving, selectionStartMoving, selectionEndMoving);
        }

        public void Undo()
        {
            Int32 caretMoving = -_update.CaretMoving;
            Int32 selectionStartMoving = -_update.SelectionStartMoving;
            Int32 selectionEndMoving = -_update.SelectionEndMoving;
            MoveCaretAndSelection(caretMoving, selectionStartMoving, selectionEndMoving);
        }
        #endregion

        private void MoveCaretAndSelection(Int32 caretMoving, Int32 selectionStartMoving, Int32 selectionEndMoving)
        {
            if (selectionStartMoving != 0 || selectionEndMoving != 0)
            {
                _view.Selection.Move(selectionStartMoving, selectionEndMoving);
            }
            if (caretMoving != 0)
            {
                _view.Caret.DocumentOffset += caretMoving;
            }
        }

        private EditView _view;
        private EditingOffsetUpdate _update;
    }
}
