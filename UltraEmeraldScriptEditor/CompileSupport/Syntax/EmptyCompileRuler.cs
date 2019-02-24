using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CompileSupport.Syntax
{
    public sealed class EmptyCompileRuler : ICompileRuler
    {
        public int ParamIndex => -1;

        public bool OverrideCompile => false;

        public void Compile(ISyntaxContext context, BinaryWriter writer)
        {
        }

        public void Postcompile(ISyntaxContext context, BinaryWriter writer)
        {
        }

        public void Precompile(ISyntaxContext context, BinaryWriter writer)
        {
        }
    }
}
