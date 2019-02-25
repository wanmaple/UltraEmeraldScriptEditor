using CompileSupport.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompileSupport.Compiler
{
    public interface ICompiler
    {
        void Compile(ISyntaxContext context);
    }
}
