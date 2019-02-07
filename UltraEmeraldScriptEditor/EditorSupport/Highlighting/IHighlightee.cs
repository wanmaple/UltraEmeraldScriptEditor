using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace EditorSupport.Highlighting
{
    /// <summary>
    /// 可以被高亮的对象
    /// </summary>
    public interface IHighlightee
    {
        Brush ForegroundBrush { get; set; }

        Brush BackgroundBrush { get; set; }

        FontWeight FontWeight { get; set; }

        FontStyle FontStyle { get; set; }

        FontStretch FontStretch { get; set; }

        /// <summary>
        /// 高亮规则
        /// </summary>
        Int32 HighlightRule { get; set; }

        String Content { get; }
    }
}
