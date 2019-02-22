using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompileSupport.Syntax
{
    public interface ISyntaxNode
    {
        String Source { get; }
        Object Value { get; }
    }
}
