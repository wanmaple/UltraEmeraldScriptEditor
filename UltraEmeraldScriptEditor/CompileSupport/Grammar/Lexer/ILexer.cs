using EditorSupport.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompileSupport.Grammar.Lexer
{
    /// <summary>
    /// 词法分析器(Lexical Analyzer)。
    /// </summary>
    public interface ILexer
    {
        ITextSource Source { get; }

        IToken Scan();
    }
}
