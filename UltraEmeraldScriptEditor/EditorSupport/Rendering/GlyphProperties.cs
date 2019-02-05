using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace EditorSupport.Rendering
{
    /// <summary>
    /// 影响文字渲染的选项
    /// </summary>
    [Serializable]
    public sealed class GlyphProperties : DependencyObject
    {
        public event EventHandler OptionChanged;

        #region Properties
        public static readonly DependencyProperty FontFamilyProperty =
    DependencyProperty.Register("FontFamily", typeof(FontFamily), typeof(GlyphProperties), new PropertyMetadata(new FontFamily("consolas"), OnOptionChanged));
        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register("FontSize", typeof(Int32), typeof(GlyphProperties), new PropertyMetadata(12, OnOptionChanged));
        public static readonly DependencyProperty FontStyleProperty =
            DependencyProperty.Register("FontStyle", typeof(FontStyle), typeof(GlyphProperties), new PropertyMetadata(FontStyles.Normal, OnOptionChanged));
        public static readonly DependencyProperty FontWeightProperty =
            DependencyProperty.Register("FontWeight", typeof(FontWeight), typeof(GlyphProperties), new PropertyMetadata(FontWeights.Normal, OnOptionChanged));
        public static readonly DependencyProperty FontStretchProperty =
            DependencyProperty.Register("FontStretch", typeof(FontStretch), typeof(GlyphProperties), new PropertyMetadata(FontStretches.Normal, OnOptionChanged));

        public FontFamily FontFamily
        {
            get { return (FontFamily)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }
        public Int32 FontSize
        {
            get { return (Int32)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }
        public FontStyle FontStyle
        {
            get { return (FontStyle)GetValue(FontStyleProperty); }
            set { SetValue(FontStyleProperty, value); }
        }
        public FontWeight FontWeight
        {
            get { return (FontWeight)GetValue(FontWeightProperty); }
            set { SetValue(FontWeightProperty, value); }
        }
        public FontStretch FontStretch
        {
            get { return (FontStretch)GetValue(FontStretchProperty); }
            set { SetValue(FontStretchProperty, value); }
        }
        public Double LineHeight
        {
            get { return _lineHeight; }
        }
        public Typeface Typeface
        {
            get { return _typeface; }
        }
        public GlyphTypeface GlyphTypeface
        {
            get { return _glyphTypeface; }
        }
        #endregion

        public GlyphProperties()
        {
            Reset();
        }

        private void Reset()
        {
            _typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
            if (!_typeface.TryGetGlyphTypeface(out _glyphTypeface))
            {
                throw new ArgumentException("FontFamily is invalid.");
            }
            _lineHeight = FontSize + 3;
        }

        private static void OnOptionChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            GlyphProperties option = dp as GlyphProperties;
            option.Reset();
            if (option.OptionChanged != null)
            {
                option.OptionChanged(option, EventArgs.Empty);
            }
        }

        private Typeface _typeface;
        private GlyphTypeface _glyphTypeface;
        private Double _lineHeight;
    }
}
