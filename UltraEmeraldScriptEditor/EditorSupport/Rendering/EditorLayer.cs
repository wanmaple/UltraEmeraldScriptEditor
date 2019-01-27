using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace EditorSupport.Rendering
{
    /// <summary>
    /// 编辑器中的层的概念。
    /// </summary>
    /// <remarks>
    /// 编辑器实际是由很多层组成。
    /// 背景层<see cref="EditorBackgroundLayer"/>
    /// 选中层<see cref="EditorSelectionLayer"/>
    /// 文字层<see cref="EditorTextLayer"/>
    /// 光标层<see cref="EditorCaretLayer"/>
    /// </remarks>
    internal abstract class EditorLayer : UIElement
    {
    }
}
