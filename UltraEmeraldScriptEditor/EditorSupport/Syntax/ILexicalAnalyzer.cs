using EditorSupport.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EditorSupport.Syntax
{
    /// <summary>
    /// 词法解析器
    /// </summary>
    public interface ILexicalAnalyzer
    {
        /// <summary>
        /// 获取下一个可用单词
        /// </summary>
        ITextSource NextAvailableWord { get; }
    }
}
