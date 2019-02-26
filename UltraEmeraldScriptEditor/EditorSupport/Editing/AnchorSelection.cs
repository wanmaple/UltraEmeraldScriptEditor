using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using EditorSupport.Document;
using EditorSupport.Rendering;

namespace EditorSupport.Editing
{
    /// <summary>
    /// 由前后锚点组成的选中框。
    /// </summary>
    public sealed class AnchorSelection : Selection
    {
        public AnchorSelection(EditView owner, TextAnchor startAnchor, TextAnchor endAnchor)
            : base(owner)
        {
            // 锚点的偏移完全由自身控制，所以设置为AnchorMovementType.Ignore
            _startAnchor = startAnchor ?? throw new ArgumentNullException("startAnchor");
            _startAnchor.MovementType = AnchorMovementType.Ignore;
            _endAnchor = endAnchor ?? throw new ArgumentNullException("endAnchor");
            _endAnchor.MovementType = AnchorMovementType.Ignore;
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
                    TriggerOffsetChanged();
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
                    TriggerOffsetChanged();
                }
            }
        }
        #endregion

        private TextAnchor _startAnchor;
        private TextAnchor _endAnchor;
    }
}
