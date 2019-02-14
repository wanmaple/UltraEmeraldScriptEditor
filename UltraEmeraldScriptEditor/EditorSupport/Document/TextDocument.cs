using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public event EventHandler<EventArgs> UpdateStarted;
        public event EventHandler<DocumentUpdateEventArgs> UpdateFinished;
        public event EventHandler<EventArgs> Changing;
        public event EventHandler<DocumentUpdateEventArgs> Changed;

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
            if (length <= 0 && content.Length <= 0)
            {
                return;
            }
            VerifyOffsetRange(offset);
            VerifyLengthRange(offset, length);

            if (Changing != null)
            {
                Changing(this, EventArgs.Empty);
            }

            var docUpdate = new DocumentUpdate();
            docUpdate.Offset = offset;
            docUpdate.InsertionLength = content.Length;
            docUpdate.RemovalLength = length;

            _rope.Replace(offset, length, content.ToArray());
            if (length > 0)
            {
                //_anchorTree.RemoveText(offset, length);
                _lineMgr.Remove(offset, length, docUpdate);
            }
            if (content.Length > 0)
            {
                //_anchorTree.InsertText(offset, content.Length);
                _lineMgr.Insert(offset, content, docUpdate);
            }
            if (Changed != null)
            {
                Changed(this, new DocumentUpdateEventArgs(docUpdate));
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

        #region Line getters
        public Int32 LineCount
        {
            get { return _lineTree.LineCount; }
        }

        public DocumentLine GetLineByNumber(Int32 lineNumber)
        {
            return _lineTree.GetLineByNumber(lineNumber);
        }

        public DocumentLine GetLineByOffset(Int32 offset)
        {
            return _lineTree.GetLineByOffset(offset);
        }

        public String GetLineText(DocumentLine line)
        {
            return GetTextAt(line.StartOffset, line.Length);
        }
        #endregion

        #region Anchor operations
        public TextAnchor CreateAnchor(Int32 offset)
        {
            VerifyAccess();
            VerifyOffsetRange(offset);
            var ret = _anchorTree.CreateAnchor(offset);
            ret.Alive = true;
            return ret;
        }

        public void RemoveAnchor(TextAnchor anchor)
        {
            VerifyAccess();
            _anchorTree.RemoveAnchor(anchor);
        }

        public void MoveAnchorLeft(TextAnchor anchor, Int32 length)
        {
            VerifyAccess();
            _anchorTree.MoveLeft(anchor._node, length);
        }

        public void MoveAnchorRight(TextAnchor anchor, Int32 length)
        {
            VerifyAccess();
            _anchorTree.MoveRight(anchor._node, length);
        }
        #endregion

        #region Locations <=> Offsets
        public TextLocation GetLocation(Int32 offset)
        {
            if (offset == 0)
            {
                return new TextLocation(1, 1);
            }
            DocumentLine docLine = GetLineByOffset(offset);
            Int32 column = offset - docLine.StartOffset + 1;
            return new TextLocation(docLine.LineNumber, column);
        }

        public Int32 GetOffset(TextLocation location)
        {
            if (location.Line == 1 && location.Column == 1)
            {
                return 0;
            }
            return GetOffset(location.Line, location.Column);
        }

        public Int32 GetOffset(Int32 line, Int32 column)
        {
            DocumentLine docLine = GetLineByNumber(line);
            if (docLine != null)
            {
                if (column <= 0)
                {
                    return docLine.StartOffset;
                }
                if (column > docLine.Length)
                {
                    return docLine.EndOffset;
                }
                return docLine.StartOffset + column - 1;
            }
            return -1;
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return Text;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return LineCount + Length * 1013;
            }
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
