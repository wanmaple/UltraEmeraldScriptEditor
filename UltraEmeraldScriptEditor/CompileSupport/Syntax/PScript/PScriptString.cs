using CompileSupport.Syntax.Exceptions;
using CompileSupport.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CompileSupport.Syntax.PScript
{
    public sealed class PScriptString : PScriptData
    {
        public PScriptString(string source)
            : base(source)
        {
            String pattern = "\"([^\\\"\\\\]|\\\")*\"";
            var match = Regex.Match(_source, pattern);
            if (match.Value.Length != _source.Length)
            {
                throw new SyntaxException(String.Format("'{0}' is invalid string.", _source), SyntaxErrorType.SYNTAX_ERROR_DATA_INVALID);
            }
            _data = _source.Substring(1, _source.Length - 2);
            foreach (Char ch in _data)
            {
                if (StringCodes.GetInstance().Encode(ch) == null)
                {
                    throw new SyntaxException(String.Format("'{0}' is not a valid character.", ch), SyntaxErrorType.SYNTAX_ERROR_DATA_INVALID);
                }
            }
            _dataType = PScriptDataType.String;
            _value = _data;
        }

        protected override void Compile(SyntaxContext context, BinaryWriter writer)
        {
            foreach (Char ch in _data)
            {
                writer.Write(StringCodes.GetInstance().Encode(ch));
            }
        }

        private String _data;
    }
}
