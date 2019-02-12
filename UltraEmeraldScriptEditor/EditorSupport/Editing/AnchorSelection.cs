using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using EditorSupport.Document;
using EditorSupport.Rendering;

namespace EditorSupport.Editing
{
    public sealed class AnchorSelection : Selection
    {
        public AnchorSelection(EditView owner, TextAnchor startAnchor, TextAnchor endAnchor)
            : base(owner)
        {
            _startAnchor = startAnchor ?? throw new ArgumentNullException("startAnchor");
            _startAnchor.MovementType = AnchorMovementType.Default;
            _endAnchor = endAnchor ?? throw new ArgumentNullException("endAnchor");
            _endAnchor.MovementType = AnchorMovementType.Default;
        }

        #region Overrides
        public override int StartOffset
        {
            get => _startAnchor.Offset;
            set
            {
                Int32 oldOffset = StartOffset;
                if (oldOffset != value)
                {
                    Int32 diff = value - oldOffset;
                    if (diff > 0)
                    {
                        _owner.Document.MoveAnchorRight(_startAnchor, diff);
                    }
                    else
                    {
                        _owner.Document.MoveAnchorLeft(_startAnchor, -diff);
                    }
                }
            }
        }
        public override int Length
        {
            get => EndOffset - StartOffset;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("'Length' must be positive.");
                }
                EndOffset = StartOffset + value;
            }
        }
        public override int EndOffset
        {
            get => _endAnchor.Offset;
            set
            {
                if (value < StartOffset)
                {
                    throw new ArgumentException("'EndOffset' can't be less than 'StartOffset'.");
                }
                Int32 oldOffset = EndOffset;
                if (oldOffset != value)
                {
                    Int32 diff = value - oldOffset;
                    if (diff > 0)
                    {
                        _owner.Document.MoveAnchorRight(_endAnchor, diff);
                    }
                    else
                    {
                        _owner.Document.MoveAnchorLeft(_endAnchor, -diff);
                    }
                }
            }
        }

        public override void Render(DrawingContext drawingContext, RenderContext renderContext)
        {
            if (IsEmpty)
            {
                return;
            }
        }
        #endregion

        private TextAnchor _startAnchor;
        private TextAnchor _endAnchor;
    }
}
