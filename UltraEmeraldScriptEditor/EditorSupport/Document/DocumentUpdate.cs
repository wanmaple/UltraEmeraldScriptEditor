using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EditorSupport.Document
{
    public sealed class DocumentUpdate
    {
        public Int32 Offset { get; set; }
        public Int32 InsertionLength { get; set; }
        public Int32 RemovalLength { get; set; }
        public Int32 LineNumberNeedUpdate { get; set; }
        public Int32 NewStartLineNumber { get; set; }
        public Int32 NewLineCount { get; set; }
        public Int32 RemovedStartLineNumber { get; set; }
        public Int32 RemovedLineCount { get; set; }

        public DocumentUpdate()
        {
            Offset = InsertionLength = RemovalLength = 0;
            LineNumberNeedUpdate = NewStartLineNumber = RemovedStartLineNumber = -1;
            NewLineCount = RemovedLineCount = 0;
        }
    }
}
