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
    /// Halfword数据（两个字节）。
    /// </summary>
    public sealed class PScriptHalfword : PScriptToken
    {
        public PScriptHalfword(string source)
            : base(source)
        {
            var calc = new RPNIntegerCalculator();
            _data = Convert.ToUInt16(calc.Calculate(_source));
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

        private UInt16 _data;
    }
}
