using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EditorSupport.Document
{
    /// <summary>
    /// 表示<see cref="TextDocument"/>中的一行的概念，本身并不存储文本，只存储所处<see cref="TextDocument"/>的偏移和长度。
    /// </summary>
    public sealed class DocumentLine : ISegment
    {
        public Int32 LineNumber
        {
            get { return DocumentLineTree.GetIndexFromNode(_node) + 1; }
        }

        public DocumentLine PreviousLine
        {
            get
            {
                return _node.Predecessor.Line;
            }
        }

        public DocumentLine NextLine
        {
            get
            {
                return _node.Successor.Line;
            }
        }

        #region Constructor
        internal DocumentLine()
        {
            _exactLength = _delimiterLength = 0;
        }
        #endregion

        #region ISegment
        public int StartOffset
        {
            get { return DocumentLineTree.GetOffsetFromNode(_node); }
        }

        public int Length
        {
            get { return _exactLength - _delimiterLength; }
        }

        /// <summary>
        /// 不包含换行符
        /// </summary>
        public int EndOffset
        {
            get { return StartOffset + Length; }
        }
        #endregion

        // 总长度（包含换行符）
        internal Int32 _exactLength;
        // 换行符的总长度
        internal Int32 _delimiterLength;

        internal DocumentLineTree.DocumentLineNode _node;
    }
}
