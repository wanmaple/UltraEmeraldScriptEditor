using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompileSupport.Grammar.Lexer
{
    public class Token : IToken
    {
        public int Tag => _tag;

        public Token(Int32 tag)
        {
            _tag = tag;
        }

        protected Int32 _tag;
    }
}
