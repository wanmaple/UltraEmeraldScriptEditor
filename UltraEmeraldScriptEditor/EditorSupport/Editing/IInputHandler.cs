using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace EditorSupport.Editing
{
    /// <summary>
    /// 输入处理。
    /// </summary>
    public interface IInputHandler
    {
        /// <summary>
        /// 拥有者
        /// </summary>
        EditView Owner { get; }
        /// <summary>
        /// 生效
        /// </summary>
        void Attach();
        /// <summary>
        /// 失效
        /// </summary>
        void Detach();
    }
}
