using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace EditorSupport.Highlighting
{
    public class CommonHighlighter : IHighlighter
    {
        public Dictionary<Int32, HighlightStyle> StyleMap => _styleMap;
        public HighlightStyle DefaultStyle { get => _defaultStyle; set => _defaultStyle = value; }

        public CommonHighlighter()
        {
            _styleMap = new Dictionary<int, HighlightStyle>();
            _defaultStyle = new HighlightStyle
            {
                Foreground = Colors.Black,
                Background = Colors.Transparent,
                FontWeight = FontWeights.Normal,
                FontStyle = FontStyles.Normal,
                FontStretch = FontStretches.Normal,
            };
        }

        public void Highlight(IHighlightRuler ruler, IHighlightee highlightee)
        {
            ruler.FormulateRule(highlightee);
            if (_styleMap.ContainsKey(highlightee.HighlightRule))
            {
                HighlightStyle style = _styleMap[highlightee.HighlightRule];
                highlightee.ForegroundBrush = new SolidColorBrush(style.Foreground);
                highlightee.BackgroundBrush = new SolidColorBrush(style.Background);
                highlightee.FontWeight = style.FontWeight;
                highlightee.FontStyle = style.FontStyle;
                highlightee.FontStretch = style.FontStretch;
            }
            else
            {
                HighlightStyle style = _defaultStyle;
                highlightee.ForegroundBrush = new SolidColorBrush(style.Foreground);
                highlightee.BackgroundBrush = new SolidColorBrush(style.Background);
                highlightee.FontWeight = style.FontWeight;
                highlightee.FontStyle = style.FontStyle;
                highlightee.FontStretch = style.FontStretch;
            }
        }

        private Dictionary<Int32, HighlightStyle> _styleMap;
        private HighlightStyle _defaultStyle;
    }
}
