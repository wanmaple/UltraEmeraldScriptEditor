using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace EditorSupport.Rendering.Renderers
{
    /// <summary>
    /// 可渲染的对象。
    /// </summary>
    public interface IRenderable
    {
        void Render(DrawingContext drawingContext, RenderContext renderContext);
    }
}
