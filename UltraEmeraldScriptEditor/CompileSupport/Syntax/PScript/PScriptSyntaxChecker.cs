using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CompileSupport.Syntax.Exceptions;
using EditorSupport.Document;

namespace CompileSupport.Syntax.PScript
{
    public sealed class PScriptSyntaxChecker : ISyntaxChecker
    {
        public ISyntaxContext Context => throw new NotImplementedException();

        public TextDocument Document => throw new NotImplementedException();

        public SyntaxCheckException Exception => throw new NotImplementedException();

        public void Check()
        {
            throw new NotImplementedException();
        }
    }
}
