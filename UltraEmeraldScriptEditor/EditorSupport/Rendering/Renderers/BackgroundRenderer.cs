using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace EditorSupport.Rendering
{
    public enum RenderType
    {
        FILL,
        CENTER,
    }

    public abstract class BackgroundRenderer : DependencyObject, IRenderable
    {
        #region Properties
        public static readonly DependencyProperty WidthProperty =
    DependencyProperty.Register("Width", typeof(Double), typeof(BackgroundRenderer), new PropertyMetadata(0.0));
        public static readonly DependencyProperty HeightProperty =
            DependencyProperty.Register("Height", typeof(Double), typeof(BackgroundRenderer), new PropertyMetadata(0.0));
        public static readonly DependencyProperty RenderTypeProperty =
            DependencyProperty.Register("RenderType", typeof(RenderType), typeof(BackgroundRenderer), new PropertyMetadata(RenderType.FILL));

        public Double Width
        {
            get { return (Double)GetValue(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }
        public Double Height
        {
            get { return (Double)GetValue(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        } 
        public RenderType RenderType
        {
            get { return (RenderType)GetValue(RenderTypeProperty); }
            set { SetValue(RenderTypeProperty, value); }
        }
        #endregion

        internal void BindEditorView(RenderView editor)
        {
            _editor = editor;
        }

        #region IRenderable
        public abstract void Render(DrawingContext context, FrameworkElement owner);
        #endregion

        protected RenderView _editor;
    }
}
