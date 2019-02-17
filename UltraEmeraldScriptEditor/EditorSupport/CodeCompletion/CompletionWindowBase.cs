using EditorSupport.Editing;
using EditorSupport.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace EditorSupport.CodeCompletion
{
    public class CompletionWindowBase : Window
    {
        static CompletionWindowBase()
        {
            WindowStyleProperty.OverrideMetadata(typeof(CompletionWindowBase), new FrameworkPropertyMetadata(WindowStyle.None));
            ShowActivatedProperty.OverrideMetadata(typeof(CompletionWindowBase), new FrameworkPropertyMetadata(Boxes.False));
            ShowInTaskbarProperty.OverrideMetadata(typeof(CompletionWindowBase), new FrameworkPropertyMetadata(Boxes.False));
        }

        protected CompletionWindowBase(EditView editview)
        {
            _editview = editview ?? throw new ArgumentNullException("editview");
            _editview.SizeChanged += OnEditViewSizeChanged;
            _editview.ScrollOffsetChanged += OnEditViewScrollOffsetChanged;
            _parentWindow = Window.GetWindow(_editview);
            _parentWindow.LocationChanged += OnParentWindowLocationChanged;
            Owner = _parentWindow;
            Loaded += OnLoaded;
            Closed += OnClosed;
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

        protected void UpdateLocation()
        {
            Rect caretRect = _editview.Caret.RenderRect;
            Point pos = new Point(caretRect.Left, caretRect.Bottom + 5.0);
            // 判断下方能否足够显示
            if (pos.Y + ActualHeight > _editview.ActualHeight)
            {
                // 显示到上方
                pos.Y = caretRect.Top - ActualHeight;
            }
            pos = _editview.PointToScreen(pos);
            this.Left = pos.X;
            this.Top = pos.Y;
        }

        protected EditView _editview;
        protected Window _parentWindow;
    }
}
