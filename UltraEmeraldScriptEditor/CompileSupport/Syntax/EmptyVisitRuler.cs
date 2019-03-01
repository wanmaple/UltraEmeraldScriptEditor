using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CompileSupport.Syntax
{
    public sealed class EmptyVisitRuler : IVisitRuler
    {
        public int ParamIndex => -1;

        public bool Override => false;

        public void Visit(ISyntaxContext context, BinaryWriter writer)
        {
        }

        public void Postvisit(ISyntaxContext context, BinaryWriter writer)
        {
        }

        public void Previsit(ISyntaxContext context, BinaryWriter writer)
        {
        }
    }
}
