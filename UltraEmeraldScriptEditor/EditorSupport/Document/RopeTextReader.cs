using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EditorSupport.Document
{
    public sealed class RopeTextReader : TextReader
    {
        public RopeTextReader(Rope<Char> rope)
            : base()
        {
            if (rope == null)
            {
                throw new ArgumentNullException("rope");
            }
            
            // 我们克隆一份rope，防止遍历中修改rope
            _rope = rope.Clone() as Rope<Char>;
            _nodeStack = new Stack<Rope<char>.RopeNode>();
            _indexInsideNode = 0;
            _totalRead = 0;
            if (_rope.Count > 0)
            {
                _currentNode = rope._root;
                GotoLeftMost();
            }
            else
            {
                _currentNode = null;
            }
        }

        #region Overrides
        public override int Peek()
        {
            if (_currentNode == null)
            {
                return -1;
            }
            return _currentNode._contents[_indexInsideNode];
        }

        public override int Read()
        {
            if (_currentNode == null)
            {
                return -1;
            }
            Char ret = _currentNode._contents[_indexInsideNode++];
            if (_indexInsideNode >= _currentNode.Length)
            {
                GotoNextNode();
            }
            ++_totalRead;
            return ret;
        }

        public override int Read(char[] buffer, int index, int count)
        {
            if (_currentNode == null)
            {
                return 0;
            }
            int sizeInCurrentNode = _currentNode.Length - _indexInsideNode;
            if (count < sizeInCurrentNode)
            {
                Array.Copy(_currentNode._contents, _indexInsideNode, buffer, index, count);
                _indexInsideNode += count;
                _totalRead += count;
                return count;
            }
            // 只读到当前节点的末尾
            Array.Copy(_currentNode._contents, _indexInsideNode, buffer, index, sizeInCurrentNode);
            GotoNextNode();
            _totalRead += sizeInCurrentNode;
            return sizeInCurrentNode;
        }

        public override int ReadBlock(char[] buffer, int index, int count)
        {
            return Read(buffer, index, count);
        }

        public override string ReadLine()
        {
            if (_currentNode == null)
            {
                return String.Empty;
            }
            Int32 restSize = _rope.Count - _totalRead;
            Char[] buffer = new Char[restSize];
            Int32 startIndex = 0;
            Char readChar;
            while ((readChar = (Char)Read()) != '\n')
            {
                buffer[startIndex] = readChar;
                ++startIndex;
            }
            return new String(buffer);
        }

        public override string ReadToEnd()
        {
            if (_currentNode == null)
            {
                return String.Empty;
            }
            Int32 restSize = _rope.Count - _totalRead;
            Char[] buffer = new Char[restSize];
            Int32 startIndex = 0;
            Int32 read = 0;
            while ((read = Read(buffer, startIndex, restSize)) > 0)
            {
                startIndex += read;
                restSize -= read;
            }
            return new String(buffer);
        }
        #endregion

        private void GotoLeftMost()
        {
            while (_currentNode != null)
            {
                if (_currentNode.Right != null)
                {
                    _nodeStack.Push(_currentNode.Right);
                }
                _currentNode = _currentNode.Left;
            }
        }

        private void GotoNextNode()
        {
            if (_nodeStack.Count == 0)
            {
                _currentNode = null;
                return;
            }
            _indexInsideNode = 0;
            _currentNode = _nodeStack.Peek();
            GotoLeftMost();
        }

        private Rope<Char> _rope;
        private Int32 _indexInsideNode;
        private Rope<Char>.RopeNode _currentNode;
        private Stack<Rope<Char>.RopeNode> _nodeStack;
        private Int32 _totalRead;
    }
}
