using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CompileSupport.Syntax.Exceptions;
using EditorSupport.Document;

namespace CompileSupport.Syntax.PScript
{
    public sealed class PScriptSyntaxChecker : ISyntaxChecker
    {
        public PScriptSyntaxChecker(TextDocument document)
        {
            _doc = document ?? throw new ArgumentNullException("document");
            _context = new PScriptSyntaxContext();
            _exception = null;
        }

        #region ISyntaxChecker
        public ISyntaxContext Context => _context;

        public TextDocument Document => _doc;

        public SyntaxCheckException Exception => _exception;

        public void Check()
        {
            ITextSource snapshot = _doc.CreateSnapshot();
        }
        #endregion

        private ISyntaxContext _context;
        private TextDocument _doc;
        private SyntaxCheckException _exception;
    }
}
