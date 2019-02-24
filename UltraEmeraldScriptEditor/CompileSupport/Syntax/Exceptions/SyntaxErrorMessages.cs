using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompileSupport.Syntax.Exceptions
{
    public static class SyntaxErrorMessages
    {
        // 数据类型解析错误
        public static readonly String InvalidByteValue = "'{0}' is invalid byte value.";
        public static readonly String InvalidHalfwordValue = "'{0}' is invalid halfword value.";
        public static readonly String InvalidWordValue = "'{0}' is invalid word value.";
        public static readonly String InvalidStringValue = "'{0}' is invalid string.";
        public static readonly String InvalidCharacterValue = "'{0}' is not a valid character.";
        // 语法检查错误
        public static readonly String CheckArgumentCountNotMatch = "Argument count doesn't match the macro. Exactly {0}, expecting {1}.";
        public static readonly String CheckParameterIndexInvalid = "Argument index {0} is invalid.";
        public static readonly String CheckNotCompileable = "{0} is not compileable.";
    }
}
