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
        #region Properties
        public Int32 LineNumber
        {
            get { return DocumentLineTree.GetIndexFromNode(_node) + 1; }
        }

        public DocumentLine PreviousLine
        {
            get
            {
                var predecessor = _node.Predecessor;
                if (predecessor != null)
                {
                    return _node.Predecessor.Line;
                }
                return null;
            }
        }

        public DocumentLine NextLine
        {
            get
            {
                var successor = _node.Successor;
                if (successor != null)
                {
                    return _node.Successor.Line;
                }
                return null;
            }
        }

        internal Boolean Alive { get; set; }
        #endregion

        #region Constructor
        internal DocumentLine()
        {
            _exactLength = _delimiterLength = 0;
            Alive = true;
        }
#if DEBUG
        internal DocumentLine(TextDocument doc)
            : this()
        {
            _doc = doc;
        }
#endif
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

        #region Overrides
#if DEBUG
        public override string ToString()
        {
            return _doc.GetTextAt(StartOffset, Length);
        }
#endif 
        #endregion

        // 总长度（包含换行符）
        internal Int32 _exactLength;
        // 换行符的总长度
        internal Int32 _delimiterLength;

        internal DocumentLineTree.DocumentLineNode _node;
#if DEBUG
        internal TextDocument _doc;
#endif
    }
}
