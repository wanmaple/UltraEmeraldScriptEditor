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
    public sealed class GlyphProperties : DependencyObject
    {
        public event EventHandler OptionChanged;

        #region Properties
        public static readonly DependencyProperty FontFamilyProperty =
    DependencyProperty.Register("FontFamily", typeof(FontFamily), typeof(GlyphProperties), new PropertyMetadata(new FontFamily("consolas"), OnOptionChanged));
        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register("FontSize", typeof(Int32), typeof(GlyphProperties), new PropertyMetadata(15, OnOptionChanged));

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
        public Double LineHeight
        {
            get { return _lineHeight; }
        }
        #endregion

        public GlyphProperties()
        {
            Reset();
        }

        private void Reset()
        {
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
        
        private Double _lineHeight;
    }
}
