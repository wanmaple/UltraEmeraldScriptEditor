using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompileSupport.Syntax.PScript
{
    public enum PScriptTokenType : Byte
    {
        None = 0,
        /// <summary>
        /// 关键字
        /// </summary>
        Keyword,
        /// <summary>
        /// 数据
        /// </summary>
        Data,
        /// <summary>
        /// 宏
        /// </summary>
        Macro,
        /// <summary>
        /// 参数
        /// </summary>
        Parameter,
        /// <summary>
        /// 注释
        /// </summary>
        Comment,
    }

    public abstract class PScriptToken : ISyntaxToken
    {
        #region ISyntaxToken
        public String Source => _source;
        public abstract bool Compileable { get; }
        /// <summary>
        /// 通用编译逻辑，不使用自身编译的时候需要重写这个方法
        /// </summary>
        /// <param name="context"></param>
        /// <param name="compileRuler"></param>
        /// <param name="writer"></param>
        public virtual void Compile(ISyntaxContext context, ICompileRuler compileRuler, BinaryWriter writer)
        {
            if (Compileable)
            {
                compileRuler.Precompile(context, writer);
                if (compileRuler.OverrideCompile)
                {
                    compileRuler.Compile(context, writer);
                }
                else
                {
                    Compile(context, writer);
                }
                compileRuler.Postcompile(context, writer);
            }
        }
        #endregion

        /// <summary>
        /// 自身编译逻辑
        /// </summary>
        /// <param name="context"></param>
        /// <param name="writer"></param>
        protected abstract void Compile(ISyntaxContext context, BinaryWriter writer);

        public PScriptTokenType TokenType => _tokenType;

        protected PScriptToken(String source)
        {
            _source = source ?? throw new ArgumentNullException("source");
            _tokenType = PScriptTokenType.None;
        }

        public override string ToString()
        {
            return _source;
        }

        protected String _source;
        protected PScriptTokenType _tokenType;
    }
}
