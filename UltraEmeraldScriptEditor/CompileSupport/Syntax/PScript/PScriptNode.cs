using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompileSupport.Syntax.PScript
{
    public enum PscriptNodeType : Byte
    {
        None = 0,
        Keyword,
        Data,
        Function,
        Comment,
    }

    public abstract class PScriptNode : ISyntaxNode
    {
        #region ISyntaxNode
        public String Source => _source;
        public Object Value => _value; 
        #endregion

        public PscriptNodeType NodeType => _nodeType;

        protected PScriptNode(String source, Object value)
        {
            _source = source ?? throw new ArgumentNullException("source");
            _value = value ?? throw new ArgumentNullException("value");
            _nodeType = PscriptNodeType.None;
        }
        
        protected String _source;
        protected Object _value;
        protected PscriptNodeType _nodeType;
    }
}
