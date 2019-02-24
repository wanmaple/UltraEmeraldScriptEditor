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
    /// <summary>
    /// 字符串数据（读取码表）。
    /// </summary>
    public sealed class PScriptString : PScriptToken
    {
        public PScriptString(string source)
            : base(source)
        {
            String pattern = "\"([^\\\"\\\\]|\\\")*\"";
            var match = Regex.Match(_source, pattern);
            if (match.Value.Length != _source.Length)
            {
                throw new FormatException(String.Format(SyntaxErrorMessages.InvalidStringValue, _source));
            }
            _data = _source.Substring(1, _source.Length - 2);
            foreach (Char ch in _data)
            {
                if (StringCodes.GetInstance().Encode(ch) == null)
                {
                    throw new FormatException(String.Format(SyntaxErrorMessages.InvalidCharacterValue, ch));
                }
            }
        }

        #region Overrides
        public override bool Compileable => true;

        protected override void Compile(ISyntaxContext context, BinaryWriter writer)
        {
            foreach (Char ch in _data)
            {
                writer.Write(StringCodes.GetInstance().Encode(ch));
            }
        }

        public override string ToString()
        {
            return _data;
        }
        #endregion

        private String _data;
    }
}
