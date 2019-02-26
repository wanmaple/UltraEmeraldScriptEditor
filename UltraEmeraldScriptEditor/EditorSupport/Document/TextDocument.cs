using EditorSupport.Undo;
using EditorSupport.Utils;
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
            _undoStack = new UndoStack();
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
            BeginUpdate();

            _isChanging = true;
            VerifyOffsetRange(offset);
            VerifyLengthRange(offset, length);

            if (Changing != null)
            {
                Changing(this, EventArgs.Empty);
            }

            var updates = new List<DocumentUpdate>();
            DocumentUpdate update4insertion = null, update4deletion = null;
            if (length > 0)
            {
                update4insertion = GenerateUpdate(offset, length, content);
                updates.Add(update4insertion);
            }
            if (content.Length > 0)
            {
                update4deletion = GenerateUpdate(offset, length, content);
                updates.Add(update4deletion);
            }

            _rope.Replace(offset, length, content.ToArray());
            if (length > 0)
            {
                //_anchorTree.RemoveText(offset, length);
                _lineMgr.Remove(offset, length, update4insertion);
            }
            if (content.Length > 0)
            {
                //_anchorTree.InsertText(offset, content.Length);
                _lineMgr.Insert(offset, content, update4deletion);
            }
            var e = new DocumentUpdateEventArgs(updates);
            if (Changed != null)
            {
                Changed(this, e);
            }

            if (!_undoing)
            {
                _undoStack.AddOperation(new DocumentEditingOperation(this, updates));
            }
            _isChanging = false;

            EndUpdate();
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

        private DocumentUpdate GenerateUpdate(Int32 offset, Int32 length, String content)
        {
            var docUpdate = new DocumentUpdate();
            docUpdate.Offset = offset;
            docUpdate.InsertionLength = content.Length;
            docUpdate.RemovalLength = length;
            docUpdate.InsertionText = content;
            docUpdate.RemovalText = length > 0 ? GetTextAt(offset, length) : String.Empty;

            return docUpdate;
        }

        private Boolean _isChanging = false;
        #endregion

        #region Async updating
        public event EventHandler<EventArgs> UpdateStarted;
        public event EventHandler<EventArgs> UpdateFinished;

        /// <summary>
        /// 通过<see cref="CallbackOnDispose"/>实现自动更新的逻辑
        /// </summary>
        /// <returns></returns>
        public CallbackOnDispose AutoUpdate()
        {
            BeginUpdate();
            return new CallbackOnDispose(EndUpdate);
        }

        /// <summary>
        /// 开始更新，引用计数为1时才触发真正的开始逻辑
        /// </summary>
        public void BeginUpdate()
        {
            VerifyAccess();
            if (_isChanging)
            {
                throw new InvalidOperationException("Can't change document within another document change.");
            }
            ++_updateCount;
            if (_updateCount == 1)
            {
                _undoStack.StartGrouping();
                if (UpdateStarted != null)
                {
                    UpdateStarted(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// 结束更新，引用计数为0时触发真正的结束逻辑
        /// </summary>
        public void EndUpdate()
        {
            VerifyAccess();
            if (_isChanging)
            {
                throw new InvalidOperationException("Can't change document within another document change.");
            }
            if (_updateCount <= 0)
            {
                throw new InvalidOperationException("No update is active.");
            }
            --_updateCount;
            if (_updateCount == 0)
            {
                _undoStack.EndGrouping();
                if (UpdateFinished != null)
                {
                    UpdateFinished(this, EventArgs.Empty);
                }
            }
        }

        private Int32 _updateCount = 0;
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

        #region Undo / Redo
        public void Undo()
        {
            _undoing = true;
            _undoStack.Undo();
            _undoing = false;
        }

        public void Redo()
        {
            _undoing = true;
            _undoStack.Redo();
            _undoing = false;
        }

        public Boolean CanUndo()
        {
            return _undoStack.CanUndo();
        }

        public Boolean CanRedo()
        {
            return _undoStack.CanRedo();
        }

        private Boolean _undoing = false;
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
        private UndoStack _undoStack;
    }
}
