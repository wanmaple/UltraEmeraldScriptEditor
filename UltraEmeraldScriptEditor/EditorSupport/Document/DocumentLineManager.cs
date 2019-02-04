using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace EditorSupport.Document
{
    internal sealed class DocumentLineManager
    {
        #region Constructor
        internal DocumentLineManager(TextDocument document, DocumentLineTree lineTree)
        {
            _doc = document ?? throw new ArgumentNullException("document");
            _lineTree = lineTree ?? throw new ArgumentNullException("lineTree");

            Rebuild();
        }
        #endregion

        #region Text operations
        internal void Rebuild()
        {
            DocumentLine line = _lineTree.GetLineByNumber(1);
            SimpleSegment seg = DocumentLineSeeker.NextLineDelimiter(_doc, 0);
            var lines = new List<DocumentLine>();
            Int32 lastDelimeterEnd = 0;
            while (seg != SimpleSegment.Invalid)
            {
                line._exactLength = seg.StartOffset + seg.Length - lastDelimeterEnd;
                line._delimiterLength = seg.Length;
                lastDelimeterEnd = seg.StartOffset + seg.Length;
                lines.Add(line);

#if DEBUG
                line = new DocumentLine(_doc);
#else
                line = new DocumentLine();
#endif
                seg = DocumentLineSeeker.NextLineDelimiter(_doc, lastDelimeterEnd);
            }
            // 最后一行也要处理
            line._exactLength = _doc.Length - lastDelimeterEnd;
            lines.Add(line);
            _lineTree.RebuildTree(lines);
        }

        internal void Insert(Int32 offset, String text)
        {
            DocumentLine line = _lineTree.GetLineByOffset(offset);
            // 不允许在\r和\n之间插入文本
            SimpleSegment seg = DocumentLineSeeker.NextLineDelimiter(text, 0);
            if (seg == SimpleSegment.Invalid)
            {
                // 插入的文本没有新的行，直接加入当前行
                SetLineLength(line, line._exactLength + text.Length);
                return;
            }
            // 和前置行合并
            Int32 lineOffset = line.StartOffset;
            Int32 lineBreakOffset = offset + seg.EndOffset;
            Int32 lengthAfterInsertion = lineOffset + line._exactLength - offset;
            Int32 delimiterLengthAfterInsertion = line._delimiterLength;
            line._delimiterLength = seg.Length;
            SetLineLength(line, lineBreakOffset - lineOffset);
            Int32 lastDelimeterEnd = seg.EndOffset;
            seg = DocumentLineSeeker.NextLineDelimiter(text, lastDelimeterEnd);
            // 中间行处理
            while (seg != SimpleSegment.Invalid)
            {
#if DEBUG
                var newLine = new DocumentLine(_doc);
#else
                var newLine = new DocumentLine();
#endif
                newLine._delimiterLength = seg.Length;
                newLine._exactLength = seg.EndOffset - lastDelimeterEnd;
                _lineTree.InsertLineAfter(line, newLine);
                line = newLine;
                lastDelimeterEnd = seg.EndOffset;
                seg = DocumentLineSeeker.NextLineDelimiter(text, lastDelimeterEnd);
            }
            // 和后置行合并
#if DEBUG
            var afterLine = new DocumentLine(_doc);
#else
            var afterLine = new DocumentLine();
#endif
            afterLine._delimiterLength = delimiterLengthAfterInsertion;
            afterLine._exactLength = lengthAfterInsertion + (text.Length - lastDelimeterEnd);
            _lineTree.InsertLineAfter(line, afterLine);
        }

        internal void Remove(Int32 offset, Int32 length)
        {
            if (offset == 0 && length == _doc.Length)
            {
                // 全删，直接重置
                _lineTree.Clear();
                return;
            }
            DocumentLine line = _lineTree.GetLineByOffset(offset);
            // 不允许在\r和\n之间删除文本
            Int32 lineOffset = line.StartOffset;
            Int32 lengthAfterDeletion = lineOffset + line._exactLength - offset;
            if (length < lengthAfterDeletion)
            {
                // 不需要删除行
                SetLineLength(line, line._exactLength - length);
                return;
            }
            DocumentLine firstLine = line;
            Int32 lengthBeforeDeletion = offset - lineOffset;
            DocumentLine nextLine = line.NextLine;
            if (lengthBeforeDeletion == 0)
            {
                // 当前行需要删除
                _lineTree.RemoveLine(line);
                firstLine = null;
            }
            else
            {
                SetLineLength(line, line._exactLength - lengthAfterDeletion);
                line._delimiterLength = 0;
            }
            line = nextLine;
            length -= lengthAfterDeletion;
            // 删除中间行
            while (length > 0)
            {
                if (length < line._exactLength)
                {
                    break;
                }
                nextLine = line.NextLine;
                _lineTree.RemoveLine(line);
                length -= line._exactLength;
                line = nextLine;
            }
            if (line != null)
            {
                if (firstLine != null)
                {
                    // 前置行没有删除，则合并这两行
                    SetLineLength(firstLine, firstLine._exactLength + (line._exactLength - length));
                    firstLine._delimiterLength = line._delimiterLength;
                    _lineTree.RemoveLine(line);
                }
                else
                {
                    // 没有前置行了，保留该行
                    SetLineLength(line, line._exactLength - length);
                }
            }
        }

        internal void SetLineLength(DocumentLine line, Int32 lineLength)
        {
            line._exactLength = lineLength;
            _lineTree.UpdateNodeData(line._node);
        }
        #endregion

        private readonly TextDocument _doc;
        private readonly DocumentLineTree _lineTree;
    }
}
