using EditorSupport.Document;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompileSupport.Syntax
{
    /// <summary>
    /// 面向过程上下文。
    /// </summary>
    /// <remarks>
    /// 一般而言，语法检查器是在工作线程构建上下文的，而智能提示需要读取上下文，所以需要保证线程安全。
    /// </remarks>
    public interface ISyntaxContext
    {
        /// <summary>
        /// 解析中的文档
        /// </summary>
        TextDocument Document { get; }
        /// <summary>
        /// 解析中的偏移
        /// </summary>
        Int32 CheckingOffset { get; }
        /// <summary>
        /// 解析中的长度
        /// </summary>
        Int32 CheckingLength { get; }
        /// <summary>
        /// 常量
        /// </summary>
        IDictionary<String, ISyntaxToken> Constants { get; }
        /// <summary>
        /// 宏
        /// </summary>
        IDictionary<String, ISyntaxToken> Macros { get; }
        /// <summary>
        /// 函数
        /// </summary>
        IDictionary<String, ISyntaxToken> Functions { get; }
        /// <summary>
        /// 所有表达式
        /// </summary>
        IList<IStatement<ISyntaxToken>> Statements { get; }

        /// <summary>
        /// 获取当前作用域
        /// </summary>
        IVisitScope Current { get; }

        /// <summary>
        /// 入栈新的作用域
        /// </summary>
        void Push(IVisitScope scope);
        /// <summary>
        /// 出栈当前作用域
        /// </summary>
        void Pop();
    }
}
