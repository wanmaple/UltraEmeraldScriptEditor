using CompileSupport.Syntax.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace CompileSupport.Syntax.PScript
{
    /// <summary>
    /// 宏。
    /// </summary>
    public sealed class PScriptMacro : PScriptToken
    {
        public ReadOnlyCollection<IStatement<PScriptToken>> Statements => _readonlyStatements;
        public ReadOnlyCollection<PScriptParameter> Parameters => _readonlyParameters;
        public String Name => _name;

        public PScriptMacro(String source, String name)
            : base(source)
        {
            _source = source ?? throw new ArgumentNullException("source");
            _name = name ?? throw new ArgumentNullException("name");
            _statements = new List<IStatement<PScriptToken>>();
            _readonlyStatements = new ReadOnlyCollection<IStatement<PScriptToken>>(_statements);
            _parameters = new List<PScriptParameter>();
            _readonlyParameters = new ReadOnlyCollection<PScriptParameter>(_parameters);
        }

        public override bool Visitable => throw new NotImplementedException();

        protected override void Visit(ISyntaxContext context, BinaryWriter writer)
        {
            if (context.Arguments.Count != _parameters.Count)
            {
                throw new SyntaxCheckException(String.Format(SyntaxErrorMessages.CheckArgumentCountNotMatch, context.Arguments.Count, _parameters.Count), SyntaxErrorType.SYNTAX_ERROR_ARGUMENTS, context.Document, context.CheckingOffset, context.CheckingLength);
            }
            foreach (var statement in _statements)
            {
                statement.Visit(context, new EmptyVisitRuler(), writer);
            }
        }
        
        private String _name;
        private List<IStatement<PScriptToken>> _statements;
        private ReadOnlyCollection<IStatement<PScriptToken>> _readonlyStatements;
        private List<PScriptParameter> _parameters;
        private ReadOnlyCollection<PScriptParameter> _readonlyParameters;
    }
}
