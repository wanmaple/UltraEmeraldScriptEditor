using CompileSupport.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompileSupport.Compiler
{
    /// <summary>
    /// 编译器。
    /// </summary>
    public interface ICompiler
    {
        /// <summary>
        /// 编译
        /// </summary>
        /// <param name="context"></param>
        void Compile(ISyntaxContext context);
        /// <summary>
        /// 清理
        /// </summary>
        void Clean();
        /// <summary>
        /// 重编译
        /// </summary>
        /// <param name="context"></param>
        void Recompile(ISyntaxContext context);
    }
}
