using EditorSupport.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EditorSupport.Rendering
{
    /// <summary>
    /// DocumentLine的可视对象
    /// </summary>
    /// <remarks>
    /// 考虑到wpf的DrawingContext.DrawText性能堪忧，每一行又有不同样式的<see cref="VisualLineElement"/>，我们对每个没有编辑的行对象存储一份<see cref="DrawingImage"/>，在该行没有进行编辑的时候，直接用DrawingContext.DrawImage绘制，编辑中的行仍然用DrawingContext.DrawText绘制。
    /// </remarks>
    public sealed class VisualLine : IDisposable
    {
        #region Properties
        public TextDocument Document { get; private set; }

        public DocumentLine Line { get; private set; }

        public List<VisualLineElement> Elements { get; private set; }
        #endregion

        #region Constructor
        public VisualLine(TextDocument doc, DocumentLine line)
        {
            Document = doc ?? throw new ArgumentNullException("doc");
            Line = line ?? throw new ArgumentNullException("line");
            Elements = new List<VisualLineElement>();
        } 
        #endregion

        #region IDisposable
        public void Dispose()
        {
        } 
        #endregion
    }
}
