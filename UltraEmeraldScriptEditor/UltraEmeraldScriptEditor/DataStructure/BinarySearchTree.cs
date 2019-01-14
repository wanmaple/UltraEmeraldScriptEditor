using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace UltraEmeraldScriptEditor.DataStructure
{
    /// <summary>
    /// 二叉搜索树
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BinarySearchTree<T> : ICollection<T>
        where T : IComparable
    {
        private class TreeNode : ITreeNode<T>
        {
            public Boolean IsLeftChild
            {
                get
                {
                    return Parent != null && Parent.LeftChild == this;
                }
            }
            public Boolean IsRightChild
            {
                get
                {
                    return Parent != null && Parent.RightChild == this;
                }
            }
            public T Data { get; set; }
            public ITreeNode<T> Parent { get; set; }
            public ITreeNode<T> LeftChild { get; set; }
            public ITreeNode<T> RightChild { get; set; }
        }

        #region CONSTRUCTOR
        public BinarySearchTree(Boolean allowDuplication = false)
        {
            _size = 0;
            _allowDuplication = allowDuplication;
        }
        public BinarySearchTree(IEnumerable<T> elements, Boolean allowDuplication = false)
            : this(allowDuplication)
        {
            foreach (var elem in elements)
            {
                Add(elem);
            }
        } 
        #endregion

        #region ICollection<T>
        public int Count
        {
            get { return _size; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void Add(T item)
        {
            var newNode = new TreeNode
            {
                Data = item,
            };
            if (_root == null)
            {
                _root = newNode;
            }
            else
            {
                ITreeNode<T> curNode = _root;
                while (true)
                {
                    if (!_allowDuplication && item.CompareTo(curNode.Data) == 0)
                    {
                        return;
                    }
                    Boolean precedes = item.CompareTo(curNode.Data) < 0;
                    if (precedes)
                    {
                        if (curNode.LeftChild != null)
                        {
                            curNode = curNode.LeftChild;
                        }
                        else
                        {
                            curNode.LeftChild = newNode;
                            newNode.Parent = curNode;
                            break;
                        }
                    }
                    else
                    {
                        if (curNode.RightChild != null)
                        {
                            curNode = curNode.RightChild;
                        }
                        else
                        {
                            curNode.RightChild = newNode;
                            newNode.Parent = curNode;
                            break;
                        }
                    }
                }
            }
            ++_size;
        }

        public void Clear()
        {
            _root = null;
            _size = 0;
        }

        public bool Contains(T item)
        {
            if (_root == null)
            {
                return false;
            }

            var curNode = _root;
            while (true)
            {
                if (item.CompareTo(curNode.Data) == 0)
                {
                    return true;
                }
                Boolean precedes = item.CompareTo(curNode.Data) < 0;
                if (precedes)
                {
                    if (curNode.LeftChild != null)
                    {
                        curNode = curNode.LeftChild;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (curNode.RightChild != null)
                    {
                        curNode = curNode.RightChild;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Int32 i = 0;
            foreach (var item in this)
            {
                array[i + arrayIndex] = item;
                ++i;
            }
        }

        public bool Remove(T item)
        {
            ITreeNode<T> curNode = _root;
            while (curNode != null)
            {
                if (item.CompareTo(curNode.Data) == 0)
                {
                    RemoveNode(curNode);
                    return true;
                }
                else if (item.CompareTo(curNode.Data) < 0)
                {
                    curNode = curNode.LeftChild;
                }
                else
                {
                    curNode = curNode.RightChild;
                }
            }
            return false;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new BinaryTreeEnumerator<T>(_root);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region PRIVATE
        private ITreeNode<T> FindPredecessor(ITreeNode<T> node)
        {
            // 存在左子树，则前驱节点就是左子树最靠右的叶子节点
            if (node.LeftChild != null)
            {
                node = node.LeftChild;
                while (node.RightChild != null)
                {
                    node = node.RightChild;
                }
                return node;
            }
            // 不存在左子树，前驱节点则为向上第一个左子树中不存在该节点的节点
            while (node.Parent != null && node.Parent.LeftChild == node)
            {
                node = node.Parent;
            }
            return node.Parent;
        }

        private ITreeNode<T> FindSuccessor(ITreeNode<T> node)
        {
            // 存在右子树，则后继节点就是右子树最靠左的叶子节点
            if (node.RightChild != null)
            {
                node = node.RightChild;
                while (node.LeftChild != null)
                {
                    node = node.LeftChild;
                }
                return node;
            }
            // 不存在右子树，后继节点则为向上第一个右子树中不能存在该节点的节点
            while (node.Parent != null && node.Parent.RightChild == node)
            {
                node = node.Parent;
            }
            return node.Parent;
        }

        private void RemoveNode(ITreeNode<T> node)
        {
            // 替换node的节点
            ITreeNode<T> toReplace = null;
             if (node.LeftChild == null && node.RightChild == null)
            {
                // 直接删除
                toReplace = null;
                if (node.Parent == null)
                {
                    _root = toReplace;
                }
                else if (node.IsLeftChild)
                {
                    node.Parent.LeftChild = toReplace;
                }
                else
                {
                    node.Parent.RightChild = toReplace;
                }
            }
            else if (node.LeftChild == null || node.RightChild == null)
            {
                // 用唯一的子节点替换该节点
                toReplace = node.LeftChild != null ? node.LeftChild : node.RightChild;
                if (node.Parent == null)
                {
                    _root = toReplace;
                }
                else if (node.IsLeftChild)
                {
                    node.Parent.LeftChild = toReplace;
                }
                else
                {
                    node.Parent.RightChild = toReplace;
                }
                toReplace.Parent = node.Parent;
            }
            else
            {
                // 用后继节点替换该节点
                // 因为有右子树所以successor不可能为空
                toReplace = FindSuccessor(node);
                // 该后继节点只可能有右孩子，所以将右孩子和父节点连接。
                if (toReplace.IsLeftChild)
                {
                    toReplace.Parent.LeftChild = toReplace.RightChild;
                }
                else
                {
                    toReplace.Parent.RightChild = toReplace.RightChild;
                }
                if (toReplace.RightChild != null)
                {
                    toReplace.RightChild.Parent = toReplace.Parent;
                }
                // 情况过于复杂，所以只替换数据
                node.Data = toReplace.Data;
            }
            --_size;
        }

        private ITreeNode<T> _root;
        private Int32 _size;
        private Boolean _allowDuplication;
        #endregion

        #region FOR DEBUG
        [Conditional("DEBUG")]
        /// <summary>
        /// 层次遍历
        /// </summary>
        /// <param name="action"></param>
        public void LevelTraversal(Action<ITreeNode<T>> action)
        {
            if (_root == null || action == null)
            {
                return;
            }
            // FIFO特性
            var nodeQueue = new Queue<ITreeNode<T>>();
            nodeQueue.Enqueue(_root);
            while (nodeQueue.Count > 0)
            {
                var curNode = nodeQueue.Dequeue();
                action(curNode);
                if (curNode.LeftChild != null)
                {
                    nodeQueue.Enqueue(curNode.LeftChild);
                }
                if (curNode.RightChild != null)
                {
                    nodeQueue.Enqueue(curNode.RightChild);
                }
            }
        }

        [Conditional("DEBUG")]
        public void CheckValid()
        {
            Int32 size = _size;
            Int32 chkSize = 0;
            LevelTraversal(node =>
            {
                if (node.LeftChild != null && node.LeftChild.Data.CompareTo(node.Data) >= 0)
                {
                    throw new Exception("Invalid BSTree");
                }
                else if (node.RightChild != null && node.RightChild.Data.CompareTo(node.Data) < 0)
                {
                    throw new Exception("Invalid BSTree");
                }
                if (node.Parent == null && node != _root)
                {
                    throw new Exception("Invalid BSTree");
                }
                if (node.Parent != null && node.Parent.LeftChild != node && node.Parent.RightChild != node)
                {
                    throw new Exception("Invalid BSTree");
                }
                ++chkSize;
            });
            if (chkSize != size)
            {
                throw new Exception("Invalid BSTree");
            }
        }
        public String Draw()
        {
            var sb = new StringBuilder();
            Int32 lastLine = 0;
            LevelTraversal(node =>
            {
                Int32 line = 0;
                var cur = node;
                while (cur != null)
                {
                    ++line;
                    cur = cur.Parent;
                }
                if (line != lastLine)
                {
                    sb.AppendLine();
                    lastLine = line;
                }
                sb.Append(node.Data).Append(" ");
            });
            return sb.ToString();
        }
        #endregion
    }
}
