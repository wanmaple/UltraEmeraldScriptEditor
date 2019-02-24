using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EditorSupport.Document;

namespace CompileSupport.Syntax.PScript
{
    public sealed class PScriptSyntaxContext : ISyntaxContext
    {
        public TextDocument Document => throw new NotImplementedException();

        public int CheckingOffset => throw new NotImplementedException();

        public int CheckingLength => throw new NotImplementedException();

        public Dictionary<string, ISyntaxToken> Constants => throw new NotImplementedException();

        public Dictionary<string, ISyntaxToken> Macros => throw new NotImplementedException();

        public Dictionary<string, ISyntaxToken> Functions => throw new NotImplementedException();

        public List<ISyntaxToken> Arguments => throw new NotImplementedException();

        public ISyntaxContext Current => throw new NotImplementedException();

        public void Pop()
        {
            throw new NotImplementedException();
        }

        public void Push()
        {
            throw new NotImplementedException();
        }
    }
}
