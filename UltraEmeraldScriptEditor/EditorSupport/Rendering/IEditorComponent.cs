using EditorSupport.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EditorSupport.Rendering
{
    /// <summary>
    /// 编辑器基础。
    /// </summary>
    public interface IEditorComponent
    {
        /// <summary>
        /// Document改变时触发（注意不是Document内容的改变，而是Document属性的改变）
        /// </summary>
        event EventHandler DocumentChanged;

        /// <summary>
        /// Editor所关联的Document对象
        /// </summary>
        TextDocument Document { get; }
    }
}
