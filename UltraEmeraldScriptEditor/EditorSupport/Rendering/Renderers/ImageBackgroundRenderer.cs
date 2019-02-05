using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace EditorSupport.Rendering
{
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

        public override void Render(DrawingContext context, FrameworkElement owner)
        {
            if (ImageSource != null)
            {
                switch (RenderType)
                {
                    case RenderType.FILL:
                        context.DrawImage(ImageSource, new Rect(new Size(owner.ActualWidth, owner.ActualHeight)));
                        break;
                    case RenderType.CENTER:
                        Double boundWidth = owner.ActualWidth;
                        Double boundHeight = owner.ActualHeight;
                        Point start = new Point((boundWidth - Width) * 0.5, (boundHeight - Height) * 0.5);
                        Point end = new Point(start.X + Width, start.Y + Height);
                        context.DrawImage(ImageSource, new Rect(start, end));
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
