using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompileSupport.Syntax.PScript
{
    public sealed class PScriptKeyword : PScriptNode
    {
        public IEnumerable<PScriptValueType> ParamTypes => _paramTypes;

        public PScriptKeyword(String source, PScriptValueType[] paramTypes)
            : base(source)
        {
            _value = _source;
            _paramTypes = paramTypes ?? throw new ArgumentNullException("paramTypes");
        }

        private PScriptValueType[] _paramTypes;
    }
}
