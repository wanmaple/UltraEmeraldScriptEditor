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

        public override bool Visitable => _innerToken.Visitable;

        public override void Visit(ISyntaxContext context, IVisitRuler visitRuler, BinaryWriter writer)
        {
            if (_innerToken.Visitable)
            {
                _innerToken.Visit(context, visitRuler, writer);
            }
        }

        protected override void Visit(ISyntaxContext context, BinaryWriter writer)
        {
            // 自身没有编译逻辑
        }

        private PScriptToken _innerToken;
    }
}
