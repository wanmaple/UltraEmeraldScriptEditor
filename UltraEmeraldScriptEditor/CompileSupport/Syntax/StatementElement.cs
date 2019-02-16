using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompileSupport.Syntax
{
    public enum StatementElementType : Byte
    {
        None = 0,
        Keyword,
        Function,
        Variable,
        Constant,
    }

    public abstract class StatementElement
    {
        #region Abstraction
        public abstract String Content { get; }
        public abstract StatementElementType Type { get; }
        public abstract Object Value { get; } 
        #endregion
    }
}
