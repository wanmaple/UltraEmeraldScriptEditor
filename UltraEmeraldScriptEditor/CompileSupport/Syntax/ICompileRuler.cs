using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CompileSupport.Syntax
{
    public interface ICompileRuler
    {
        /// <summary>
        /// 编译中的参数索引
        /// </summary>
        Int32 ParamIndex { get; }
        /// <summary>
        /// 是否覆盖原有的编译方式
        /// </summary>
        Boolean OverrideCompile { get; }

        /// <summary>
        /// 编译前
        /// </summary>
        /// <param name="context"></param>
        /// <param name="writer"></param>
        void Precompile(SyntaxContext context, BinaryWriter writer);
        /// <summary>
        /// 覆盖的编译方式，仅当OverrideCompile为true时生效
        /// </summary>
        /// <param name="context"></param>
        /// <param name="writer"></param>
        void Compile(SyntaxContext context, BinaryWriter writer);
        /// <summary>
        /// 编译后
        /// </summary>
        /// <param name="context"></param>
        /// <param name="writer"></param>
        void Postcompile(SyntaxContext context, BinaryWriter writer);
    }
}
