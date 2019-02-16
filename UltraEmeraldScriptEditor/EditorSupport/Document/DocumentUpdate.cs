using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EditorSupport.Document
{
    /// <summary>
    /// 文本更新数据。
    /// </summary>
    public sealed class DocumentUpdate
    {
        /// <summary>
        /// 偏移
        /// </summary>
        public Int32 Offset { get; set; }
        /// <summary>
        /// 插入文本长度
        /// </summary>
        public Int32 InsertionLength { get; set; }
        /// <summary>
        /// 删除文本长度
        /// </summary>
        public Int32 RemovalLength { get; set; }
        /// <summary>
        /// 插入的文本
        /// </summary>
        public String InsertionText { get; set; }
        /// <summary>
        /// 删除的文本
        /// </summary>
        public String RemovalText { get; set; }
        /// <summary>
        /// 需要更新的行号
        /// </summary>
        public Int32 LineNumberNeedUpdate { get; set; }
        /// <summary>
        /// 新增行的起始行号
        /// </summary>
        public Int32 NewStartLineNumber { get; set; }
        /// <summary>
        /// 新增行数
        /// </summary>
        public Int32 NewLineCount { get; set; }
        /// <summary>
        /// 删除行的起始行号
        /// </summary>
        public Int32 RemovedStartLineNumber { get; set; }
        /// <summary>
        /// 删除行数
        /// </summary>
        public Int32 RemovedLineCount { get; set; }

        public DocumentUpdate()
        {
            Offset = InsertionLength = RemovalLength = 0;
            LineNumberNeedUpdate = NewStartLineNumber = RemovedStartLineNumber = -1;
            NewLineCount = RemovedLineCount = 0;
        }
    }
}
