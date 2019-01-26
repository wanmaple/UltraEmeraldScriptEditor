using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EditorSupport.Document
{
    /// <summary>
    /// 可读片段。
    /// </summary>
    public interface ISegment
    {
        Int32 StartOffset { get; }
        Int32 Length { get; }
        Int32 EndOffset { get; }
    }

    public static class ISegmentExtensions
    {
        public static Boolean Contains(this ISegment segment, Int32 offset)
        {
            return offset >= segment.StartOffset && offset <= segment.EndOffset;
        }
    }
}
