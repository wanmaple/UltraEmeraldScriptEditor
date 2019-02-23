using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace CompileSupport.Syntax.PScript
{
    public sealed class PScriptStatement : IStatement<PScriptToken>
    {
        #region IStatement
        public SyntaxContext Context => _context;
        public ReadOnlyCollection<PScriptToken> Tokens => _tokens.AsReadOnly();
        #endregion

        public PScriptKeyword Keyword => _tokens[0] as PScriptKeyword;
        public List<PScriptToken> Arguments => _arguments;

        public PScriptStatement(SyntaxContext context, PScriptKeyword keyword, params PScriptToken[] arguments)
        {
            _context = context ?? throw new ArgumentNullException("context");
            _tokens = new List<PScriptToken>();
            _tokens.Add(keyword ?? throw new ArgumentNullException("keyword"));
            _tokens.AddRange(arguments);
            _arguments = new List<PScriptToken>(arguments);
        }

        public void Compile(BinaryWriter writer)
        {
            // 关键字不参与编译
        }

        #region Overrides
        public override string ToString()
        {
            return base.ToString();
        }
        #endregion

        private SyntaxContext _context;
        private List<PScriptToken> _tokens;
        private List<PScriptToken> _arguments;
    }
}
