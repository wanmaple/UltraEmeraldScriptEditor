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
        public DocumentEditingOperation(TextDocument document, IEnumerable<DocumentUpdate> updates)
            : base(document)
        {
            _updates = updates ?? throw new ArgumentNullException("updates");
        }

        #region Overrides
        public override void Redo()
        {
            foreach (var update in _updates)
            {
                // 重新做一遍update的操作即可
                if (update.InsertionLength > 0 && update.RemovalLength > 0)
                {
                    _document.Replace(update.Offset, update.RemovalLength, update.InsertionText);
                }
                else if (update.InsertionLength > 0)
                {
                    _document.Insert(update.Offset, update.InsertionText);
                }
                else if (update.RemovalLength > 0)
                {
                    _document.Remove(update.Offset, update.RemovalLength);
                }
            }
        }

        public override void Undo()
        {
            foreach (var update in _updates.Reverse())
            {
                if (update.InsertionLength > 0 && update.RemovalLength > 0)
                {
                    // 替换，则将删除的文本替换原来插入的文本
                    _document.Replace(update.Offset, update.InsertionLength, update.RemovalText);
                }
                else if (update.InsertionLength > 0)
                {
                    // 插入，则删除插入的文本
                    _document.Remove(update.Offset, update.InsertionLength);
                }
                else if (update.RemovalLength > 0)
                {
                    // 删除，则插入删除的文本
                    _document.Insert(update.Offset, update.RemovalText);
                }
            }
        }
        #endregion

        private IEnumerable<DocumentUpdate> _updates;
    }
}
