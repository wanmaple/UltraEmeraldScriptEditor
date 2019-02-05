using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace EditorSupport.Rendering
{
    public class VisualLineRenderer : IRenderable
    {
        public LinkedList<VisualLine> VisibleLines { get; private set; }

        public VisualLineRenderer(RenderView editor)
        {
            _editor = editor ?? throw new ArgumentNullException("editor");
            VisibleLines = new LinkedList<VisualLine>();
        }

        #region IRenderable
        public void Render(DrawingContext context, FrameworkElement owner)
        {
            Point startPos = new Point(_editor.Padding.Left, _editor.Padding.Top);
            Typeface typeface = _editor.GlyphOption.Typeface;
            foreach (var visualLine in VisibleLines)
            {
                String content = _editor.Document.GetLineText(visualLine.Line);
                var formattedText = new FormattedText(content, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, _editor.GlyphOption.FontSize, Brushes.Black);
                context.DrawText(formattedText, startPos);
                startPos.Y += _editor.GlyphOption.LineHeight;
            }
        }
        #endregion

        private RenderView _editor;
    }
}
