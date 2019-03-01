using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using EditorSupport.Document;

namespace CompileSupport.Syntax.PScript
{
    public sealed class PScriptStatement : IStatement<PScriptToken>
    {
        #region IStatement
        public ISyntaxContext Context => _context;
        public ReadOnlyCollection<PScriptToken> Tokens => _tokens.AsReadOnly();
        #endregion

        public PScriptKeyword Keyword => _tokens[0] as PScriptKeyword;
        public List<PScriptToken> Arguments => _arguments;
        public ISegment Segment => throw new NotImplementedException();

        public PScriptStatement(ISyntaxContext context, PScriptKeyword keyword, params PScriptToken[] arguments)
        {
            _context = context ?? throw new ArgumentNullException("context");
            _tokens = new List<PScriptToken>();
            _tokens.Add(keyword ?? throw new ArgumentNullException("keyword"));
            _tokens.AddRange(arguments);
            _arguments = new List<PScriptToken>(arguments);
        }

        public void Visit(ISyntaxContext context, IVisitRuler visitRuler, BinaryWriter writer)
        {
            foreach (var arg in _arguments)
            {
                arg.Visit(context, Keyword.VisitRuler, writer);
            }
        }

        private ISyntaxContext _context;
        private List<PScriptToken> _tokens;
        private List<PScriptToken> _arguments;
    }
}
