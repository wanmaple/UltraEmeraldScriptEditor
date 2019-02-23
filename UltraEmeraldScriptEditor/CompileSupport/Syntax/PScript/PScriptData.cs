using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CompileSupport.Syntax.PScript
{
    public abstract class PScriptData : PScriptCompileableToken
    {
        public PScriptDataType DataType => _dataType;

        protected PScriptData(string source) 
            : base(source)
        {
        }

        protected PScriptDataType _dataType;
    }
}
