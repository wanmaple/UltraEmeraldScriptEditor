using EditorSupport.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompileSupport.Matcher
{
    /// <summary>
    /// 文本匹配器。
    /// </summary>
    public interface IDocumentMatcher
    {
        /// <summary>
        /// 目标文档
        /// </summary>
        ITextSource Document { get; }
        /// <summary>
        /// 当前偏移
        /// </summary>
        Int32 CurrentOffset { get; set; }

        /// <summary>
        /// 从指定偏移处匹配
        /// </summary>
        /// <param name="startOffset"></param>
        /// <returns></returns>
        ISegment MatchFrom(Int32 startOffset);
        /// <summary>
        /// 匹配下一处
        /// </summary>
        /// <returns></returns>
        ISegment MatchNext();
    }
}
