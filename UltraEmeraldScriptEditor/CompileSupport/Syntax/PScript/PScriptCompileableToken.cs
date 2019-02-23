using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CompileSupport.Syntax.PScript
{
    /// <summary>
    /// 指参与编译的元素。
    /// </summary>
    public abstract class PScriptCompileableToken : PScriptToken, ICompileable
    {
        protected abstract void Compile(SyntaxContext context, BinaryWriter writer);

        protected PScriptCompileableToken(string source) 
            : base(source)
        {
        }

        public virtual void Compile(SyntaxContext context, ICompileRuler compileRuler, BinaryWriter writer)
        {
            compileRuler.Precompile(context, writer);
            if (compileRuler.OverrideCompile)
            {
                compileRuler.Compile(context, writer);
            }
            compileRuler.Postcompile(context, writer);
        }
    }
}
