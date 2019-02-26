using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CompileSupport.Syntax.Exceptions;
using EditorSupport.Document;

namespace CompileSupport.Syntax.PScript
{
    public sealed class PScriptSyntaxChecker : ISyntaxChecker, IDisposable
    {
        public PScriptSyntaxChecker(TextDocument document)
        {
            _doc = document ?? throw new ArgumentNullException("document");
            _reader = _doc.CreateReader();
            _context = new PScriptSyntaxContext();
            _buffer = new Char[1024];
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

        #region IDisposable
        public void Dispose()
        {
            if (_reader != null)
            {
                _reader.Dispose();
            }
        } 
        #endregion

        private ISyntaxContext _context;
        private TextDocument _doc;
        private SyntaxCheckException _exception;
        private TextReader _reader;
        private Char[] _buffer;
    }
}
