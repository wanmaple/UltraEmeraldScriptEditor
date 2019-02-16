using EditorSupport.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace EditorSupport.Rendering.Renderers
{
    /// <summary>
    /// 行号绘制器。
    /// </summary>
    public sealed class LineNumberRenderer : IRenderable
    {
        public Double RenderWidth { get; private set; }

        public LineNumberRenderer(RenderView editor)
        {
            _editor = editor ?? throw new ArgumentNullException("editor");
        }

        #region IRenderable
        public void Render(DrawingContext drawingContext, RenderContext renderContext)
        {
            Int32 lineCount = _editor.Document.LineCount;
            Int32 lineNumberCount = GetLineNumberCount(lineCount);
            Typeface typeface = TypefaceGenerator.GetInstance().GenerateTypeface(_editor.GlyphOption.FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
            GlyphTypeface glyphTypeface = TypefaceGenerator.GetInstance().GenerateGlyphTypeface(_editor.GlyphOption.FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
            UInt16 indice = glyphTypeface.CharacterToGlyphMap['0'];
            Double characterWidth = glyphTypeface.AdvanceWidths[indice] * _editor.GlyphOption.FontSize;
            Double characterHeight = glyphTypeface.AdvanceHeights[indice] * _editor.GlyphOption.FontSize;
            Double maxNumberWidth = Math.Max(lineNumberCount * characterWidth, 3 * characterWidth);
            Point renderOffset = new Point(renderContext.Offset.X - _editor.HorizontalOffset, _editor._lineRenderer.RenderOffset.Y + _editor.Padding.Top);
            var brush = new SolidColorBrush(CommonUtilities.ColorFromHexString("#FF7D7D7D"));
            foreach (VisualLine visualLine in _editor._lineRenderer.VisibleLines)
            {
                var formattedText = new FormattedText(visualLine.Line.LineNumber.ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, _editor.GlyphOption.FontSize, brush);
                Point actualRenderOffset = renderOffset;
                actualRenderOffset.X += maxNumberWidth - formattedText.Width;
                actualRenderOffset.Y += (_editor.GlyphOption.LineHeight - characterHeight);
                drawingContext.DrawText(formattedText, actualRenderOffset);
                renderOffset.Y += _editor.GlyphOption.LineHeight;
            }
            drawingContext.DrawLine(new Pen(brush, 2.0), new Point(_editor._lineRenderer.RenderOffset.X + maxNumberWidth + 5.0, _editor._lineRenderer.RenderOffset.Y), new Point(_editor._lineRenderer.RenderOffset.X + maxNumberWidth + 5.0, 2000.0));
            
            RenderWidth = maxNumberWidth + 10.0;
        }
        #endregion

        private Int32 GetLineNumberCount(Int32 lineCount)
        {
            Int32 ret = 0;
            while (lineCount > 0)
            {
                lineCount /= 10;
                ++ret;
            }
            return ret;
        }

        private RenderView _editor;
    }
}
