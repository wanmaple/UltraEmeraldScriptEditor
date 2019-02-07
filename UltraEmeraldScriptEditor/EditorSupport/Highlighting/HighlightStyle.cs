using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace EditorSupport.Highlighting
{
    public sealed class HighlightStyle
    {
        public Color Foreground { get; set; }
        public Color Background { get; set; }
        public FontWeight FontWeight { get; set; }
        public FontStyle FontStyle { get; set; }
        public FontStretch FontStretch { get; set; }

        public HighlightStyle()
        {
            Foreground = Colors.Black;
            Background = Colors.Transparent;
            FontWeight = FontWeights.Normal;
            FontStyle = FontStyles.Normal;
            FontStretch = FontStretches.Normal;
        }
    }
}
