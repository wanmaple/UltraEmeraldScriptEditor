using EditorSupport.Document;
using EditorSupport.Rendering.Renderers;
using EditorSupport.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace EditorSupport.Rendering
{
    /// <summary>
    /// DocumentLine的可视对象。
    /// </summary>
    public sealed class VisualLine : IRenderable, IDisposable
    {
        #region Properties
        public TextDocument Document { get; private set; }

        public DocumentLine Line { get; private set; }

        public LinkedList<VisualLineElement> Elements { get; private set; }

        public ValueSequence CharacterVisualOffsets { get; private set; }

        public Double VisualLength
        {
            get { return CharacterVisualOffsets.GetSumValue(CharacterVisualOffsets.Count); }
        }

        public RenderView Owner { get; private set; }
        #endregion

        #region Constructor
        public VisualLine(RenderView owner, TextDocument doc, DocumentLine line)
        {
            Document = doc ?? throw new ArgumentNullException("doc");
            Line = line ?? throw new ArgumentNullException("line");
            Owner = owner ?? throw new ArgumentNullException("owner");
            Elements = new LinkedList<VisualLineElement>();
            CharacterVisualOffsets = new ValueSequence();
            Rebuild();
        }
        #endregion

        #region Rebuilding
        public void Rebuild()
        {
            RebuildElements();
            RebuildVisualOffsets();
        }

        private void RebuildVisualOffsets()
        {
            // 本来是打算用一个数组存储偏移，但是实际上很多字符之间的偏移是一样的，为了节省内存这里定义一种新的结构。
            CharacterVisualOffsets.Clear();
            if (Line.Length == 0)
            {
                return;
            }

            String text = Document.GetLineText(Line);
            foreach (var elem in Elements)
            {
                GlyphTypeface globalGlyphTypeface = TypefaceGenerator.GetInstance().GenerateGlyphTypeface(new FontFamily("Microsoft YaHei"), elem.FontStyle, elem.FontWeight, elem.FontStretch);
                GlyphTypeface glyphTypeface = TypefaceGenerator.GetInstance().GenerateGlyphTypeface(Owner.GlyphOption.FontFamily, elem.FontStyle, elem.FontWeight, elem.FontStretch);
                for (int i = 0; i < elem.Length; i++)
                {
                    Char ch = text[elem.RelativeOffset + i];
                    UInt16 indice;
                    Double width;
                    if (glyphTypeface.CharacterToGlyphMap.ContainsKey(ch))
                    {
                        indice = glyphTypeface.CharacterToGlyphMap[ch];
                        width = glyphTypeface.AdvanceWidths[indice] * Owner.GlyphOption.FontSize;
                    }
                    else
                    {
                        indice = globalGlyphTypeface.CharacterToGlyphMap[ch];
                        width = globalGlyphTypeface.AdvanceWidths[indice] * Owner.GlyphOption.FontSize;
                    }
                    CharacterVisualOffsets.Add(width);
                }
            }
        }

        private void RebuildElements()
        {
            Elements.Clear();
            if (Line.Length == 0)
            {
                return;
            }
            String text = Document.GetLineText(Line);
            using (var sr = new StringReader(text))
            {
                Owner.HighlightRuler.SplitText(sr, (offset, length) =>
                {
                    // -1表示直接读到结尾
                    if (length == -1)
                    {
                        length = text.Length - offset;
                    }
                    var elem = new GlyphTextElement(this, offset, length, text.Substring(offset, length));
                    Owner.Highlighter.Highlight(Owner.HighlightRuler, elem);
                    elem.GenerateTypeface(Owner.GlyphOption.FontFamily, Owner.GlyphOption.FontSize, Owner.GlyphOption.LineHeight);
                    Elements.AddLast(elem);
                });
            }
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            VisualLineImageCache.GetInstance().RemoveCache(Document, Line);
        }
        #endregion

        #region IRenderable
        public void Render(DrawingContext drawingContext, RenderContext renderContext)
        {
            // DrawingImage会自动Trimming，怎么解决这个问题？
#if false
            DrawingImage cachedImg = VisualLineImageCache.GetInstance().GetCache(Document, Line);
            DrawingGroup drawingGroup = null;
            if (cachedImg == null)
            {
                renderContext.PrepareRendering();

                drawingGroup = new DrawingGroup();
                foreach (VisualLineElement elem in Elements)
                {
                    var drawings = elem.GenerateDrawings(drawingContext, renderContext);
                    if (drawings != null)
                    {
                        foreach (var drawing in drawings)
                        {
                            drawingGroup.Children.Add(drawing);
                        }
                    }
                }
                cachedImg = new DrawingImage(drawingGroup);
                VisualLineImageCache.GetInstance().AddCache(Document, Line, cachedImg);

                renderContext.FinishRendering();
            }
            cachedImg.Freeze();
            Point startPos = renderContext.Offset;
            startPos.Y += (Owner.GlyphOption.LineHeight - cachedImg.Height);
            drawingContext.DrawImage(cachedImg, new Rect(startPos, new Size(cachedImg.Width, cachedImg.Height)));
            drawingContext.DrawDrawing(drawingGroup);
#else
            renderContext.PrepareRendering();

            foreach (VisualLineElement elem in Elements)
            {
                var drawings = elem.GenerateDrawings(drawingContext, renderContext);
                if (drawings != null)
                {
                    foreach (var drawing in drawings)
                    {
                        drawingContext.DrawDrawing(drawing);
                    }
                }
            }

            renderContext.FinishRendering();
#endif
        }
        #endregion
    }
}
