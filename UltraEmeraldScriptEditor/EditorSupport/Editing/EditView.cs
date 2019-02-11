using EditorSupport.Document;
using EditorSupport.Rendering;
using System;
using System.Collections.Generic;
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
    DependencyProperty.Register("Document", typeof(TextDocument), typeof(EditView), new PropertyMetadata(OnDocumentChanged));
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
            Cursor = Cursors.IBeam;

            ActiveInputHandler = EditingCommandHelper.CreateHandler(this);
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
            if (CanContentEdit && Content is IEditInfo)
            {
                (Content as IEditInfo).MeasureCaretRendering(_caret);
            }
            _caret.Render(drawingContext, _renderContext);
        }

        protected override void OnScrollChanged(ScrollChangedEventArgs e)
        {
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
                }
                Focus();
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
            Document.Insert(_caret.DocumentOffset, e.Text);
            _caret.MoveRight(e.Text.Length);
            Redraw();
        }
        #endregion

        public void InsertText(String content)
        {

        }

        public void Redraw()
        {
            base.InvalidateVisual();
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
            _caret.DocumentOffset = 0;
            if (CanContentEdit && Content is IEditInfo)
            {
                (Content as IEditInfo).ChangeDocument(newDoc);
            }
            Redraw();
        }
        #endregion

        private RenderContext _renderContext;
        private Caret _caret;
        private IInputHandler _activeInputHandler;
    }
}
