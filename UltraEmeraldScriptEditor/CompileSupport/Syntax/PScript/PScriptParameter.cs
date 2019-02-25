using CompileSupport.Syntax.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CompileSupport.Syntax.PScript
{
    /// <summary>
    /// 形参。
    /// </summary>
    public sealed class PScriptParameter : PScriptToken
    {
        public String Name => _name;
        public Int32 Index => _index;

        public PScriptParameter(String name, Int32 index)
            : base(name)
        {
            _name = name ?? throw new ArgumentNullException("name");
            _index = index;
            _tokenType = PScriptTokenType.Parameter;
        }

        public override bool Visitable => true;

        public override void Visit(ISyntaxContext context, IVisitRuler visitRuler, BinaryWriter writer)
        {
            if (_index < 0 || _index >= context.Current.Arguments.Count)
            {
                throw new SyntaxCheckException(String.Format(SyntaxErrorMessages.CheckParameterIndexInvalid, _index), SyntaxErrorType.SYNTAX_ERROR_ARGUMENTS, context.Document, context.CheckingOffset, context.CheckingLength);
            }
            var argument = context.Current.Arguments[_index];
            if (argument.Visitable)
            {
                argument.Visit(context, visitRuler, writer);
            }
        }

        protected override void Visit(ISyntaxContext context, BinaryWriter writer)
        {
            // 自身没有编译逻辑，只是替换参数编译而已。
        }

        private String _name;
        private Int32 _index;
    }
}
