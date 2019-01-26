using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EditorSupport.Document
{
    internal static class DocumentLineSeeker
    {
        internal static readonly Char[] LineChars = new Char[] { '\r', '\n', };
        internal static readonly String[] LineStrings = new String[] { "\r\n", "\r", "\n", };

        internal static SimpleSegment NextLine(String content, Int32 offset)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }
            Int32 pos = content.IndexOfAny(LineChars, offset);
            if (pos >= 0)
            {
                if (content[pos] == '\r' && pos + 1 < content.Length && content[pos + 1] == '\n')
                {
                    return new SimpleSegment(pos, 2);
                }
                return new SimpleSegment(pos, 1);
            }
            return SimpleSegment.Invalid;
        }

        internal static SimpleSegment NextLine(ITextSource content, Int32 offset)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }
            Int32 contentLength = content.Length;
            Int32 pos = content.IndexOfAny(LineChars, offset, contentLength - offset);
            if (pos >= 0)
            {
                if (content.GetCharacterAt(pos) == '\r' && pos + 1 < content.Length && content.GetCharacterAt(pos + 1) == '\n')
                {
                    return new SimpleSegment(pos, 2);
                }
                return new SimpleSegment(pos, 1);
            }
            return SimpleSegment.Invalid;
        }
    }
}
