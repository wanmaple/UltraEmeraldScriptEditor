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
    /// Word数据（四个字节）。
    /// </summary>
    public sealed class PScriptWord : PScriptToken
    {
        public PScriptWord(string source)
            : base(source)
        {
            var calc = new RPNIntegerCalculator();
            _data = Convert.ToUInt32(calc.Calculate(_source));
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

        private UInt32 _data;
    }
}
