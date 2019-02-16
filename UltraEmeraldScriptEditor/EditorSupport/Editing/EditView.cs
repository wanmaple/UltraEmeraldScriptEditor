using EditorSupport.Document;
using EditorSupport.Rendering;
using EditorSupport.Undo;
using EditorSupport.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EditorSupport.Editing
{
    /// <summary>
    /// 编辑框。给Content提供编辑支持，Content需要实现<see cref="IEditInfo"/>
    /// </summary>
    public sealed class EditView : ScrollViewer, IWeakEventListener
    {
        #region Properties
        public static readonly DependencyProperty DocumentProperty =
    DependencyProperty.Register("Document", typeof(TextDocument), typeof(EditView), new PropertyMetadata(new TextDocument(), OnDocumentChanged));
        public static readonly DependencyProperty CanContentEditProperty =
            DependencyProperty.Register("CanContentEdit", typeof(Boolean), typeof(EditView), new PropertyMetadata(false));

        public TextDocument Document
        {
            get { return (TextDocument)GetValue(DocumentProperty); }
            set { SetValue(DocumentProperty, value); }
        }
        public Boolean CanContentEdit
        {
            get { return (Boolean)GetValue(CanContentEditProperty); }
            set { SetValue(CanContentEditProperty, value); }
        }

        public Caret Caret => _caret;
        public Selection Selection => _selection;

        public IInputHandler ActiveInputHandler
        {
            get => _activeInputHandler;
            set
            {
                if (value != null && value.Owner != this)
                {
                    throw new InvalidOperationException("The handler's owner is not the attaching view.");
                }
                if (_activeInputHandler != value)
                {
                    if (_activeInputHandler != null)
                    {
                        _activeInputHandler.Detach();
                    }
                    _activeInputHandler = value;
                    if (_activeInputHandler != null)
                    {
                        _activeInputHandler.Attach();
                    }
                }
            }
        }
        #endregion

        #region Constructor
        public EditView()
        {
            _renderContext = new RenderContext();

            _caret = new Caret(this);
            _caret.PositionChanged += OnCaretPositionChanged;
            _selection = new AnchorSelection(this, Document.CreateAnchor(0), Document.CreateAnchor(0));
            _selection.OffsetChanged += OnSelectionOffsetChanged;
            _undoStack = new UndoStack();

            Loaded += (s, e) =>
            {
                Cursor = Cursors.IBeam;
                CreateDefaultInputHandler();
            };
        }
        #endregion

        #region Overrides
        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            _renderContext.Region = new Rect(new Point(0, 0), arrangeBounds);

            return base.ArrangeOverride(arrangeBounds);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (_selection != null)
            {
                _selection.Render(drawingContext, _renderContext);
            }
            if (_caret != null)
            {
                _caret.Render(drawingContext, _renderContext);
            }
        }

        protected override void OnScrollChanged(ScrollChangedEventArgs e)
        {
            Measure();
            Redraw();
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            _caret.Visible = true;
            Redraw();
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            _caret.Visible = false;
            Redraw();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            foreach (var handler in _mouseLeftDownHandlers)
            {
                handler(e);
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            foreach (var handler in _mouseLeftUpHandlers)
            {
                handler(e);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            foreach (var handler in _mouseMoveHandlers)
            {
                handler(e);
            }
        }

        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            base.OnTextInput(e);
            BeginUpdating();
            InsertText(e.Text);
            EndUpdating();
            Redraw();
        }
        #endregion

        #region Caret / Selection
        public void InsertText(String content)
        {
            if (String.IsNullOrEmpty(content))
            {
                return;
            }
            if (_selection.IsEmpty)
            {
                Document.Insert(_selection.StartOffset, content);
            }
            else
            {
                Int32 offset = _selection.StartOffset;
                Int32 length = _selection.Length;
                _caret.DocumentOffset = _selection.StartOffset;
                SelectionFollowCaret(false, FlowDirection.LeftToRight);
                Document.Replace(offset, length, content);
            }
            _caret.MoveRight(content.Length);
            SelectionFollowCaret(false, FlowDirection.LeftToRight);
            MoveCaretInVisual();
        }

        public void InsertLine(String content)
        {
            if (String.IsNullOrEmpty(content))
            {
                return;
            }
            DocumentLine line = Document.GetLineByOffset(_caret.DocumentOffset);
            Document.Insert(line.StartOffset, content);
            _caret.DocumentOffset += content.Length;     // 光标位置不变
            SelectionFollowCaret(false, FlowDirection.LeftToRight);
            MoveCaretInVisual();
        }

        public void RemoveSelection()
        {
            if (_selection.IsEmpty)
            {
                return;
            }
            Document.Remove(_selection.StartOffset, _selection.Length);
            _caret.DocumentOffset = _selection.StartOffset;
            SelectionFollowCaret(false, FlowDirection.LeftToRight);
        }

        public void TabForward()
        {
            InsertText(CommonUtilities.Tab);
            MoveCaretInVisual();
        }

        public void TabBackward()
        {
            TextLocation location = Document.GetLocation(_selection.StartOffset);
            Int32 backwardLength = Math.Min(location.Column, CommonUtilities.Tab.Length);
            String backwardText = Document.GetTextAt(_selection.StartOffset - backwardLength, backwardLength);
            Int32 removeLength = 0;
            for (int i = backwardText.Length - 1; i >= 0; --i)
            {
                Char ch = backwardText[i];
                if (ch == ' ')
                {
                    ++removeLength;
                }
                else
                {
                    break;
                }
            }
            if (removeLength > 0)
            {
                Document.Remove(_caret.DocumentOffset - removeLength, removeLength);
                _caret.MoveLeft(removeLength);
                SelectionFollowCaret(false, FlowDirection.LeftToRight);
            }
            MoveCaretInVisual();
        }

        public void SelectWord()
        {
            if (CanContentEdit && Content is IEditInfo)
            {
                var offsets = (Content as IEditInfo).MeasureWordOffsets(_caret.DocumentOffset);
                Int32 wordStart = offsets.Item1;
                Int32 wordEnd = offsets.Item2;
                if (wordStart != wordEnd)
                {
                    _caret.DocumentOffset = wordEnd;
                    _selection.StartOffset = wordStart;
                    _selection.EndOffset = wordEnd;
                    MoveCaretInVisual();
                }
            }
        }

        public void SelectLine()
        {
            DocumentLine line = Document.GetLineByOffset(_caret.DocumentOffset);
            Int32 lineStart = line.StartOffset;
            Int32 lineEnd = lineStart + line._exactLength;
            _caret.DocumentOffset = lineStart;
            _selection.StartOffset = lineStart;
            _selection.EndOffset = lineEnd;
            MoveCaretInVisual();
        }

        public void SelectAll()
        {
            _caret.DocumentOffset = Document.Length;
            _selection.StartOffset = 0;
            _selection.EndOffset = Document.Length;
            MoveCaretInVisual();
        }

        internal void Redraw()
        {
            base.InvalidateVisual();
        }

        internal void MeasureCaretLocation(Point positionToView)
        {
            if (Content != null && Content is IInputElement)
            {
                if (CanContentEdit && Content is IEditInfo)
                {
                    (Content as IEditInfo).MeasureCaretLocation(_caret, positionToView);
                }
            }
        }

        internal void MoveCaretWordLeft()
        {
            if (CanContentEdit && Content is IEditInfo)
            {
                _caret.DocumentOffset = (Content as IEditInfo).WordLeftCaretOffset(_caret.DocumentOffset);
            }
        }

        internal void MoveCaretWordRight()
        {
            if (CanContentEdit && Content is IEditInfo)
            {
                _caret.DocumentOffset = (Content as IEditInfo).WordRightCaretOffset(_caret.DocumentOffset);
            }
        }

        internal void MoveCaretLineUp()
        {
            if (CanContentEdit && Content is IEditInfo)
            {
                _caret.DocumentOffset = (Content as IEditInfo).LineUpCaretOffset(_caret.DocumentOffset);
            }
        }

        internal void MoveCaretLineDown()
        {
            if (CanContentEdit && Content is IEditInfo)
            {
                _caret.DocumentOffset = (Content as IEditInfo).LineDownCaretOffset(_caret.DocumentOffset);
            }
        }

        internal void MoveCaretPageUp()
        {
            if (CanContentEdit && Content is IEditInfo)
            {
                _caret.DocumentOffset = (Content as IEditInfo).PageUpCaretOffset(_caret.DocumentOffset);
            }
        }

        internal void MoveCaretPageDown()
        {
            if (CanContentEdit && Content is IEditInfo)
            {
                _caret.DocumentOffset = (Content as IEditInfo).PageDownCaretOffset(_caret.DocumentOffset);
            }
        }

        internal void MoveCaretWheelUp()
        {
            if (CanContentEdit && Content is IEditInfo)
            {
                _caret.DocumentOffset = (Content as IEditInfo).WheelUpCaretOffset(_caret.DocumentOffset);
            }
        }

        internal void MoveCaretWheelDown()
        {
            if (CanContentEdit && Content is IEditInfo)
            {
                _caret.DocumentOffset = (Content as IEditInfo).WheelDownCaretOffset(_caret.DocumentOffset);
            }
        }

        internal void MoveCaretWheelLeft()
        {
            if (CanContentEdit && Content is IEditInfo)
            {
                _caret.DocumentOffset = (Content as IEditInfo).WheelLeftCaretOffset(_caret.DocumentOffset);
            }
        }

        internal void MoveCaretWheelRight()
        {
            if (CanContentEdit && Content is IEditInfo)
            {
                _caret.DocumentOffset = (Content as IEditInfo).WheelRightCaretOffset(_caret.DocumentOffset);
            }
        }

        internal void MoveCaret(CaretMovementType movementType, Boolean doSelect)
        {
            TextLocation location;
            Int32 length;
            DocumentLine line;
            FlowDirection direction = _caret.DocumentOffset == _selection.StartOffset ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            switch (movementType)
            {
                case CaretMovementType.CharacterLeft:
                    location = Document.GetLocation(_caret.DocumentOffset);
                    length = location.Column == 1 ? CommonUtilities.LineBreak.Length : 1;
                    _caret.MoveLeft(length);
                    SelectionFollowCaret(doSelect, direction);
                    break;
                case CaretMovementType.CharacterRight:
                    location = Document.GetLocation(_caret.DocumentOffset);
                    line = Document.GetLineByNumber(location.Line);
                    length = location.Column == (line.Length + 1) ? CommonUtilities.LineBreak.Length : 1;
                    _caret.MoveRight(length);
                    SelectionFollowCaret(doSelect, direction);
                    break;
                case CaretMovementType.WordLeft:
                    MoveCaretWordLeft();
                    break;
                case CaretMovementType.WordRight:
                    MoveCaretWordRight();
                    break;
                case CaretMovementType.LineStart:
                    line = Document.GetLineByOffset(_caret.DocumentOffset);
                    _caret.DocumentOffset = line.StartOffset;
                    SelectionFollowCaret(doSelect, direction);
                    break;
                case CaretMovementType.LineEnd:
                    line = Document.GetLineByOffset(_caret.DocumentOffset);
                    _caret.DocumentOffset = line.EndOffset;
                    SelectionFollowCaret(doSelect, direction);
                    break;
                case CaretMovementType.LineUp:
                    MoveCaretLineUp();
                    SelectionFollowCaret(doSelect, direction);
                    break;
                case CaretMovementType.LineDown:
                    MoveCaretLineDown();
                    SelectionFollowCaret(doSelect, direction);
                    break;
                case CaretMovementType.PageUp:
                    MoveCaretPageUp();
                    SelectionFollowCaret(doSelect, direction);
                    break;
                case CaretMovementType.PageDown:
                    MoveCaretPageDown();
                    SelectionFollowCaret(doSelect, direction);
                    break;
                case CaretMovementType.WheelUp:
                    MoveCaretWheelUp();
                    SelectionFollowCaret(doSelect, direction);
                    break;
                case CaretMovementType.WheelDown:
                    MoveCaretWheelDown();
                    SelectionFollowCaret(doSelect, direction);
                    break;
                case CaretMovementType.WheelLeft:
                    MoveCaretWheelLeft();
                    SelectionFollowCaret(doSelect, direction);
                    break;
                case CaretMovementType.WheelRight:
                    MoveCaretWheelRight();
                    SelectionFollowCaret(doSelect, direction);
                    break;
                case CaretMovementType.DocumentStart:
                    _caret.DocumentOffset = 0;
                    SelectionFollowCaret(doSelect, direction);
                    break;
                case CaretMovementType.DocumentEnd:
                    _caret.DocumentOffset = Document.Length;
                    SelectionFollowCaret(doSelect, direction);
                    break;
                default:
                    break;
            }
            MoveCaretInVisual();
        }

        internal void SelectionFollowCaret(Boolean doSelect, FlowDirection direction)
        {
            if (!doSelect)
            {
                _selection.SetEmpty(_caret.DocumentOffset);
                return;
            }

            if (_selection.IsEmpty)
            {
                if (_caret.DocumentOffset < _selection.StartOffset)
                {
                    _selection.StartOffset = _caret.DocumentOffset;
                }
                else if (_caret.DocumentOffset > _selection.EndOffset)
                {
                    _selection.EndOffset = _caret.DocumentOffset;
                }
            }
            else
            {
                if (direction == FlowDirection.LeftToRight)
                {
                    if (_caret.DocumentOffset < _selection.StartOffset)
                    {
                        _selection.EndOffset = _selection.StartOffset;
                        _selection.StartOffset = _caret.DocumentOffset;
                    }
                    else
                    {
                        _selection.EndOffset = _caret.DocumentOffset;
                    }
                }
                else
                {
                    if (_caret.DocumentOffset > _selection.EndOffset)
                    {
                        _selection.StartOffset = _selection.EndOffset;
                        _selection.EndOffset = _caret.DocumentOffset;
                    }
                    else
                    {
                        _selection.StartOffset = _caret.DocumentOffset;
                    }
                }
            }
        }

        internal void MoveCaretInVisual()
        {
            Double left = _caret.ViewRect.Left;
            Double top = _caret.ViewRect.Top;
            Double right = _caret.ViewRect.Right;
            Double bottom = _caret.ViewRect.Bottom;
            Double offsetX = 0.0, offsetY = 0.0;
            Boolean xChanged = false, yChanged = false;
            if (left < 0)
            {
                offsetX = left;
                xChanged = true;
            }
            else if (right > ViewportWidth)
            {
                offsetX = right - ViewportWidth;
                xChanged = true;
            }
            if (top < 0)
            {
                offsetY = top;
                yChanged = true;
            }
            else if (bottom > ViewportHeight)
            {
                offsetY = bottom - ViewportHeight;
                yChanged = true;
            }
            if (xChanged)
            {
                base.ScrollToHorizontalOffset(HorizontalOffset + offsetX);
            }
            if (yChanged)
            {
                base.ScrollToVerticalOffset(VerticalOffset + offsetY);
            }
        }

        private void Measure()
        {
            MeasureCaret();
            MeasureSelection();
        }

        private void MeasureCaret()
        {
            if (CanContentEdit && Content is IEditInfo)
            {
                (Content as IEditInfo).MeasureCaretRendering(_caret);
            }
        }

        private void MeasureSelection()
        {
            if (CanContentEdit && Content is IEditInfo)
            {
                (Content as IEditInfo).MeasureSelectionRendering(_selection);
            }
        }

        private void CreateDefaultInputHandler()
        {
            var defaultHandler = new InputHandlerGroup(this);
            defaultHandler.Children.Add(EditingCommandHelper.CreateHandler(this));
            defaultHandler.Children.Add(CaretNavigationCommandHelper.CreateHandler(this));
            defaultHandler.Children.Add(new SelectionMouseHandler(this));
            ActiveInputHandler = defaultHandler;
        }

        private void OnSelectionOffsetChanged(object sender, EventArgs e)
        {
            MeasureSelection();
        }

        private void OnCaretPositionChanged(object sender, EventArgs e)
        {
            MeasureCaret();
        }

        #endregion

        #region Undo / Redo
        public void Undo()
        {
            Document._undoing = true;
            Document.Undo();
            Document._undoing = false;
            _undoStack.Undo();
        }

        public void Redo()
        {
            Document._undoing = true;
            Document.Redo();
            Document._undoing = false;
            _undoStack.Redo();
        }

        public Boolean CanUndo()
        {
            return _undoStack.CanUndo();
        }

        public Boolean CanRedo()
        {
            return _undoStack.CanRedo();
        }

        internal void BeginUpdating()
        {
            if (_updating)
            {
                return;
            }
            _updating = true;
            _update.CaretMoving = _caret.DocumentOffset;
            _update.SelectionStartMoving = _selection.StartOffset;
            _update.SelectionEndMoving = _selection.EndOffset;
        }

        internal void EndUpdating()
        {
            if (!_updating)
            {
                return;
            }
            _updating = false;
            _update.CaretMoving = _caret.DocumentOffset - _update.CaretMoving;
            _update.SelectionStartMoving = _selection.StartOffset - _update.SelectionStartMoving;
            _update.SelectionEndMoving = _selection.EndOffset - _update.SelectionEndMoving;
            _undoStack.AddOperation(new EditingOperation(this, _update.Clone()));
        }

        private Boolean _updating = false;
        private EditingOffsetUpdate _update = new EditingOffsetUpdate();
        #endregion

        #region IWeakEventListener
        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType == typeof(TextDocumentWeakEventManager.Changing))
            {
                OnDocumentChanging(sender as TextDocument, e);
                return true;
            }
            else if (managerType == typeof(TextDocumentWeakEventManager.Changed))
            {
                OnDocumentChanged(sender as TextDocument, e as DocumentUpdateEventArgs);
                return true;
            }
            else if (managerType == typeof(TextDocumentWeakEventManager.UpdateStarted))
            {
                OnDocumentUpdateStarted(sender as TextDocument, e);
                return true;
            }
            else if (managerType == typeof(TextDocumentWeakEventManager.UpdateFinished))
            {
                OnDocumentUpdateFinished(sender as TextDocument, e as DocumentUpdateEventArgs);
                return true;
            }
            return false;
        }
        #endregion

        #region Weak events
        private void OnDocumentChanging(TextDocument document, EventArgs e)
        {

        }

        private void OnDocumentChanged(TextDocument document, DocumentUpdateEventArgs e)
        {

        }

        private void OnDocumentUpdateStarted(TextDocument document, EventArgs e)
        {

        }

        private void OnDocumentUpdateFinished(TextDocument document, DocumentUpdateEventArgs e)
        {

        }
        #endregion

        #region PropertyChange EventHandlers
        private static void OnDocumentChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            EditView editor = dp as EditView;
            editor.OnDocumentChanged(e.OldValue as TextDocument, e.NewValue as TextDocument);
        }
        private void OnDocumentChanged(TextDocument oldDoc, TextDocument newDoc)
        {
            if (oldDoc != null)
            {
                TextDocumentWeakEventManager.Changing.RemoveListener(oldDoc, this);
                TextDocumentWeakEventManager.Changed.RemoveListener(oldDoc, this);
                TextDocumentWeakEventManager.UpdateStarted.RemoveListener(oldDoc, this);
                TextDocumentWeakEventManager.UpdateFinished.RemoveListener(oldDoc, this);
            }
            if (newDoc != null)
            {
                TextDocumentWeakEventManager.Changing.AddListener(newDoc, this);
                TextDocumentWeakEventManager.Changed.AddListener(newDoc, this);
                TextDocumentWeakEventManager.UpdateStarted.AddListener(newDoc, this);
                TextDocumentWeakEventManager.UpdateFinished.AddListener(newDoc, this);
            }
            if (CanContentEdit && Content is IEditInfo)
            {
                (Content as IEditInfo).ChangeDocument(newDoc);
            }
            _caret.DocumentOffset = 0;
            _selection.Reset();
            _undoStack.Reset();
            ScrollToHome();
            Measure();
            Redraw();
        }
        #endregion

        private RenderContext _renderContext;
        private Caret _caret;
        private Selection _selection;
        private IInputHandler _activeInputHandler;
        private UndoStack _undoStack;
        internal List<Action<MouseButtonEventArgs>> _mouseLeftDownHandlers = new List<Action<MouseButtonEventArgs>>();
        internal List<Action<MouseButtonEventArgs>> _mouseLeftUpHandlers = new List<Action<MouseButtonEventArgs>>();
        internal List<Action<MouseEventArgs>> _mouseMoveHandlers = new List<Action<MouseEventArgs>>();
    }
}
