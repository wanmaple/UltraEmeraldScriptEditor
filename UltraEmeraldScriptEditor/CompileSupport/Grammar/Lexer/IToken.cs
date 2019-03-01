using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompileSupport.Grammar.Lexer
{
    public interface IToken
    {
        Int32 Tag { get; }
    }
}
