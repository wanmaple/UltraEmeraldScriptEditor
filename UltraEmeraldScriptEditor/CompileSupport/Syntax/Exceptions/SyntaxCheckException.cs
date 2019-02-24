using EditorSupport.Document;
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
        SYNTAX_ERROR_ARGUMENTS = 10002,
        SYNTAX_ERROR_NOT_COMPILEABLE = 10003,
    }

    public class SyntaxCheckException : Exception
    {
        public TextDocument Document => _doc;
        public String ErrorContent => _doc.GetTextAt(_errorOffset, _errorLength);
        public SyntaxErrorType ErrorType => _errorType;

        public SyntaxCheckException(String message, SyntaxErrorType errorType, TextDocument document, Int32 offset, Int32 length, Exception innerException = null)
            : base(message, innerException)
        {
            _errorType = errorType;
            _doc = document ?? throw new ArgumentNullException("document");
            _errorOffset = offset;
            _errorLength = length;
        }

        protected SyntaxErrorType _errorType;
        protected TextDocument _doc;
        protected Int32 _errorOffset;
        protected Int32 _errorLength;
    }
}
