using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EditorSupport.Highlighting
{
    /// <summary>
    /// 对象高亮器
    /// </summary>
    public interface IHighlighter
    {
        void Highlight(IHighlightRuler ruler, IHighlightee highlightee);
    }
}
