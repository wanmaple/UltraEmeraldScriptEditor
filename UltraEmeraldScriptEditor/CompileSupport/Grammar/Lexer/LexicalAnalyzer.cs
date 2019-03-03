using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CompileSupport.Utils;
using EditorSupport.Document;

namespace CompileSupport.Grammar.Lexer
{
    public class LexicalAnalyzer : ILexer, IDisposable
    {
        public LexicalAnalyzer(ITextSource source)
        {
            _source = source ?? throw new ArgumentNullException("source");
            _reader = _source.CreateReader();
            _buffer = new DoubleBuffer(_reader);
        }

        #region ILexer
        public ITextSource Source => _source;

        public IToken Scan()
        {
            return null;
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            _reader.Dispose();
        }
        #endregion

        private ITextSource _source;
        private DoubleBuffer _buffer;
        private TextReader _reader;
    }
}
