﻿using System;
using System.Collections.Generic;
using System.Globalization;
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
            : this(String.Empty)
        {
        }
        public TextDocument(IEnumerable<Char> initialText)
        {
            _rope = new Rope<char>(initialText);
            _anchorTree = new TextAnchorTree(this);
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
        public string Text
        {
            get
            {
                VerifyAccess();
                return _rope.ToString(0, _rope.Count);
            }
        }

        public int Length
        {
            get
            {
                VerifyAccess();
                return _rope.Count;
            }
        }

        public TextReader CreateReader()
        {
            lock (_lockObj)
            {
                return new RopeTextReader(_rope);
            }
        }

        public ITextSource CreateSnapshot()
        {
            lock (_lockObj)
            {
                return new RopeTextSource(_rope);
            }
        }

        public char GetCharacterAt(int offset)
        {
            VerifyAccess();
            return _rope[offset];
        }

        public string GetTextAt(int offset, int length)
        {
            VerifyAccess();
            return _rope.ToString(offset, length);
        }

        public int IndexOf(String content, int startIndex, int length)
        {
            VerifyAccess();
            return _rope.IndexOfText(content, startIndex, length);
        }

        public ICollection<int> AllIndexesOf(String content, int startIndex, int length)
        {
            VerifyAccess();
            return _rope.AllIndexesOfText(content, startIndex, length);
        }
        #endregion

        public TextAnchor CreateAnchor(Int32 offset)
        {
            VerifyAccess();
            VerifyOffsetRange(offset);
            return _anchorTree.CreateAnchor(offset);
        }

        private void OnTextChanged()
        {
        }

        private void VerifyOffsetRange(Int32 offset)
        {
            if (offset < 0 || offset > Length)
            {
                throw new ArgumentOutOfRangeException("offset", offset, "0 <= offset <= " + Length.ToString(CultureInfo.InvariantCulture));
            }
        }

        private readonly Rope<Char> _rope;
        private readonly TextAnchorTree _anchorTree;
    }
}
