using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CompileSupport.Syntax
{
    /// <summary>
    /// 指一个可编译对象。
    /// </summary>
    public interface ICompileable
    {
        void Compile(ISyntaxContext context, ICompileRuler compileRuler, BinaryWriter writer);
    }
}
