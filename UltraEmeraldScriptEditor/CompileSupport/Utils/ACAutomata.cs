using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompileSupport.Utils
{
    /// <summary>
    /// Aho-Corasick自动机。
    /// </summary>
    public sealed class ACAutomata
    {
        public ACAutomata()
        {
            _trie = new Trie();
        }

        public Boolean MatchLiteral(String literal)
        {
            return _trie.IsMatch(literal);
        }

        public void AddLiteral(String literal)
        {
            _trie.Add(literal);
        }

        public void AddLiterals(IEnumerable<String> literals)
        {
            _trie.AddRange(literals);
        }

        public void RemoveLiteral(String literal)
        {
            _trie.Remove(literal);
        }

        public void RemoveLiterals(IEnumerable<String> literals)
        {
            _trie.RemoveRange(literals);
        }

        public void ClearLiterals()
        {
            _trie.Clear();
        }

        private Trie _trie;
    }
}
