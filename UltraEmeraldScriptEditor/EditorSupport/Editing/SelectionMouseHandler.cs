using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EditorSupport.Editing
{
    public enum SelectionMode : Byte
    {
        /// <summary>
        /// 无选择
        /// </summary>
        None = 0,
        /// <summary>
        /// 鼠标按下，可能开始拖动选择
        /// </summary>
        PossiblyDragStart,
        /// <summary>
        /// 拖动选中项
        /// </summary>
        Drag,
        /// <summary>
        /// 按住Shift键选择
        /// </summary>
        Normal,
        /// <summary>
        /// 双击或按住Ctrl键选中一个单词
        /// </summary>
        WholeWord,
        /// <summary>
        /// 三击选中一行
        /// </summary>
        WholeLine,
        /// <summary>
        /// 按住Alt键选择一个矩形范围（这个暂时不做，以后加上）
        /// </summary>
        Rectanglular,
    }

    public sealed class SelectionMouseHandler : IInputHandler
    {
        public SelectionMouseHandler(EditView owner)
        {
            _owner = owner ?? throw new ArgumentNullException("owner");
        }

        #region IInputHandler
        public EditView Owner => _owner;

        public void Attach()
        {
            _mode = SelectionMode.None;
            _owner.MouseLeftButtonDown += OnMouseLeftButtonDown;
            _owner.MouseMove += OnMouseMove;
            _owner.MouseLeftButtonUp += OnMouseLeftButtonUp;
        }

        public void Detach()
        {
            _mode = SelectionMode.None;
            _owner.MouseLeftButtonDown -= OnMouseLeftButtonDown;
            _owner.MouseMove -= OnMouseMove;
            _owner.MouseLeftButtonUp -= OnMouseLeftButtonUp;
        }
        #endregion

        private void OnMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        }

        private void OnMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        }

        private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
        }

        private EditView _owner;
        private SelectionMode _mode;
    }
}
