using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompileSupport.Syntax.Exceptions
{
    public enum SyntaxErrorType
    {
        SYNTAX_ERROR_UNKNOWN = 10000,
        SYNTAX_ERROR_DATA_INVALID = 10001,
    }

    public class SyntaxException : Exception
    {
        public SyntaxErrorType ErrorType => _errorType;

        public SyntaxException(string message, SyntaxErrorType errorType, Exception innerException = null) 
            : base(message, innerException)
        {
            _errorType = errorType;
        }

        protected SyntaxErrorType _errorType;
    }
}
