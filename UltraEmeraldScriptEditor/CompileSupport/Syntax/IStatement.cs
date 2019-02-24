using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompileSupport.Syntax
{
    /// <summary>
    /// 表达式。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IStatement<T> : ICompileable
        where T : ISyntaxToken
    {
        /// <summary>
        /// 所属上下文
        /// </summary>
        ISyntaxContext Context { get; }
        /// <summary>
        /// 成员
        /// </summary>
        ReadOnlyCollection<T> Tokens { get; }
    }
}
