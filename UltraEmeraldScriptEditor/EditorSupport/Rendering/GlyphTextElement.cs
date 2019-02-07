using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace EditorSupport.Rendering
{
    /// <summary>
    /// 通过GlyphRunDrawing绘制文本的元素
    /// </summary>
    public sealed class GlyphTextElement : VisualLineElement
    {
        private class GlyphRunParams
        {
            public Boolean ForGlobal { get; set; }
            public List<UInt16> Indices { get; set; }
            public List<Double> AdvanceWidths { get; set; }
            public Double TotalWidth { get; set; }

            public GlyphRunParams(Boolean forGlobal)
            {
                Indices = new List<ushort>(50);
                AdvanceWidths = new List<double>(50);
                TotalWidth = 0.0;
                ForGlobal = forGlobal;
            }
        }

        #region Constructor
        public GlyphTextElement(VisualLine owner, Int32 relativeOffset, Int32 length, String content)
            : base(owner, relativeOffset, length)
        {
            _content = content ?? throw new ArgumentNullException("content");
            _drawings = new List<Drawing>();
        }
        #endregion

        public void GenerateTypeface(FontFamily fontFamily, Int32 fontSize, Double lineHeight)
        {
            if (fontFamily == null)
            {
                throw new ArgumentNullException("fontFamily");
            }
            _glyphTypeface = TypefaceGenerator.GetInstance().GenerateGlyphTypeface(fontFamily, FontStyle, FontWeight, FontStretch);
            _globalGlyphTypeface = TypefaceGenerator.GetInstance().GenerateGlyphTypeface(new FontFamily("Microsoft YaHei"), FontStyle, FontWeight, FontStretch);
            _fontSize = fontSize;
            _lineHeight = lineHeight;
        }

        #region Overrides
        public override string Content => _content;

        public override IEnumerable<Drawing> GenerateDrawings(DrawingContext drawingContext, RenderContext renderContext)
        {
            if (_glyphTypeface == null)
            {
                return new List<Drawing>();
            }

            _drawings.Clear();
            _totalWidth = 0.0;

            GlyphRunParams param = null;
            GlyphRun gr = null;
            Point origin = renderContext.Offset;
            // GlyphDrawing的起始点在左下角，所以要偏移一个LineHeight
            origin.Y += _lineHeight;
            Int32 idx = 0;
            // 起始点的偏移是0
            do
            {
                Char ch = _content[idx];
                if (_glyphTypeface.CharacterToGlyphMap.ContainsKey(ch))
                {
                    if (param != null && param.ForGlobal)
                    {
                        gr = new GlyphRun(_globalGlyphTypeface, 0, false, _fontSize, param.Indices, origin, param.AdvanceWidths, null, null, null, null, null, XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag));
                        _drawings.Add(new GlyphRunDrawing(_fgBrush, gr));
                        origin.X += param.TotalWidth;
                        param = null;
                    }

                    if (param == null)
                    {
                        param = new GlyphRunParams(false);
                    }
                    UInt16 indice = _glyphTypeface.CharacterToGlyphMap[ch];
                    Double width = _glyphTypeface.AdvanceWidths[indice] * _fontSize;
                    param.Indices.Add(indice);
                    param.AdvanceWidths.Add(width);
                    param.TotalWidth += width;
                    _totalWidth += width;
                }
                else
                {
                    if (param != null && !param.ForGlobal)
                    {
                        gr = new GlyphRun(_glyphTypeface, 0, false, _fontSize, param.Indices, origin, param.AdvanceWidths, null, null, null, null, null, XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag));
                        _drawings.Add(new GlyphRunDrawing(_fgBrush, gr));
                        origin.X += param.TotalWidth;
                        param = null;
                    }

                    if (param == null)
                    {
                        param = new GlyphRunParams(true);
                    }
                    UInt16 indice = _globalGlyphTypeface.CharacterToGlyphMap[ch];
                    Double width = _globalGlyphTypeface.AdvanceWidths[indice] * _fontSize;
                    param.Indices.Add(indice);
                    param.AdvanceWidths.Add(width);
                    param.TotalWidth += width;
                    _totalWidth += width;
                }
                ++idx;
            } while (idx < _content.Length);

            if (param != null)
            {
                gr = new GlyphRun(param.ForGlobal ? _globalGlyphTypeface : _glyphTypeface, 0, false, _fontSize, param.Indices, origin, param.AdvanceWidths, null, null, null, null, null, XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag));
                _drawings.Add(new GlyphRunDrawing(_fgBrush, gr));
            }

            renderContext.PushTranslation(_totalWidth, 0.0);

            return _drawings;
        }
        #endregion

        private GlyphTypeface _glyphTypeface;
        // 当前字体可能有不支持的字符，所以需要一个支持全国通用的typeface来渲染
        private GlyphTypeface _globalGlyphTypeface;

        private List<Drawing> _drawings;
        private Double _totalWidth;
        private Int32 _fontSize;
        private Double _lineHeight;
        private String _content;
    }
}
