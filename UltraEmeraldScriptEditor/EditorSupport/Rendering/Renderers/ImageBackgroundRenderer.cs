using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace EditorSupport.Rendering
{
    [Obsolete("BackgroundRenderers are obsoleted.")]
    public class ImageBackgroundRenderer : BackgroundRenderer
    {
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(ImageBackgroundRenderer), new PropertyMetadata(null));

        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public ImageBackgroundRenderer()
        {
        }

        public override void Render(DrawingContext drawingContext, RenderContext renderContext)
        {
            if (ImageSource != null)
            {
                switch (RenderType)
                {
                    case RenderType.DEFAULT:
                        drawingContext.DrawImage(ImageSource, new Rect(renderContext.Offset, new Size(Width, Height)));
                        break;
                    case RenderType.FILL:
                        drawingContext.DrawImage(ImageSource, renderContext.Region);
                        break;
                    case RenderType.CENTER:
                        Double boundWidth = renderContext.Region.Width;
                        Double boundHeight = renderContext.Region.Height;
                        Point start = new Point((boundWidth - Width) * 0.5, (boundHeight - Height) * 0.5);
                        Point end = new Point(start.X + Width, start.Y + Height);
                        drawingContext.DrawImage(ImageSource, new Rect(start, end));
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
