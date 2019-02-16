using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EditorSupport.Editing
{
    public sealed class EditingOffsetUpdate
    {
        public Int32 CaretOffsetEarlier { get; set; }
        public Int32 CaretOffsetLater { get; set; }
        public Int32 SelectionStartEarlier { get; set; }
        public Int32 SelectionEndEarlier { get; set; }
        public Int32 SelectionStartLater { get; set; }
        public Int32 SelectionEndLater { get; set; }

        public EditingOffsetUpdate Clone()
        {
            return new EditingOffsetUpdate
            {
                CaretOffsetEarlier = CaretOffsetEarlier,
                CaretOffsetLater = CaretOffsetLater,
                SelectionStartEarlier = SelectionStartEarlier,
                SelectionStartLater = SelectionStartLater,
                SelectionEndEarlier = SelectionEndEarlier,
                SelectionEndLater = SelectionEndLater,
            };
        }
    }
}
