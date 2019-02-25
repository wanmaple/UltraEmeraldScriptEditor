using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CompileSupport.Syntax
{
    /// <summary>
    /// 基本元素。
    /// </summary>
    public interface ISyntaxToken : IVisitable
    {
        /// <summary>
        /// 源码
        /// </summary>
        String Source { get; }
        /// <summary>
        /// 是否参与访问过程
        /// </summary>
        Boolean Visitable { get; }
    }
}
