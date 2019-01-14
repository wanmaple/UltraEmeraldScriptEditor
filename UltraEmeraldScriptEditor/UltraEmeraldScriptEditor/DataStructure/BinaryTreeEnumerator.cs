using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UltraEmeraldScriptEditor.DataStructure
{
    /// <summary>
    /// 二叉搜索树的遍历器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BinaryTreeEnumerator<T> : IEnumerator<T>
    {
        private enum EnumAction
        {
            End,
            Right,
            Parent,
        }

        public BinaryTreeEnumerator(ITreeNode<T> root)
        {
            _root = root;
            Reset();
        }

        #region IEnumerator<T>
        public T Current
        {
            get
            {
                return _current.Data;
            }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            switch (_action)
            {
                case EnumAction.End:
                    break;
                case EnumAction.Right:
                    _current = _right;
                    while (_current.LeftChild != null)
                    {
                        _current = _current.LeftChild;
                    }
                    _right = _current.RightChild;
                    _action = _right == null ? EnumAction.Parent : EnumAction.Right;
                    return true;
                case EnumAction.Parent:
                    while (_current.Parent != null)
                    {
                        var prev = _current;
                        _current = _current.Parent;
                        if (_current.LeftChild == prev)
                        {
                            _right = _current.RightChild;
                            _action = _right == null ? EnumAction.Parent : EnumAction.Right;
                            return true;
                        }
                    }
                    _action = EnumAction.End;
                    break;
                default:
                    break;
            }
            return false;
        }

        public void Reset()
        {
            _right = _root;
            _action = _root == null ? EnumAction.End : EnumAction.Right;
        }
        #endregion

        private ITreeNode<T> _current;
        private ITreeNode<T> _root;
        private ITreeNode<T> _right;
        private EnumAction _action;
    }
}
