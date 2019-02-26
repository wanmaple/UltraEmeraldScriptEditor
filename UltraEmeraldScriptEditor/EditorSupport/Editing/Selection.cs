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
        public event EventHandler OffsetChanged;

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
            _noTrigging = false;
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
            Move(offset, offset);
        }

        public void Move(Int32 startOffset, Int32 endOffset)
        {
            _noTrigging = true;
            Int32 oldStart = StartOffset, oldEnd = EndOffset;
            if (endOffset > 0)
            {
                MoveEnd(endOffset);
                MoveStart(startOffset);
            }
            else if (startOffset < 0)
            {
                MoveStart(startOffset);
                MoveEnd(endOffset);
            }
            else
            {
                if (startOffset != 0)
                {
                    MoveStart(startOffset);
                }
                if (endOffset != 0)
                {
                    MoveEnd(endOffset);
                }
            } 
            _noTrigging = false;
            TriggerOffsetChanged();
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
            _noTrigging = true;
            EndOffset = StartOffset = offset;
            _noTrigging = false;
            TriggerOffsetChanged();
        }

        public void Set(Int32 startOffset, Int32 endOffset)
        {
            if (startOffset < 0 || startOffset > _owner.Document.Length)
            {
                throw new ArgumentException(String.Format("{0} <= startOffset <= {1}", startOffset, _owner.Document.Length));
            }
            if (endOffset < 0 || endOffset > _owner.Document.Length)
            {
                throw new ArgumentException(String.Format("{0} <= endOffset <= {1}", endOffset, _owner.Document.Length));
            }
            _noTrigging = true;
            StartOffset = startOffset;
            EndOffset = endOffset;
            _noTrigging = false;
            TriggerOffsetChanged();
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

        protected void TriggerOffsetChanged()
        {
            if (!_noTrigging && OffsetChanged != null)
            {
                OffsetChanged(this, EventArgs.Empty);
            }
        }

        protected Brush _brush;
        protected List<Rect> _renderRects;
        protected List<Rect> _viewRects;
        protected EditView _owner;
        protected Boolean _noTrigging;
    }
}
