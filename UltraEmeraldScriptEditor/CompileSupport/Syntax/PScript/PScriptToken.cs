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
        /// <summary>
        /// 通用访问逻辑
        /// </summary>
        /// <param name="context"></param>
        /// <param name="visitRuler"></param>
        /// <param name="writer"></param>
        public virtual void Visit(ISyntaxContext context, IVisitRuler visitRuler, BinaryWriter writer)
        {
            visitRuler.Previsit(context, writer);
            if (visitRuler.Override)
            {
                visitRuler.Visit(context, writer);
            }
            else
            {
                Visit(context, writer);
            }
            visitRuler.Postvisit(context, writer);
        }
        #endregion

        /// <summary>
        /// 自身访问逻辑
        /// </summary>
        /// <param name="context"></param>
        /// <param name="writer"></param>
        protected abstract void Visit(ISyntaxContext context, BinaryWriter writer);

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
