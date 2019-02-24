using CompileSupport.Syntax.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CompileSupport.Syntax.PScript
{
    public sealed class PScriptKeyword : PScriptToken
    {
        public IEnumerable<PScriptDataType> ParamTypes => _paramTypes;
        public ICompileRuler CompileRuler => _compileRuler;
        public override bool Compileable => false;

        public PScriptKeyword(String source, PScriptDataType[] paramTypes)
            : base(source)
        {
            _paramTypes = paramTypes ?? throw new ArgumentNullException("paramTypes");
        }

        protected override void Compile(ISyntaxContext context, BinaryWriter writer)
        {
            throw new SyntaxCheckException(SyntaxErrorMessages.CheckNotCompileable, SyntaxErrorType.SYNTAX_ERROR_NOT_COMPILEABLE, context.Document, context.CheckingOffset, context.CheckingLength);
        }

        private ICompileRuler _compileRuler;
        private PScriptDataType[] _paramTypes;
    }
}
