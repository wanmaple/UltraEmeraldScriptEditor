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

        public override void Render(DrawingContext drawingContext, RenderContext renderContext)
        {
            if (Brush != null)
            {
                switch (RenderType)
                {
                    case RenderType.DEFAULT:
                        drawingContext.DrawRectangle(Brush, null, new Rect(renderContext.Offset, new Size(Width, Height)));
                        break;
                    case RenderType.FILL:
                        drawingContext.DrawRectangle(Brush, null, renderContext.Region);
                        break;
                    case RenderType.CENTER:
                        Double boundWidth = renderContext.Region.Width;
                        Double boundHeight = renderContext.Region.Height;
                        Point start = new Point((boundWidth - Width) * 0.5, (boundHeight - Height) * 0.5);
                        Point end = new Point(start.X + Width, start.Y + Height);
                        drawingContext.DrawRectangle(Brush, null, new Rect(start, end));
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
