using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EditorSupport.Document
{
    internal sealed class DocumentLineManager
    {
        internal DocumentLineManager(TextDocument document, DocumentLineTree lineTree)
        {
            _doc = document ?? throw new ArgumentNullException("document");
            _lineTree = lineTree ?? throw new ArgumentNullException("lineTree");

            Rebuild();
        }

        public void Rebuild()
        {
            DocumentLine line = _lineTree.GetLineByNumber(1);
            SimpleSegment seg = DocumentLineSeeker.NextLine(_doc, 0);
            var lines = new List<DocumentLine>();
            Int32 lastDelimeterEnd = 0;
            while (seg != SimpleSegment.Invalid)
            {
                line._exactLength = seg.StartOffset + seg.Length - lastDelimeterEnd;
                line._delimiterLength = seg.Length;
                lastDelimeterEnd = seg.StartOffset + seg.Length;
                lines.Add(line);

                line = new DocumentLine();
                seg = DocumentLineSeeker.NextLine(_doc, lastDelimeterEnd);
            }
            // 最后一行也要处理
            line._exactLength = _doc.Length - lastDelimeterEnd;
            lines.Add(line);
            _lineTree.RebuildTree(lines);
        }

        private readonly TextDocument _doc;
        private readonly DocumentLineTree _lineTree;
    }
}
