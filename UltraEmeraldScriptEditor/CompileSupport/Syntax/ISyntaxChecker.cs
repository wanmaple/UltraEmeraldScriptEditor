using CompileSupport.Syntax.Exceptions;
using EditorSupport.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompileSupport.Syntax
{
    /// <summary>
    /// 语法检查器。
    /// </summary>
    public interface ISyntaxChecker
    {
        ISyntaxContext Context { get; }
        TextDocument Document { get; }
        SyntaxCheckException Exception { get; }

        void Check();
    }
}
