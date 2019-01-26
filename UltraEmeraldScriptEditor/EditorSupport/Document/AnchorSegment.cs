using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace EditorSupport.Document
{
    /// <summary>
    /// 使用两个锚点的文本片段。
    /// </summary>
    public sealed class AnchorSegment : ISegment
    {
        #region Constructor
        public AnchorSegment(TextAnchor startAnchor, TextAnchor endAnchor)
        {
            if (startAnchor == null)
            {
                throw new ArgumentNullException("startAnchor");
            }
            if (endAnchor == null)
            {
                throw new ArgumentNullException("endAnchor");
            }
            if (endAnchor.Offset < startAnchor.Offset)
            {
                throw new ArgumentOutOfRangeException("endAnchor", endAnchor.Offset, "endAnchor.Offset >= " + startAnchor.Offset.ToString(CultureInfo.InvariantCulture));
            }
            _startAnchor = startAnchor;
            _endAnchor = endAnchor;
        }
        #endregion

        #region ISegment
        public int StartOffset
        {
            get { return _startAnchor.Offset; }
        }

        public int Length
        {
            get { return Math.Max(0, EndOffset - StartOffset); }
        }

        public int EndOffset
        {
            get { return _endAnchor.Offset; }
        }
        #endregion

        private readonly TextAnchor _startAnchor;
        private readonly TextAnchor _endAnchor;
    }
}
