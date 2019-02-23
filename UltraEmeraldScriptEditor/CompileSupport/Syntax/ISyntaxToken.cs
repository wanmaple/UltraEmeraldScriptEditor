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
    public interface ISyntaxToken
    {
        String Source { get; }
        Object Value { get; }
    }
}
