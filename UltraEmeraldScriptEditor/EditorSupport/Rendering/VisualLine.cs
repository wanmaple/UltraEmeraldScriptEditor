using EditorSupport.Document;
using EditorSupport.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace EditorSupport.Rendering
{
    /// <summary>
    /// DocumentLine的可视对象
    /// </summary>
    /// <remarks>
    /// 考虑到wpf的DrawingContext.DrawText性能堪忧，每一行又有不同样式的<see cref="VisualLineElement"/>，我们对每个没有编辑的行对象存储一份<see cref="DrawingImage"/>，在该行没有进行编辑的时候，直接用DrawingContext.DrawImage绘制，编辑中的行仍然用DrawingContext.DrawText绘制。
    /// </remarks>
    public sealed class VisualLine : IRenderable, IDisposable
    {
        #region Properties
        public TextDocument Document { get; private set; }

        public DocumentLine Line { get; private set; }

        public LinkedList<VisualLineElement> Elements { get; private set; }

        public GlyphProperties GlyphProperties { get; private set; }

        public ValueSequence CharacterVisualOffsets { get; private set; }

        public Double VisualLength
        {
            get { return CharacterVisualOffsets.GetSumValue(CharacterVisualOffsets.Count); }
        }
        #endregion

        #region Constructor
        public VisualLine(TextDocument doc, DocumentLine line, GlyphProperties glyphProperties)
        {
            Document = doc ?? throw new ArgumentNullException("doc");
            Line = line ?? throw new ArgumentNullException("line");
            GlyphProperties = glyphProperties ?? throw new ArgumentNullException("glyphProperties");
            Elements = new LinkedList<VisualLineElement>();
            CharacterVisualOffsets = new ValueSequence();
            Rebuild();
        } 
        #endregion

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
                GlyphTypeface glyphTypeface = TypefaceGenerator.GetInstance().GenerateGlyphTypeface(GlyphProperties.FontFamily, elem.FontStyle, elem.FontWeight, elem.FontStretch);
                for (int i = 0; i < elem.Length; i++)
                {
                    Char ch = text[elem.RelativeOffset + i];
                    UInt16 indice;
                    Double width;
                    if (glyphTypeface.CharacterToGlyphMap.ContainsKey(ch))
                    {
                        indice = glyphTypeface.CharacterToGlyphMap[ch];
                        width = glyphTypeface.AdvanceWidths[indice] * GlyphProperties.FontSize;
                    }
                    else
                    {
                        indice = globalGlyphTypeface.CharacterToGlyphMap[ch];
                        width = globalGlyphTypeface.AdvanceWidths[indice] * GlyphProperties.FontSize;
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
            Int32 startIdx = 0;
            Boolean spaceAtFirst = true;
            GlyphTextElement elem = null;
            String content = null;
            for (int i = 0; i < text.Length; ++i)
            {
                Char ch = text[i];
                if (spaceAtFirst && ch == ' ')
                {
                    continue;
                }
                else if (spaceAtFirst && ch != ' ')
                {
                    spaceAtFirst = false;
                    continue;
                }
                else if (ch == ' ')
                {
                    Int32 relativeIdx = startIdx;
                    Int32 length = i - startIdx;
                    content = text.Substring(startIdx, length);
                    elem = new GlyphTextElement(this, relativeIdx, length, content);
                    elem.GenerateTypeface(GlyphProperties.FontFamily, GlyphProperties.FontSize, GlyphProperties.LineHeight);
                    Elements.AddLast(elem);
                    startIdx = i;
                    spaceAtFirst = true;
                }
            }
            {
                Int32 relativeIdx = startIdx;
                Int32 length = text.Length - startIdx;
                content = text.Substring(relativeIdx, length);
                elem = new GlyphTextElement(this, relativeIdx, length, content);
                elem.ForegroundBrush = Brushes.Blue;
                elem.FontWeight = FontWeights.Bold;
                elem.GenerateTypeface(GlyphProperties.FontFamily, GlyphProperties.FontSize, GlyphProperties.LineHeight);
                Elements.AddLast(elem);
            }
        }

        #region IDisposable
        public void Dispose()
        {
            VisualLineImageCache.GetInstance().RemoveCache(Document, Line);
        }
        #endregion

        #region IRenderable
        public void Render(DrawingContext drawingContext, RenderContext renderContext)
        {
            DrawingImage cachedImg = VisualLineImageCache.GetInstance().GetCache(Document, Line);
            if (cachedImg == null)
            {
                renderContext.PrepareRendering();

                var drawingGroup = new DrawingGroup();
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
            drawingContext.DrawImage(cachedImg, new Rect(startPos, new Size(cachedImg.Width, cachedImg.Height)));
            //for (int i = 0; i <= CharacterVisualOffsets.Count; i++)
            //{
            //    var pos = new Point(startPos.X + CharacterVisualOffsets.GetSumValue(i) - 1, startPos.Y);
            //    drawingContext.DrawRectangle(Brushes.Red, null, new Rect(pos, new Size(2, cachedImg.Height)));
            //}
        }
        #endregion
    }
}
