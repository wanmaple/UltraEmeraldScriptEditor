using CompileSupport.Syntax.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CompileSupport.Syntax.PScript
{
    public sealed class PScriptWord : PScriptData
    {
        public PScriptWord(string source)
            : base(source)
        {
            if (!UInt32.TryParse(_source, out _data))
            {
                throw new SyntaxException(String.Format("'{0}' is invalid word value.", _source), SyntaxErrorType.SYNTAX_ERROR_DATA_INVALID);
            }
            _dataType = PScriptDataType.Word;
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

        private UInt32 _data;
    }
}
