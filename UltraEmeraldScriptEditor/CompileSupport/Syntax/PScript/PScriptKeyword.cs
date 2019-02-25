using CompileSupport.Syntax.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CompileSupport.Syntax.PScript
{
    /// <summary>
    /// 关键字。
    /// </summary>
    public sealed class PScriptKeyword : PScriptToken
    {
        public IEnumerable<PScriptDataType> ParamTypes => _paramTypes;
        public IVisitRuler VisitRuler => _visitRuler;
        public override bool Visitable => false;

        public PScriptKeyword(String source, PScriptDataType[] paramTypes)
            : base(source)
        {
            _paramTypes = paramTypes ?? throw new ArgumentNullException("paramTypes");
            _tokenType = PScriptTokenType.Keyword;
        }

        protected override void Visit(ISyntaxContext context, BinaryWriter writer)
        {
            throw new SyntaxCheckException(SyntaxErrorMessages.CheckNotCompileable, SyntaxErrorType.SYNTAX_ERROR_NOT_COMPILEABLE, context.Document, context.CheckingOffset, context.CheckingLength);
        }

        private IVisitRuler _visitRuler;
        private PScriptDataType[] _paramTypes;
    }
}
