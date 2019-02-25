using EditorSupport.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompileSupport.Syntax
{
    /// <summary>
    /// 面向过程作用域。
    /// </summary>
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
        Dictionary<String, ISyntaxToken> Constants { get; }
        /// <summary>
        /// 宏
        /// </summary>
        Dictionary<String, ISyntaxToken> Macros { get; }
        /// <summary>
        /// 函数
        /// </summary>
        Dictionary<String, ISyntaxToken> Functions { get; }
        /// <summary>
        /// 所有表达式
        /// </summary>
        List<IStatement<ISyntaxToken>> Statements { get; }

        /// <summary>
        /// 获取当前作用域
        /// </summary>
        IVisitScope Current { get; }

        /// <summary>
        /// 入栈新的作用域
        /// </summary>
        void Push();
        /// <summary>
        /// 出栈当前作用域
        /// </summary>
        void Pop();
    }
}
