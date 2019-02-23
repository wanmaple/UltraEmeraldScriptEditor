using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CompileSupport.Syntax.PScript
{
    public sealed class PScriptKeyword : PScriptToken
    {
        public IEnumerable<PScriptDataType> ParamTypes => _paramTypes;
        public ICompileRuler CompileRuler => _compileRuler;

        public PScriptKeyword(String source, PScriptDataType[] paramTypes)
            : base(source)
        {
            _value = _source;
            _paramTypes = paramTypes ?? throw new ArgumentNullException("paramTypes");
        }

        private ICompileRuler _compileRuler;
        private PScriptDataType[] _paramTypes;
    }
}
