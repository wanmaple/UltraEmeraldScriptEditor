using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CompileSupport.Syntax.PScript
{
    /// <summary>
    /// 包装器。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PScriptWrapper<T> : PScriptToken
        where T : PScriptToken
    {
        public PScriptWrapper(String source, T innerToken)
            : base(source)
        {
            _innerToken = innerToken?? throw new ArgumentNullException("innerToken");
        }

        public override bool Compileable => _innerToken.Compileable;

        public override void Compile(ISyntaxContext context, ICompileRuler compileRuler, BinaryWriter writer)
        {
            if (_innerToken.Compileable)
            {
                _innerToken.Compile(context, compileRuler, writer);
            }
        }

        protected override void Compile(ISyntaxContext context, BinaryWriter writer)
        {
            // 自身没有编译逻辑
        }

        private PScriptToken _innerToken;
    }
}
