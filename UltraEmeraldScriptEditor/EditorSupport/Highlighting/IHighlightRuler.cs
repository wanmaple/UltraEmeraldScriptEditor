using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EditorSupport.Highlighting
{
    /// <summary>
    /// 高亮规则制定器。
    /// </summary>
    /// <remarks>
    /// 仅仅只用来设置<see cref="IHighlightee"/>的Rule，高亮的样式由<see cref="IHighlighter"/>负责。
    /// </remarks>
    public interface IHighlightRuler
    {
        /// <summary>
        /// 设置Highlightee的高亮类型
        /// </summary>
        /// <param name="highlightee"></param>
        void FormulateRule(IHighlightee highlightee);

        /// <summary>
        /// 分割字符串，handler的参数分别为相对偏移和长度(长度为-1表示到结尾，这是为了避免reader一定要读完全文)
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="handler"></param>
        void SplitText(TextReader reader, Action<Int32, Int32> handler);
    }
}
