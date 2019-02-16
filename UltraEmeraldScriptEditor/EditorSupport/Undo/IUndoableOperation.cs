using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EditorSupport.Undo
{
    /// <summary>
    /// 指可以撤销和恢复的操作。
    /// </summary>
    public interface IUndoableOperation
    {
        /// <summary>
        /// 撤销
        /// </summary>
        void Undo();

        /// <summary>
        /// 恢复
        /// </summary>
        void Redo();
    }
}
