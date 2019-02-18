using EditorSupport.Editing;
using EditorSupport.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace EditorSupport.CodeCompletion
{
    public class CompletionWindowBase : Window
    {
        public EditView EditView
        {
            get => _editview;
            set
            {
                if (_editview != value)
                {
                    if (_editview != null)
                    {
                        _editview.SizeChanged -= OnEditViewSizeChanged;
                        _editview.ScrollOffsetChanged -= OnEditViewScrollOffsetChanged;

                        _parentWindow.LocationChanged -= OnParentWindowLocationChanged;
                    }
                    _editview = value;
                    if (_editview != null)
                    {
                        _editview.SizeChanged += OnEditViewSizeChanged;
                        _editview.ScrollOffsetChanged += OnEditViewScrollOffsetChanged;

                        _parentWindow = Window.GetWindow(_editview);
                        _parentWindow.LocationChanged += OnParentWindowLocationChanged;
                    }
                    else
                    {
                        throw new ArgumentNullException("EditView");
                    }
                }
            }
        }

        #region Constructor
        static CompletionWindowBase()
        {
            WindowStyleProperty.OverrideMetadata(typeof(CompletionWindowBase), new FrameworkPropertyMetadata(WindowStyle.None));
            ResizeModeProperty.OverrideMetadata(typeof(CompletionWindowBase), new FrameworkPropertyMetadata(ResizeMode.NoResize));
            ShowActivatedProperty.OverrideMetadata(typeof(CompletionWindowBase), new FrameworkPropertyMetadata(Boxes.False));
            ShowInTaskbarProperty.OverrideMetadata(typeof(CompletionWindowBase), new FrameworkPropertyMetadata(Boxes.False));
        }

        protected CompletionWindowBase()
        {
            AddHandler(MouseUpEvent, new MouseButtonEventHandler(OnMouseUp), true);
            Loaded += OnLoaded;
            Closed += OnClosed;
        }
        #endregion

        #region Virtual
        public new virtual void Show()
        {
            Owner = _parentWindow;
            base.Show();
        }

        protected virtual void OnLoaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("loaded");
            UpdateLocation();
        }

        protected virtual void OnClosed(object sender, EventArgs e)
        {
            Debug.WriteLine("closed");
        }
        #endregion

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
        }

        #region Event handlers
        protected void OnParentWindowLocationChanged(object sender, EventArgs e)
        {
            UpdateLocation();
        }

        protected void OnEditViewSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateLocation();
        }

        protected void OnEditViewScrollOffsetChanged(object sender, EventArgs e)
        {
            UpdateLocation();
        }

        private void OnMouseUp(Object sender, MouseButtonEventArgs e)
        {
            // 永远无法被激活
            if (_parentWindow != null)
            {
                _parentWindow.Activate();
            }
        }
        #endregion

        protected void UpdateLocation()
        {
            if (_editview == null)
            {
                Left = Top = 0.0;
                return;
            }
            Rect caretRect = _editview.Caret.RenderRect;
            Point pos = new Point(caretRect.Left, caretRect.Bottom + 5.0);
            // 判断下方能否足够显示
            if (pos.Y + ActualHeight > _editview.ActualHeight)
            {
                // 显示到上方
                pos.Y = caretRect.Top - ActualHeight;
            }
            pos = _editview.PointToScreen(pos);
            Left = pos.X;
            Top = pos.Y;
        }

        protected EditView _editview;
        protected Window _parentWindow;
        protected WindowInteropHelper _interopHelper;
    }
}
