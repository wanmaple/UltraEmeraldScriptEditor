using EditorSupport.Document;
using EditorSupport.Rendering;
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
            Cursor = Cursors.IBeam;

            _caret = new Caret(this);
            _selection = new AnchorSelection(this, Document.CreateAnchor(0), Document.CreateAnchor(0));

            CreateDefaultInputHandler();
        }
        #endregion

        #region Overrides
        protected override Size MeasureOverride(Size constraint)
        {
            return base.MeasureOverride(constraint);
        }

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
            if (Content != null && Content is IInputElement)
            {
                Point pos = e.GetPosition(Content as IInputElement);
                if (CanContentEdit && Content is IEditInfo)
                {
                    (Content as IEditInfo).MeasureCaretLocation(_caret, pos);
                    _selection.SetEmpty(_caret.DocumentOffset);
                }
                Focus();
                Measure();
                MoveCaretInVisual();
                Redraw();
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
        }

        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            base.OnPreviewKeyUp(e);
        }

        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            base.OnTextInput(e);
            InsertText(e.Text);
            Redraw();
        }
        #endregion

        public void InsertText(String content)
        {
            if (_selection.IsEmpty)
            {
                Document.Insert(_selection.StartOffset, content);
            }
            else
            {
                Document.Replace(_selection.StartOffset, _selection.Length, content);
            }
            _caret.MoveRight(content.Length);
            _selection.SetEmpty(_caret.DocumentOffset);
            Measure();
        }

        public void RemoveSelection()
        {
            if (_selection.IsEmpty)
            {
                return;
            }
            Document.Remove(_selection.StartOffset, _selection.Length);
            _caret.DocumentOffset = _selection.StartOffset;
            _selection.SetEmpty(_caret.DocumentOffset);
            Measure();
        }

        public void TabForward()
        {
            InsertText(CommonUtilities.Tab);
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
                _selection.Move(-removeLength);
            }
        }

        public void SelectAll()
        {
            _selection.StartOffset = 0;
            _selection.EndOffset = Document.Length;
            _caret.DocumentOffset = Document.Length;
            Measure();
            MoveCaretInVisual();
        }

        public void Redraw()
        {
            base.InvalidateVisual();
        }

        internal void MoveCaret(CaretMovementType movementType, Boolean doSelect)
        {
            TextLocation location;
            Int32 length;
            DocumentLine line, prevLine, nextLine;
            switch (movementType)
            {
                case CaretMovementType.CharacterLeft:
                    if (doSelect)
                    {
                        if (_caret.DocumentOffset == _selection.StartOffset)
                        {
                            location = Document.GetLocation(_caret.DocumentOffset);
                            length = location.Column == 1 ? CommonUtilities.LineBreak.Length : 1;
                            _caret.MoveLeft(length);
                            _selection.MoveStart(-length);
                        }
                        else if (_caret.DocumentOffset == _selection.EndOffset)
                        {
                            location = Document.GetLocation(_caret.DocumentOffset);
                            length = location.Column == 1 ? CommonUtilities.LineBreak.Length : 1;
                            _caret.MoveLeft(length);
                            _selection.MoveEnd(-length);
                        }
                    }
                    else
                    {
                        location = Document.GetLocation(_caret.DocumentOffset);
                        length = location.Column == 1 ? CommonUtilities.LineBreak.Length : 1;
                        _caret.MoveLeft(length);
                        _selection.SetEmpty(_caret.DocumentOffset);
                    }
                    break;
                case CaretMovementType.CharacterRight:
                    if (doSelect)
                    {
                        if (_caret.DocumentOffset == _selection.EndOffset)
                        {
                            location = Document.GetLocation(_caret.DocumentOffset);
                            line = Document.GetLineByNumber(location.Line);
                            length = location.Column == (line.Length + 1) ? CommonUtilities.LineBreak.Length : 1;
                            _caret.MoveRight(length);
                            _selection.MoveEnd(length);
                        }
                        else if (_caret.DocumentOffset == _selection.StartOffset)
                        {
                            location = Document.GetLocation(_caret.DocumentOffset);
                            line = Document.GetLineByNumber(location.Line);
                            length = location.Column == (line.Length + 1) ? CommonUtilities.LineBreak.Length : 1;
                            _caret.MoveRight(length);
                            _selection.MoveStart(length);
                        }
                    }
                    else
                    {
                        location = Document.GetLocation(_caret.DocumentOffset);
                        line = Document.GetLineByNumber(location.Line);
                        length = location.Column == (line.Length + 1) ? CommonUtilities.LineBreak.Length : 1;
                        _caret.MoveRight(length);
                        _selection.SetEmpty(_caret.DocumentOffset);
                    }
                    break;
                case CaretMovementType.WordLeft:
                    break;
                case CaretMovementType.WordRight:
                    break;
                case CaretMovementType.LineStart:
                    location = Document.GetLocation(_caret.DocumentOffset);
                    line = Document.GetLineByOffset(_caret.DocumentOffset);
                    if (doSelect)
                    {
                        if (_selection.IsEmpty || _caret.DocumentOffset < _selection.EndOffset)
                        {
                            _caret.MoveLeft(_caret.DocumentOffset - line.StartOffset);
                            _selection.StartOffset = _caret.DocumentOffset;
                        }
                        else if (_caret.DocumentOffset > _selection.StartOffset)
                        {
                            prevLine = Document.GetLineByOffset(_selection.StartOffset);
                            _caret.MoveLeft(_caret.DocumentOffset - line.StartOffset);
                            if (prevLine.LineNumber == location.Line)
                            {
                                _selection.EndOffset = _selection.StartOffset;
                                _selection.StartOffset = _caret.DocumentOffset;
                            }
                            else
                            {
                                _selection.EndOffset = _caret.DocumentOffset;
                            }
                        }
                    }
                    else
                    {
                        _caret.MoveLeft(_caret.DocumentOffset - line.StartOffset);
                        _selection.SetEmpty(_caret.DocumentOffset);
                    }
                    break;
                case CaretMovementType.LineEnd:
                    location = Document.GetLocation(_caret.DocumentOffset);
                    line = Document.GetLineByOffset(_caret.DocumentOffset);
                    if (doSelect)
                    {
                        if (_selection.IsEmpty || _caret.DocumentOffset > _selection.StartOffset)
                        {
                            _caret.MoveRight(line.EndOffset - _caret.DocumentOffset);
                            _selection.EndOffset = _caret.DocumentOffset;
                        }
                        else if (_caret.DocumentOffset < _selection.EndOffset)
                        {
                            nextLine = Document.GetLineByOffset(_selection.EndOffset);
                            _caret.MoveRight(line.EndOffset - _caret.DocumentOffset);
                            if (nextLine.LineNumber == location.Line)
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
                    else
                    {
                        _caret.MoveRight(line.EndOffset - _caret.DocumentOffset);
                        _selection.SetEmpty(_caret.DocumentOffset);
                    }
                    break;
                case CaretMovementType.LineUp:
                    if (CanContentEdit && Content is IEditInfo)
                    {
                        if (doSelect)
                        {
                            if (_caret.DocumentOffset == _selection.StartOffset)
                            {
                                _caret.DocumentOffset = (Content as IEditInfo).LineUpCaretOffset(_caret.DocumentOffset);
                                _selection.StartOffset = _caret.DocumentOffset;
                            }
                            else if (_caret.DocumentOffset == _selection.EndOffset)
                            {
                                _caret.DocumentOffset = (Content as IEditInfo).LineUpCaretOffset(_caret.DocumentOffset);
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
                        }
                        else
                        {
                            _caret.DocumentOffset = (Content as IEditInfo).LineUpCaretOffset(_caret.DocumentOffset);
                            _selection.SetEmpty(_caret.DocumentOffset);
                        }
                    }
                    break;
                case CaretMovementType.LineDown:
                    if (CanContentEdit && Content is IEditInfo)
                    {
                        if (doSelect)
                        {
                            if (_caret.DocumentOffset == _selection.EndOffset)
                            {
                                _caret.DocumentOffset = (Content as IEditInfo).LineDownCaretOffset(_caret.DocumentOffset);
                                _selection.EndOffset = _caret.DocumentOffset;
                            }
                            else if (_caret.DocumentOffset == _selection.StartOffset)
                            {
                                _caret.DocumentOffset = (Content as IEditInfo).LineDownCaretOffset(_caret.DocumentOffset);
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
                        else
                        {
                            _caret.DocumentOffset = (Content as IEditInfo).LineDownCaretOffset(_caret.DocumentOffset);
                            _selection.SetEmpty(_caret.DocumentOffset);
                        }
                    }
                    break;
                case CaretMovementType.PageUp:
                    if (CanContentEdit && Content is IEditInfo)
                    {
                        if (doSelect)
                        {
                            if (_caret.DocumentOffset == _selection.StartOffset)
                            {
                                _caret.DocumentOffset = (Content as IEditInfo).PageUpCaretOffset(_caret.DocumentOffset);
                                _selection.StartOffset = _caret.DocumentOffset;
                            }
                            else if (_caret.DocumentOffset == _selection.EndOffset)
                            {
                                _caret.DocumentOffset = (Content as IEditInfo).PageUpCaretOffset(_caret.DocumentOffset);
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
                        }
                        else
                        {
                            _caret.DocumentOffset = (Content as IEditInfo).PageUpCaretOffset(_caret.DocumentOffset);
                            _selection.SetEmpty(_caret.DocumentOffset);
                        }
                    }
                    break;
                case CaretMovementType.PageDown:
                    if (CanContentEdit && Content is IEditInfo)
                    {
                        if (doSelect)
                        {
                            if (_caret.DocumentOffset == _selection.EndOffset)
                            {
                                _caret.DocumentOffset = (Content as IEditInfo).PageDownCaretOffset(_caret.DocumentOffset);
                                _selection.EndOffset = _caret.DocumentOffset;
                            }
                            else if (_caret.DocumentOffset == _selection.StartOffset)
                            {
                                _caret.DocumentOffset = (Content as IEditInfo).PageDownCaretOffset(_caret.DocumentOffset);
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
                        else
                        {
                            _caret.DocumentOffset = (Content as IEditInfo).PageDownCaretOffset(_caret.DocumentOffset);
                            _selection.SetEmpty(_caret.DocumentOffset);
                        }
                    }
                    break;
                case CaretMovementType.DocumentStart:
                    _caret.DocumentOffset = 0;
                    if (doSelect)
                    {
                        _selection.StartOffset = _caret.DocumentOffset;
                    }
                    else
                    {
                        _selection.SetEmpty(_caret.DocumentOffset);
                    }
                    break;
                case CaretMovementType.DocumentEnd:
                    _caret.DocumentOffset = Document.Length;
                    if (doSelect)
                    {
                        _selection.EndOffset = _caret.DocumentOffset;
                    }
                    else
                    {
                        _selection.SetEmpty(_caret.DocumentOffset);
                    }
                    break;
                default:
                    break;
            }
            Measure();
            MoveCaretInVisual();
            _caret.RestartAnimation();
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
            if (CanContentEdit && Content is IEditInfo)
            {
                (Content as IEditInfo).MeasureSelectionRendering(_selection);
            }
            if (CanContentEdit && Content is IEditInfo)
            {
                (Content as IEditInfo).MeasureCaretRendering(_caret);
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
            ScrollToHome();
            Measure();
            Redraw();
        }
        #endregion

        private RenderContext _renderContext;
        private Caret _caret;
        private Selection _selection;
        private IInputHandler _activeInputHandler;
    }
}
