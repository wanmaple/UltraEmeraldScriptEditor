using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CompileSupport.Syntax
{
    /// <summary>
    /// 访问规则。
    /// </summary>
    public interface IVisitRuler
    {
        /// <summary>
        /// 访问中的参数索引
        /// </summary>
        Int32 ParamIndex { get; }
        /// <summary>
        /// 是否覆盖原有的访问方式
        /// </summary>
        Boolean Override { get; }

        /// <summary>
        /// 访问前
        /// </summary>
        /// <param name="context"></param>
        /// <param name="writer"></param>
        void Previsit(ISyntaxContext context, BinaryWriter writer);
        /// <summary>
        /// 覆盖的访问方式，仅当OverrideCompile为true时生效
        /// </summary>
        /// <param name="context"></param>
        /// <param name="writer"></param>
        void Visit(ISyntaxContext context, BinaryWriter writer);
        /// <summary>
        /// 访问后
        /// </summary>
        /// <param name="context"></param>
        /// <param name="writer"></param>
        void Postvisit(ISyntaxContext context, BinaryWriter writer);
    }
}
