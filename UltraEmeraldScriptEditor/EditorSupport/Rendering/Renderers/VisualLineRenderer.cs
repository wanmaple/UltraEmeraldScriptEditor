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
        public Point RenderOffset { get => _renderOffset; set => _renderOffset = value; }

        public VisualLineRenderer(RenderView editor)
        {
            _editor = editor ?? throw new ArgumentNullException("editor");
            VisibleLines = new LinkedList<VisualLine>();
        }

        #region IRenderable
        public void Render(DrawingContext drawingContext, RenderContext renderContext)
        {
            if (VisibleLines.Count > 0)
            {
                foreach (var visualLine in VisibleLines)
                {
                    if (visualLine.VisualLength + _renderOffset.X > 0)
                    {
                        visualLine.Render(drawingContext, renderContext);
                    }
                    renderContext.PushTranslation(0.0, _editor.GlyphOption.LineHeight);
                }
            }
        }
        #endregion

        private RenderView _editor;
        private Point _renderOffset;
    }
}
