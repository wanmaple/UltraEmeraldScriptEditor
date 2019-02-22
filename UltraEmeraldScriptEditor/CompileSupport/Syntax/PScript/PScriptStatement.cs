using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace CompileSupport.Syntax.PScript
{
    public sealed class PScriptStatement : IStatement<PScriptNode>
    {
        #region IStatement
        public ReadOnlyCollection<PScriptNode> Nodes => _nodes.AsReadOnly();
        #endregion

        public PScriptKeyword Keyword => _nodes[0] as PScriptKeyword;
        public List<PScriptNode> Arguments => _arguments;

        public PScriptStatement(PScriptKeyword keyword, params PScriptNode[] arguments)
        {
            _nodes = new List<PScriptNode>();
            _nodes.Add(keyword ?? throw new ArgumentNullException("keyword"));
            _nodes.AddRange(arguments);
            _arguments = new List<PScriptNode>(arguments);
        }

        private List<PScriptNode> _nodes;
        private List<PScriptNode> _arguments;
    }
}
