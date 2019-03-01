using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CompileSupport.Syntax
{
    /// <summary>
    /// 指一个可访问对象。
    /// </summary>
    public interface IVisitable
    {
        void Visit(ISyntaxContext context, IVisitRuler visitRuler, BinaryWriter writer);
    }
}
