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
        }

        public override bool Compileable => true;

        public override void Compile(ISyntaxContext context, ICompileRuler compileRuler, BinaryWriter writer)
        {
            if (_index < 0 || _index >= context.Arguments.Count)
            {
                throw new SyntaxCheckException(String.Format(SyntaxErrorMessages.CheckParameterIndexInvalid, _index), SyntaxErrorType.SYNTAX_ERROR_ARGUMENTS, context.Document, context.CheckingOffset, context.CheckingLength);
            }
            var argument = context.Arguments[_index];
            if (argument.Compileable)
            {
                argument.Compile(context, compileRuler, writer);
            }
        }

        protected override void Compile(ISyntaxContext context, BinaryWriter writer)
        {
            // 自身没有编译逻辑，只是替换参数编译而已。
        }

        private String _name;
        private Int32 _index;
    }
}
