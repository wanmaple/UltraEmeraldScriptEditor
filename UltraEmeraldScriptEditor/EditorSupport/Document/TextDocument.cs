using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace EditorSupport.Document
{
    /// <summary>
    /// 文本model的核心类。
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
            _lineTree = new DocumentLineTree(this);
            _lineMgr = new DocumentLineManager(this, _lineTree);
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

        public int IndexOfAny(char[] chars, int startIndex, int length)
        {
            VerifyAccess();
            return _rope.IndexOfAny(chars, startIndex, length);
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

        #region Text modification
        public void Append(String content)
        {
            Replace(Length, 0, content);
        }
        public void Append(ITextSource content)
        {
            Replace(Length, 0, content);
        }

        public void Insert(Int32 offset, String content)
        {
            Replace(offset, 0, content);
        }
        public void Insert(Int32 offset, ITextSource content)
        {
            Replace(offset, 0, content);
        }

        public void Remove(Int32 offset, Int32 length)
        {
            Replace(offset, length, String.Empty);
        }

        public void Replace(Int32 offset, Int32 length, String content)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }
            VerifyOffsetRange(offset);
            VerifyLengthRange(offset, length);
            _rope.Replace(offset, length, content.ToArray());
            if (length > 0)
            {
                _anchorTree.RemoveText(offset, length);
                _lineMgr.Remove(offset, length);
            }
            if (content.Length > 0)
            {
                _anchorTree.InsertText(offset, content.Length);
                _lineMgr.Insert(offset, content);
            }
#if DEBUG
            _anchorTree.VerifySelf();
            _lineTree.VerifySelf();
#endif
        }
        public void Replace(Int32 offset, Int32 length, ITextSource content)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }
            Replace(offset, length, content.Text);
        }
        #endregion

        #region Anchor operations
        public TextAnchor CreateAnchor(Int32 offset)
        {
            VerifyAccess();
            VerifyOffsetRange(offset);
            return _anchorTree.CreateAnchor(offset);
        }

        public void RemoveAnchor(TextAnchor anchor)
        {
            VerifyAccess();
            _anchorTree.RemoveAnchor(anchor);
        } 
        #endregion

        #region Validation
        private void VerifyOffsetRange(Int32 offset)
        {
            if (offset < 0 || offset > Length)
            {
                throw new ArgumentOutOfRangeException("offset", offset, "0 <= offset <= " + Length.ToString(CultureInfo.InvariantCulture));
            }
        }

        private void VerifyLengthRange(Int32 offset, Int32 length)
        {
            if (length < 0 || offset + length > Length)
            {
                throw new ArgumentOutOfRangeException("length", length, "0 <= length, offset(" + offset + ") + length <= " + Length.ToString(CultureInfo.InvariantCulture));
            }
        } 
        #endregion

        private readonly Rope<Char> _rope;
        private readonly TextAnchorTree _anchorTree;
        private readonly DocumentLineTree _lineTree;
        private readonly DocumentLineManager _lineMgr;
    }
}
