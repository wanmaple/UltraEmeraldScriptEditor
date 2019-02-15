using EditorSupport.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace EditorSupport.Editing
{
    internal enum SelectionMode : Byte
    {
        /// <summary>
        /// 无选择
        /// </summary>
        None = 0,
        /// <summary>
        /// 鼠标拖动选择
        /// </summary>
        PossiblyDragStart,
        /// <summary>
        /// 按住Shift键选择
        /// </summary>
        Normal,
        /// <summary>
        /// 双击选中一个单词
        /// </summary>
        WholeWord,
        /// <summary>
        /// 三击选中一行
        /// </summary>
        WholeLine,
    }

    internal enum ScrollDirection : Byte
    {
        None = 0,
        Left = 1,
        Top = 1 << 1,
        Right = 1 << 2,
        Down = 1 << 3,
    }

    public sealed class SelectionMouseHandler : IInputHandler
    {
        public SelectionMouseHandler(EditView owner)
        {
            _owner = owner ?? throw new ArgumentNullException("owner");
            _scrollingTimer = new DispatcherTimer(DispatcherPriority.Input);
            _scrollingTimer.Interval = TimeSpan.FromSeconds(0.1);
            _scrollingTimer.Tick += OnScrolling;
            _scrolling = false;
        }

        #region IInputHandler
        public EditView Owner => _owner;

        public void Attach()
        {
            _mode = SelectionMode.None;
            // 必须通过这种方式添加事件，不然ScrollViewer自身的事件会受到影响
            _owner._mouseLeftDownHandlers.Add(OnMouseLeftButtonDown);
            _owner._mouseLeftUpHandlers.Add(OnMouseLeftButtonUp);
            _owner._mouseMoveHandlers.Add(OnMouseMove);
            _owner.MouseEnter += OnMouseEnter;
            _owner.MouseLeave += OnMouseLeave;
        }

        public void Detach()
        {
            _mode = SelectionMode.None;
            _owner._mouseLeftDownHandlers.Remove(OnMouseLeftButtonDown);
            _owner._mouseLeftUpHandlers.Remove(OnMouseLeftButtonUp);
            _owner._mouseMoveHandlers.Remove(OnMouseMove);
            _owner.MouseEnter -= OnMouseEnter;
            _owner.MouseLeave -= OnMouseLeave;
        }
        #endregion

        private void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            ModifierKeys modifiers = Keyboard.Modifiers;
            Boolean shift = (modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;

            _mode = SelectionMode.Normal;
            FlowDirection direction = _owner.Caret.DocumentOffset == _owner.Selection.StartOffset ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
            _owner.MeasureCaretLocation(e.GetPosition(_owner.Content as IInputElement));
            if (shift)
            {
                _owner.SelectionFollowCaret(true, direction);
            }
            else
            {
                if (e.ClickCount == 2)
                {
                    _mode = SelectionMode.WholeWord;
                    _owner.SelectWord();
                }
                else if (e.ClickCount == 3)
                {
                    _mode = SelectionMode.WholeLine;
                    _owner.SelectLine();
                }
                else
                {
                    _owner.SelectionFollowCaret(false, direction);
                }
            }
            _owner.Redraw();

            e.Handled = true;
        }

        private void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            _mode = SelectionMode.None;

            e.Handled = true;
        }

        private void OnMouseMove(MouseEventArgs e)
        {
            if (_mode == SelectionMode.PossiblyDragStart)
            {
                FlowDirection direction = _owner.Caret.DocumentOffset == _owner.Selection.StartOffset ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
                _owner.MeasureCaretLocation(e.GetPosition(_owner.Content as IInputElement));
                _owner.SelectionFollowCaret(true, direction);
                _owner.Redraw();
            }
            else if (_mode == SelectionMode.Normal)
            {
                _mode = SelectionMode.PossiblyDragStart;
            }

            e.Handled = true;
        }

        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            StopScrolling();
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (_mode != SelectionMode.None)
            {
                StartScrolling();
            }

            e.Handled = true;
        }

        private void OnScrolling(object sender, EventArgs e)
        {
            Mouse.Capture(_owner);
            Point mousePos = Mouse.GetPosition(_owner.Content as IInputElement);
            Boolean released = Mouse.LeftButton == MouseButtonState.Released;
            Mouse.Capture(null);

            if (released)
            {
                StopScrolling();
                _mode = SelectionMode.None;
                return;
            }

            // 自动滚动
            Double threshold = 0.0;
            ScrollDirection direction = ScrollDirection.None;
            if (mousePos.X < -threshold)
            {
                direction |= ScrollDirection.Left;
            }
            else if (mousePos.X > _owner.ViewportWidth + threshold)
            {
                direction |= ScrollDirection.Right;
            }
            if (mousePos.Y < -threshold)
            {
                direction |= ScrollDirection.Top;
            }
            else if (mousePos.Y > _owner.ViewportHeight + threshold)
            {
                direction |= ScrollDirection.Down;
            }

            if ((direction & ScrollDirection.Left) == ScrollDirection.Left)
            {
                _owner.MoveCaret(CaretMovementType.WheelLeft, true);
                _owner.Redraw();
            }
            else if ((direction & ScrollDirection.Right) == ScrollDirection.Right)
            {
                _owner.MoveCaret(CaretMovementType.WheelRight, true);
                _owner.Redraw();
            }
            if ((direction & ScrollDirection.Top) == ScrollDirection.Top)
            {                
                _owner.MoveCaret(CaretMovementType.WheelUp, true);
                _owner.Redraw();
            }
            else if ((direction & ScrollDirection.Down) == ScrollDirection.Down)
            {
                _owner.MoveCaret(CaretMovementType.WheelDown, true);
                _owner.Redraw();
            }
        }

        private void StartScrolling()
        {
            if (!_scrolling)
            {
                _scrolling = true;
                _scrollingTimer.Start();
            }
        }

        private void StopScrolling()
        {
            if (_scrolling)
            {
                _scrolling = false;
                _scrollingTimer.Stop();
            }
        }

        private EditView _owner;
        private SelectionMode _mode;
        private DispatcherTimer _scrollingTimer;
        private Boolean _scrolling;
    }
}
