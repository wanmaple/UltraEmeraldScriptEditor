using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CompileSupport.Syntax.PScript
{
    /// <summary>
    /// 全局常量。
    /// </summary>
    public sealed class PScriptConstant : PScriptWrapper<PScriptToken>
    {
        public String Name => _name;

        public PScriptConstant(String name, PScriptToken innerToken)
            : base(name, innerToken)
        {
            _name = name ?? throw new ArgumentNullException("name");
            _innerToken = innerToken ?? throw new ArgumentNullException("innerData");
        }

        private PScriptToken _innerToken;
        private String _name;
    }
}
