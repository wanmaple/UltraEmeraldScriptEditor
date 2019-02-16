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
            _view.Caret.DocumentOffset = _update.CaretOffsetLater;
            _view.Caret.DocumentOffset = _update.SelectionStartLater;
            _view.Caret.DocumentOffset = _update.SelectionEndLater;
        }

        public void Undo()
        {
            _view.Caret.DocumentOffset = _update.CaretOffsetEarlier;
            _view.Caret.DocumentOffset = _update.SelectionStartEarlier;
            _view.Caret.DocumentOffset = _update.SelectionEndEarlier;
        }
        #endregion

        private EditView _view;
        private EditingOffsetUpdate _update;
    }
}
