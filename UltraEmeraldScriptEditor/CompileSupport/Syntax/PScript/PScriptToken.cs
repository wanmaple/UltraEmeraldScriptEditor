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
        #region ISyntaxNode
        public String Source => _source;
        public Object Value => _value; 
        #endregion

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
        protected Object _value;
        protected PScriptTokenType _tokenType;
    }
}
