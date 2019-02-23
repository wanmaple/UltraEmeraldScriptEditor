using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompileSupport.Syntax
{
    public interface IStatement<T> where T : ISyntaxToken
    {
        SyntaxContext Context { get; }
        ReadOnlyCollection<T> Tokens { get; }

        void Compile(BinaryWriter writer);
    }
}
