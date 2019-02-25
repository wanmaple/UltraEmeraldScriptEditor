using CompileSupport.Syntax.Exceptions;
using CompileSupport.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CompileSupport.Syntax.PScript
{
    /// <summary>
    /// Byte数据（一个字节）。
    /// </summary>
    public sealed class PScriptByte : PScriptToken
    {
        public PScriptByte(string source)
            : base(source)
        {
            var calc = new RPNIntegerCalculator();
            _data = Convert.ToByte(calc.Calculate(_source));
        }

        #region Overrides
        public override bool Visitable => true;

        public override string ToString()
        {
            return _data.ToString();
        }

        protected override void Visit(ISyntaxContext context, BinaryWriter writer)
        {
            writer.Write(_data);
        }
        #endregion

        private Byte _data;
    }
}
