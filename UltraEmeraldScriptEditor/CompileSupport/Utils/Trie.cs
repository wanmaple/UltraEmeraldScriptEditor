using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CompileSupport.Utils
{
    /// <summary>
    /// Aho-Corasick多模式匹配算法用到的Trie树。
    /// </summary>
    internal sealed class Trie
    {
        internal class TrieNode
        {
            /// <summary>
            /// 子节点
            /// </summary>
            internal SortedDictionary<Char, TrieNode> Children => _children;
            /// <summary>
            /// 父节点
            /// </summary>
            internal TrieNode Parent { get; set; }
            /// <summary>
            /// 失败节点
            /// </summary>
            internal TrieNode Failure { get; set; }
            /// <summary>
            /// 存储的字符
            /// </summary>
            internal Char Character { get; set; }
            /// <summary>
            /// 可接收状态
            /// </summary>
            internal Boolean Accepted { get; set; }

            internal TrieNode(Char ch)
            {
                Character = ch;
                Accepted = false;
                _children = new SortedDictionary<char, TrieNode>();
            }

            public override string ToString()
            {
                if (Parent == null)
                {
                    return "root";
                }
                return Character.ToString() + ":" + Accepted.ToString();
            }

            private SortedDictionary<Char, TrieNode> _children;
        }

        public Trie()
            : this(new List<String>())
        {
        }

        public Trie(IEnumerable<String> literals)
        {
            if (literals == null)
            {
                throw new ArgumentNullException("literals");
            }
            // 根节点不存储实际字符，只作为失败节点处理。
            _root = new TrieNode('\u0000');
            _literals = new List<string>();
            AddRange(literals);
        }

        public Boolean IsMatch(String input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (input.Length <= 0)
            {
                return false;
            }
            Int32 idx = 0;
            TrieNode curNode = _root;
            Boolean ret = false;
            while (true)
            {
                TrieNode matchNode = curNode.Children.ContainsKey(input[idx]) ? curNode.Children[input[idx]] : null;
                if (matchNode != null)
                {
                    ++idx;
                    curNode = matchNode;
                }
                else
                {
                    curNode = curNode.Failure;
                    if (curNode == _root)
                    {
                        ret = false;
                        break;
                    }
                }
                if (idx == input.Length && matchNode.Accepted)
                {
                    ret = true;
                    break;
                }
            }
            return ret;
        }

        public void Add(String literal)
        {
            AddRange(new String[] { literal, });
        }

        /// <summary>
        /// 效率为M Log(N)
        /// </summary>
        /// <param name="literals"></param>
        public void AddRange(IEnumerable<String> literals)
        {
            literals = literals.TakeWhile(literal => !_literals.Contains(literal));
            if (literals.Count() > 0)
            {
                foreach (var literal in literals)
                {
                    if (String.IsNullOrEmpty(literal))
                    {
                        continue;
                    }
                    TrieNode curNode = null;
                    curNode = _root;
                    for (int j = 0; j < literal.Length; j++)
                    {
                        TrieNode child = curNode.Children.ContainsKey(literal[j]) ? curNode.Children[literal[j]] : null;
                        if (child == null)
                        {
                            child = new TrieNode(literal[j]);
                            child.Parent = curNode;
                            curNode.Children.Add(literal[j], child);
                        }
                        curNode = child;
                        if (j == literal.Length - 1)
                        {
                            child.Accepted = true;
                        }
                    }
                }
                _literals.AddRange(literals);
                RebuildFailures();
            }
        }

        public void Remove(String literal)
        {
            RemoveRange(new String[] { literal, });
        }

        /// <summary>
        /// 效率为M Log(N)
        /// </summary>
        /// <param name="literals"></param>
        public void RemoveRange(IEnumerable<String> literals)
        {
            literals = literals.TakeWhile(literal => _literals.Contains(literal));
            if (literals.Count() > 0)
            {
                foreach (var literal in literals)
                {
                    if (String.IsNullOrEmpty(literal))
                    {
                        continue;
                    }
                    Int32 idx = 0;
                    TrieNode curNode = _root;
                    TrieNode nodeToRemove = null;
                    while (idx < literal.Length)
                    {
                        TrieNode matchNode = curNode.Children.ContainsKey(literal[idx]) ? curNode.Children[literal[idx]] : null;
                        Debug.Assert(matchNode != null);
                        if (nodeToRemove == null && matchNode.Children.Count <= 1)
                        {
                            nodeToRemove = matchNode;
                        }
                        else if (nodeToRemove != null && matchNode.Children.Count > 1)
                        {
                            nodeToRemove = null;
                        }
                        curNode = matchNode;
                        ++idx;
                    }
                    if (nodeToRemove != null)
                    {
                        var parent = nodeToRemove.Parent;
                        parent.Children.Remove(nodeToRemove.Character);
                    }
                }
                _literals.RemoveAll(literal => literals.Contains(literal));
                RebuildFailures();
            }
        }

        public void Clear()
        {
            _root.Children.Clear();
            _literals.Clear();
        }

        private void RebuildFailures()
        {
            TrieNode curNode = null;
            // 构造失败路径，使用层次遍历
            var nodeQueue = new Queue<TrieNode>();
            nodeQueue.Enqueue(_root);
            while (nodeQueue.Count > 0)
            {
                curNode = nodeQueue.Dequeue();
                foreach (var pair in curNode.Children)
                {
                    TrieNode child = pair.Value;
                    nodeQueue.Enqueue(child);
                    if (child.Parent == _root)
                    {
                        // 如果是第二层节点，失败节点直接指向root
                        child.Failure = _root;
                    }
                    else
                    {
                        // 沿着父节点的失败节点找，如果找到一个节点，它的子节点中也存在相同字符的节点，将失败节点指向它
                        TrieNode tmp = curNode.Failure;
                        while (tmp != null)
                        {
                            TrieNode failure = tmp.Children.ContainsKey(child.Character) ? tmp.Children[child.Character] : null;
                            if (failure != null)
                            {
                                child.Failure = failure;
                                break;
                            }
                            tmp = tmp.Failure;
                        }
                        if (child.Failure == null)
                        {
                            // 没找到失败节点，指向root
                            child.Failure = _root;
                        }
                    }
                }
            }
        }

        private List<String> _literals;
        private TrieNode _root;
    }
}
