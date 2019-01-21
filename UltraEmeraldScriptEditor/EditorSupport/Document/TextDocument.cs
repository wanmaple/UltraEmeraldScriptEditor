using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace EditorSupport.Document
{
    /// <summary>
    /// 文本model的核心类
    /// </summary>
    public sealed class TextDocument : ITextSource
    {
        #region Constructor
        public TextDocument()
        {
            _rope = new Rope<char>();
        }
        public TextDocument(IEnumerable<Char> initialText)
        {
            _rope = new Rope<char>(initialText);
        }
        #endregion

        #region Thread ownership
        /// <summary>
        /// 验证所处线程是否是Document自身的线程
        /// </summary>
        public void VerifyAccess()
        {
            if (Thread.CurrentThread != _ownerThread)
            {
                throw new InvalidOperationException("TextDocument can be accessed only from the thread that owns it.");
            }
        }

        private Object _lockObj = new Object();
        private Thread _ownerThread = Thread.CurrentThread;
        #endregion

        #region ITextSource
        public string Text => throw new NotImplementedException();

        public int Length => throw new NotImplementedException();

        public event EventHandler TextChanged;

        public TextReader CreateReader()
        {
            throw new NotImplementedException();
        }

        public ITextSource CreateSnapshot()
        {
            throw new NotImplementedException();
        }

        public char GetCharacterAt(int offset)
        {
            throw new NotImplementedException();
        }

        public string GetTextAt(int offset, int length)
        {
            throw new NotImplementedException();
        }

        public int IndexOfAny(char[] anyOf, int startIndex, int length)
        {
            throw new NotImplementedException();
        }
        #endregion

        private Rope<Char> _rope;
    }
}
