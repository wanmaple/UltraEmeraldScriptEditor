using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EditorSupport.Editing
{
    public sealed class EditingOffsetUpdate
    {
        public Int32 CaretMoving { get; set; }
        public Int32 SelectionStartMoving { get; set; }
        public Int32 SelectionEndMoving { get; set; }

        public EditingOffsetUpdate Clone()
        {
            return new EditingOffsetUpdate
            {
                CaretMoving = CaretMoving,
                SelectionStartMoving = SelectionStartMoving,
                SelectionEndMoving = SelectionEndMoving,
            };
        }
    }
}
