using EditorSupport.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EditorSupport.Undo
{
    /// <summary>
    /// 文本变化的撤销和恢复。
    /// </summary>
    public sealed class DocumentEditingOperation : DocumentOperation
    {
        public DocumentEditingOperation(TextDocument document, DocumentUpdate update)
            : base(document)
        {
            _update = update ?? throw new ArgumentNullException("update");
        }

        #region Overrides
        public override void Redo()
        {
            // 重新做一遍update的操作即可
            if (_update.InsertionLength > 0 && _update.RemovalLength > 0)
            {
                _document.Replace(_update.Offset, _update.RemovalLength, _update.InsertionText);
            }
            else if (_update.InsertionLength > 0)
            {
                _document.Insert(_update.Offset, _update.InsertionText);
            }
            else if (_update.RemovalLength > 0)
            {
                _document.Remove(_update.Offset, _update.RemovalLength);
            }
        }

        public override void Undo()
        {
            if (_update.InsertionLength > 0 && _update.RemovalLength > 0)
            {
                // 替换，则将删除的文本替换原来插入的文本
                _document.Replace(_update.Offset, _update.InsertionLength, _update.RemovalText);
            }
            else if (_update.InsertionLength > 0)
            {
                // 插入，则删除插入的文本
                _document.Remove(_update.Offset, _update.InsertionLength);
            }
            else if (_update.RemovalLength > 0)
            {
                // 删除，则插入删除的文本
                _document.Insert(_update.Offset, _update.RemovalText);
            }
        }
        #endregion

        private DocumentUpdate _update;
    }
}
