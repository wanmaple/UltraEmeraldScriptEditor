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

        public abstract void Render(DrawingContext drawingContext, RenderContext renderContext);
        #endregion

        #region Properties

        public Brush Brush { get => _brush; set => _brush = value; }

        public Boolean IsEmpty { get => Length <= 0; }

        public List<Rect> RenderRects { get => _renderRects; set => _renderRects = value; }

        public EditView Owner { get => _owner; }
        #endregion

        #region Constructor
        protected Selection(EditView owner)
        {
            _owner = owner ?? throw new ArgumentNullException("owner");
            _brush = new SolidColorBrush(CommonUtilities.ColorFromHexString("#7F7D7D7D"));
            _renderRects = new List<Rect>();
        }
        #endregion

        public void MoveLeft(Int32 length = 1)
        {
            if (length <= 0)
            {
                throw new ArgumentException("'length' must be positive.");
            }
            StartOffset = Math.Max(StartOffset - length, 0);
        }

        public void MoveRight(Int32 length = 1)
        {
            if (length <= 0)
            {
                throw new ArgumentException("'length' must be positive.");
            }
            EndOffset = Math.Min(EndOffset + length, Owner.Document.Length);
        }

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

        protected Brush _brush;
        protected List<Rect> _renderRects;
        protected EditView _owner;
    }
}
