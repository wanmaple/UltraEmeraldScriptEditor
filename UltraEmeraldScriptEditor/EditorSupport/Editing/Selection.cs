using EditorSupport.Document;
using EditorSupport.Rendering;
using EditorSupport.Rendering.Renderers;
using EditorSupport.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace EditorSupport.Editing
{
    public abstract class Selection : ISegment, IRenderable
    {
        #region Abstraction
        public abstract int StartOffset { get; set; }

        public abstract int Length { get; set; }

        public abstract int EndOffset { get; set; }
        #endregion

        #region Properties

        public Brush Brush { get => _brush; set => _brush = value; }

        public Boolean IsEmpty { get => Length <= 0; }

        public List<Rect> RenderRects { get => _renderRects; }

        public List<Rect> ViewRects { get => _viewRects; }

        public Boolean Visible { get => _renderRects.Count > 0; }

        public EditView Owner { get => _owner; }
        #endregion

        #region Constructor
        protected Selection(EditView owner)
        {
            _owner = owner ?? throw new ArgumentNullException("owner");
            _brush = new SolidColorBrush(CommonUtilities.ColorFromHexString("#7F7D7D7D"));
            _renderRects = new List<Rect>();
            _viewRects = new List<Rect>();
        }
        #endregion

        /// <summary>
        /// StartOffset偏移
        /// </summary>
        /// <param name="offset"></param>
        public void MoveStart(Int32 offset)
        {
            StartOffset = CommonUtilities.Clamp(StartOffset + offset, 0, Owner.Document.Length);
        }

        /// <summary>
        /// EndOffset偏移
        /// </summary>
        /// <param name="offset"></param>
        public void MoveEnd(Int32 offset)
        {
            EndOffset = CommonUtilities.Clamp(EndOffset + offset, 0, Owner.Document.Length);
        }

        /// <summary>
        /// StartOffset和EndOffset一起偏移
        /// </summary>
        /// <param name="offset"></param>
        public void Move(Int32 offset)
        {
            Int32 oldLength = Length;
            if (offset > 0)
            {
                MoveEnd(offset);
                StartOffset = EndOffset - oldLength;
            }
            else if (offset < 0)
            {
                MoveStart(offset);
                EndOffset = StartOffset + oldLength;
            }
        }

        /// <summary>
        /// 清空Selection并且设置到指定偏移
        /// </summary>
        /// <param name="offset"></param>
        public void SetEmpty(Int32 offset)
        {
            if (offset < 0 || offset > _owner.Document.Length)
            {
                throw new ArgumentException(String.Format("{0} <= offset <= {1}", offset, _owner.Document.Length));
            }
            EndOffset = StartOffset = offset;
        }

        public void Reset()
        {
            SetEmpty(0);
        }

        public virtual void Render(DrawingContext drawingContext, RenderContext renderContext)
        {
            if (!Visible)
            {
                return;
            }
            foreach (Rect rect in _renderRects)
            {
                drawingContext.DrawRectangle(_brush, null, rect);
            }
        }

        protected Brush _brush;
        protected List<Rect> _renderRects;
        protected List<Rect> _viewRects;
        protected EditView _owner;
    }
}
