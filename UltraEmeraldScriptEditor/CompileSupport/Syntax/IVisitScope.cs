using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompileSupport.Syntax
{
    public interface IVisitScope
    {
        /// <summary>
        /// 实参
        /// </summary>
        List<ISyntaxToken> Arguments { get; }
        /// <summary>
        /// 临时变量
        /// </summary>
        List<ISyntaxToken> Variables { get; }
    }
}
