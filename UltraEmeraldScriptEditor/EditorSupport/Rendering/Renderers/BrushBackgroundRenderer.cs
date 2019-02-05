using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace EditorSupport.Rendering
{
    public class BrushBackgroundRenderer : BackgroundRenderer
    {
        public static readonly DependencyProperty BrushProperty =
            DependencyProperty.Register("Brush", typeof(Brush), typeof(BackgroundRenderer), new PropertyMetadata(null));

        public Brush Brush
        {
            get { return (Brush)GetValue(BrushProperty); }
            set { SetValue(BrushProperty, value); }
        }

        public BrushBackgroundRenderer()
        {
        }

        public override void Render(DrawingContext context, FrameworkElement owner)
        {
            if (Brush != null)
            {
                switch (RenderType)
                {
                    case RenderType.FILL:
                        context.DrawRectangle(Brush, null, new Rect(new Size(owner.ActualWidth, owner.ActualHeight)));
                        break;
                    case RenderType.CENTER:
                        Double boundWidth = owner.ActualWidth;
                        Double boundHeight = owner.ActualHeight;
                        Point start = new Point((boundWidth - Width) * 0.5, (boundHeight - Height) * 0.5);
                        Point end = new Point(start.X + Width, start.Y + Height);
                        context.DrawRectangle(Brush, null, new Rect(start, end));
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
