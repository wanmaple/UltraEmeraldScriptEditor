using CompileSupport.Syntax.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CompileSupport.Syntax.PScript
{
    public sealed class PScriptByte : PScriptData
    {
        public PScriptByte(string source) 
            : base(source)
        {
            if (!Byte.TryParse(_source, out _data))
            {
                throw new SyntaxException(String.Format("'{0}' is invalid byte value.", _source), SyntaxErrorType.SYNTAX_ERROR_DATA_INVALID);
            }
            _dataType = PScriptDataType.Byte;
            _value = _data;
        }

        #region Overrides
        protected override void Compile(SyntaxContext context, BinaryWriter writer)
        {
            writer.Write(_data);
        }

        public override string ToString()
        {
            return _data.ToString();
        }
        #endregion

        private Byte _data;
    }
}
