using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace EditorSupport.Rendering
{
    public abstract class VisualLineElement
    {
        public Brush ForegroundBrush { get; set; }

        public abstract FormattedText FormattedText { get; }
    }
}
