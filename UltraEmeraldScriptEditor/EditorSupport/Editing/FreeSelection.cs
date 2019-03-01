using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EditorSupport.Editing
{
    /// <summary>
    /// 自由控制的选中框。
    /// </summary>
    public sealed class FreeSelection : Selection
    {
        #region Constructor
        public FreeSelection(EditView owner)
    : base(owner)
        {
            _startOffset = _endOffset = 0;
        } 
        #endregion

        #region Overrides
        public override int StartOffset
        {
            get => _startOffset;
            set
            {
                if (_startOffset != value)
                {
                    _startOffset = value;
                    RaiseOffsetChangedEvent();
                }
            }
        }
        public override int Length
        {
            get => _endOffset - _startOffset;
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
            get => _endOffset;
            set
            {
                if (_endOffset != value)
                {
                    _endOffset = value;
                    RaiseOffsetChangedEvent();
                }
            }
        } 
        #endregion

        private Int32 _startOffset, _endOffset;
    }
}
